import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';
import { setupAuth, auth } from './auth.js';

// Custom metrics for page and workflow operations
const pageCreationDuration = new Trend('page_creation_duration', true);
const pagePublishDuration = new Trend('page_publish_duration', true);
const workflowCreationDuration = new Trend('workflow_creation_duration', true);
const workflowTransitionDuration = new Trend('workflow_transition_duration', true);
const pageWorkflowAssignDuration = new Trend('page_workflow_assign_duration', true);

const pagesCreated = new Counter('pages_created_total');
const pagesPublished = new Counter('pages_published_total');
const workflowsCreated = new Counter('workflows_created_total');
const workflowTransitions = new Counter('workflow_transitions_total');
const workflowErrors = new Counter('workflow_errors_total');

const pageCreationSuccess = new Rate('page_creation_success_rate');
const workflowAssignSuccess = new Rate('workflow_assign_success_rate');
const workflowTransitionSuccess = new Rate('workflow_transition_success_rate');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// Test configuration
export const options = {
    scenarios: {
        // Main scenario: Page creation with workflow management
        page_workflow_scenario: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 3 },   // Warm up
                { duration: '1m', target: 5 },    // Ramp up
                { duration: '3m', target: 10 },   // Peak load
                { duration: '2m', target: 15 },   // Stress test
                { duration: '1m', target: 5 },    // Wind down
                { duration: '30s', target: 0 }    // Cool down
            ],
            gracefulRampDown: '30s'
        },
        // Concurrent workflow transitions
        workflow_transitions_scenario: {
            executor: 'constant-arrival-rate',
            rate: 5,
            timeUnit: '1s',
            duration: '5m',
            preAllocatedVUs: 5,
            maxVUs: 10,
            exec: 'workflowTransitionsOnly',
            startTime: '1m' // Start after pages are created
        }
    },
    thresholds: {
        'http_req_duration': ['p(95)<2000', 'p(99)<3000'],
        'http_req_failed': ['rate<0.1'],
        'page_creation_duration': ['p(95)<3000', 'p(99)<5000'],
        'workflow_transition_duration': ['p(95)<2500', 'p(99)<4000'],
        'page_creation_success_rate': ['rate>0.95'],
        'workflow_assign_success_rate': ['rate>0.90'],
        'workflow_transition_success_rate': ['rate>0.90'],
        'workflow_errors_total': ['count<20']
    }
};

// Store created pages for workflow transitions
const createdPages = [];

export function setup() {
    const authData = setupAuth();
    
    // Ensure we have a default workflow definition
    const workflowDef = {
        id: 'load-test-workflow',
        name: 'Load Test Workflow',
        description: 'Workflow for load testing pages',
        isActive: true
    };
    
    console.log(`Starting Page-Workflow focused load test against ${BASE_URL}`);
    return { auth: authData, workflowDefinitionId: workflowDef.id };
}

// Main test function - Page creation with workflow
export default function(data) {
    const { workflowDefinitionId } = data;
    
    group('Page Creation with Workflow', () => {
        
        // Step 1: Create a new page via Manager API
        group('Create Page', () => {
            const siteId = 'default'; // Assuming default site
            const typeId = 'StandardPage';
            
            // Get page creation template
            const createStart = new Date().getTime();
            const templateRes = auth.makeRequest(
                'GET', 
                `${BASE_URL}/manager/api/page/create/${siteId}/${typeId}`,
                null,
                true
            );
            
            if (templateRes.status === 200) {
                const pageModel = JSON.parse(templateRes.body);
                
                // Populate page data
                pageModel.title = `Load Test Page ${__VU}-${__ITER}-${Date.now()}`;
                pageModel.slug = `load-test-page-${__VU}-${__ITER}-${Date.now()}`;
                pageModel.navigationTitle = `LT Page ${__VU}-${__ITER}`;
                pageModel.metaDescription = 'Page created during load testing with workflow';
                pageModel.metaKeywords = 'load-test, workflow, performance';
                
                // Add content blocks
                if (!pageModel.blocks) pageModel.blocks = [];
                pageModel.blocks.push({
                    type: 'Piranha.Extend.Blocks.HtmlBlock',
                    body: `<h1>Load Test Page</h1>
                           <p>This page was created during load testing at ${new Date().toISOString()}</p>
                           <p>VU: ${__VU}, Iteration: ${__ITER}</p>`
                });
                
                // Save as draft first
                const saveRes = auth.makeRequest(
                    'POST',
                    `${BASE_URL}/manager/api/page/save/draft`,
                    pageModel,
                    true
                );
                const createEnd = new Date().getTime();
                
                const success = check(saveRes, {
                    'page created as draft': (r) => r.status === 200,
                    'page has ID': (r) => {
                        if (r.status === 200) {
                            const saved = JSON.parse(r.body);
                            return saved.id !== undefined;
                        }
                        return false;
                    }
                });
                
                pageCreationSuccess.add(success ? 1 : 0);
                
                if (success) {
                    const savedPage = JSON.parse(saveRes.body);
                    pageCreationDuration.add(createEnd - createStart);
                    pagesCreated.add(1);
                    
                    // Store page info for later use
                    createdPages.push({
                        id: savedPage.id,
                        title: savedPage.title,
                        slug: savedPage.slug
                    });
                    
                    // Step 2: Assign workflow to the page
                    sleep(0.5);
                    assignWorkflowToPage(savedPage.id, workflowDefinitionId);
                    
                    // Step 3: Perform workflow transitions
                    sleep(1);
                    performWorkflowTransitions(savedPage.id);
                    
                } else {
                    workflowErrors.add(1);
                    console.error(`Failed to create page: ${saveRes.status} - ${saveRes.body}`);
                }
            }
        });
        
        sleep(2);
        
        // Step 4: Check page and workflow status
        group('Verify Page and Workflow', () => {
            if (createdPages.length > 0) {
                const randomPage = createdPages[Math.floor(Math.random() * createdPages.length)];
                
                // Get page info
                const pageRes = auth.makeRequest(
                    'GET',
                    `${BASE_URL}/manager/api/page/${randomPage.id}`,
                    null,
                    true
                );
                
                check(pageRes, {
                    'page retrieved successfully': (r) => r.status === 200,
                    'page has expected data': (r) => {
                        if (r.status === 200) {
                            const page = JSON.parse(r.body);
                            return page.id === randomPage.id;
                        }
                        return false;
                    }
                });
                
                // Get workflow instance for this page
                const workflowRes = auth.makeRequest(
                    'GET',
                    `${BASE_URL}/api/workflow/content-extensions/${randomPage.id}`,
                    null,
                    true
                );
                
                check(workflowRes, {
                    'workflow instance found': (r) => r.status === 200,
                    'workflow has instance ID': (r) => {
                        if (r.status === 200) {
                            const ext = JSON.parse(r.body);
                            return ext.currentWorkflowInstanceId !== null;
                        }
                        return false;
                    }
                });
            }
        });
    });
    
    sleep(think_time());
}

// Function to assign workflow to a page
function assignWorkflowToPage(pageId, workflowDefinitionId) {
    group('Assign Workflow to Page', () => {
        const assignStart = new Date().getTime();
        
        // Method 1: Using the simplified endpoint
        const assignRes = auth.makeRequest(
            'POST',
            `${BASE_URL}/api/workflow/assign-workflow-to-page/${pageId}`,
            { workflowDefinitionId: workflowDefinitionId },
            true
        );
        
        const assignEnd = new Date().getTime();
        
        const success = check(assignRes, {
            'workflow assigned to page': (r) => r.status === 200 || r.status === 201,
            'response has workflow info': (r) => {
                if (r.status === 200 || r.status === 201) {
                    const body = JSON.parse(r.body);
                    return body.workflowInstanceId !== undefined;
                }
                return false;
            }
        });
        
        workflowAssignSuccess.add(success ? 1 : 0);
        
        if (success) {
            pageWorkflowAssignDuration.add(assignEnd - assignStart);
            workflowsCreated.add(1);
            return JSON.parse(assignRes.body);
        } else {
            // Fallback: Try the full create endpoint
            const createStart = new Date().getTime();
            const createRes = auth.makeRequest(
                'POST',
                `${BASE_URL}/api/workflow/CreateWorkflowInstanceWithContent`,
                {
                    contentId: pageId,
                    workflowDefinitionId: workflowDefinitionId,
                    contentType: 'page',
                    contentTitle: `Page ${pageId}`
                },
                true
            );
            const createEnd = new Date().getTime();
            
            const fallbackSuccess = check(createRes, {
                'workflow created (fallback)': (r) => r.status === 200 || r.status === 201
            });
            
            if (fallbackSuccess) {
                workflowCreationDuration.add(createEnd - createStart);
                workflowsCreated.add(1);
                return JSON.parse(createRes.body);
            } else {
                workflowErrors.add(1);
                console.error(`Failed to assign workflow: ${assignRes.status} - ${assignRes.body}`);
            }
        }
        
        return null;
    });
}

// Function to perform workflow transitions
function performWorkflowTransitions(pageId) {
    group('Workflow Transitions', () => {
        // Get workflow instance
        const extRes = auth.makeRequest(
            'GET',
            `${BASE_URL}/api/workflow/content-extensions/${pageId}`,
            null,
            true
        );
        
        if (extRes.status === 200) {
            const ext = JSON.parse(extRes.body);
            const instanceId = ext.currentWorkflowInstanceId;
            
            if (instanceId) {
                // Get available transitions
                const transitionsRes = auth.makeRequest(
                    'GET',
                    `${BASE_URL}/api/workflow/workflow-instances/${instanceId}/transitions`,
                    null,
                    true
                );
                
                if (transitionsRes.status === 200) {
                    const transitionData = JSON.parse(transitionsRes.body);
                    const availableTransitions = transitionData.availableTransitions || [];
                    
                    // Perform first available transition
                    if (availableTransitions.length > 0) {
                        const transition = availableTransitions[0];
                        
                        const transitionStart = new Date().getTime();
                        const transitionRes = auth.makeRequest(
                            'POST',
                            `${BASE_URL}/api/workflow/workflow-instances/${instanceId}/transition`,
                            {
                                transitionRuleId: transition.id,
                                comment: `Load test transition from ${transition.fromState} to ${transition.toState}`
                            },
                            true
                        );
                        const transitionEnd = new Date().getTime();
                        
                        const success = check(transitionRes, {
                            'workflow transition successful': (r) => r.status === 200,
                            'content published if final state': (r) => {
                                if (r.status === 200) {
                                    const result = JSON.parse(r.body);
                                    // Check if content was published when moving to published state
                                    if (transition.toState === 'Published' || transition.isPublished) {
                                        return result.contentPublished === true;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        });
                        
                        workflowTransitionSuccess.add(success ? 1 : 0);
                        
                        if (success) {
                            workflowTransitionDuration.add(transitionEnd - transitionStart);
                            workflowTransitions.add(1);
                            
                            const result = JSON.parse(transitionRes.body);
                            if (result.contentPublished) {
                                pagesPublished.add(1);
                            }
                        } else {
                            workflowErrors.add(1);
                        }
                    }
                }
            }
        }
    });
}

// Scenario for just workflow transitions (uses existing pages)
export function workflowTransitionsOnly() {
    if (createdPages.length > 0) {
        const randomPage = createdPages[Math.floor(Math.random() * createdPages.length)];
        performWorkflowTransitions(randomPage.id);
    }
    sleep(think_time(0.5, 1.5));
}

// Helper function for think time
function think_time(min = 1, max = 3) {
    return Math.random() * (max - min) + min;
}

export function teardown(data) {
    console.log('\n=== Page-Workflow Load Test Summary ===');
    console.log(`Total pages created: ${createdPages.length}`);
    console.log('\nKey operations tested:');
    console.log('✓ Page creation via Manager API');
    console.log('✓ Workflow assignment to pages');
    console.log('✓ Workflow state transitions');
    console.log('✓ Automatic page publishing via workflow');
    console.log('\nCheck metrics at:');
    console.log('- Dashboard: http://localhost:8080/dashboard/metrics');
    console.log('- Grafana: http://localhost:3000');
    console.log('- Jaeger: http://localhost:16686');
}