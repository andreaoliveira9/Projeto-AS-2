import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { setupAuth, auth } from './auth.js';

// Custom metrics
const workflowTransitionDuration = new Trend('workflow_transition_duration', true);
const workflowStateChanges = new Counter('workflow_state_changes');
const workflowErrors = new Counter('workflow_errors');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// Test configuration
export const options = {
    scenarios: {
        workflow_load_test: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 5 },
                { duration: '1m', target: 10 },
                { duration: '2m', target: 20 },
                { duration: '1m', target: 10 },
                { duration: '30s', target: 0 }
            ],
            gracefulRampDown: '30s'
        }
    },
    thresholds: {
        'http_req_duration': ['p(95)<2000', 'p(99)<3000'],
        'http_req_failed': ['rate<0.1'],
        'workflow_transition_duration': ['p(95)<2500', 'p(99)<4000'],
        'workflow_errors': ['count<10']
    },
    ext: {
        loadimpact: {
            projectID: 3478725,
            name: 'Piranha CMS Workflow Load Test'
        }
    }
};

// Setup function - runs once per VU
export function setup() {
    return setupAuth();
}

// Main test function
export default function (data) {
    const authData = data;

    group('Workflow Operations', () => {
        // Test 0: Trigger custom metrics for demonstration
        group('Trigger Custom Metrics', () => {
            const res = http.post(`${BASE_URL}/api/metricstest/trigger-all-metrics`);
            
            check(res, {
                'metrics triggered': (r) => r.status === 200
            });
        });

        sleep(1);

        // Test 1: Get workflow definitions
        group('Get Workflow Definitions', () => {
            const res = auth.makeRequest('GET', `${BASE_URL}/api/workflow/definitions`, null, true);
            
            check(res, {
                'status is 200': (r) => r.status === 200,
                'response has definitions': (r) => {
                    const body = JSON.parse(r.body);
                    return Array.isArray(body) && body.length > 0;
                }
            });

            if (res.status !== 200) {
                workflowErrors.add(1);
            }
        });

        sleep(1);

        // Test 2: Get workflow states
        group('Get Workflow States', () => {
            // First get definitions to get a definition ID
            const defRes = auth.makeRequest('GET', `${BASE_URL}/api/workflow/definitions`, null, true);
            let definitionId = null;
            if (defRes.status === 200) {
                const definitions = JSON.parse(defRes.body);
                if (definitions.length > 0) {
                    definitionId = definitions[0].id;
                }
            }
            
            if (!definitionId) {
                console.log('No workflow definitions found, skipping states test');
                return;
            }
            
            const res = auth.makeRequest('GET', `${BASE_URL}/api/workflow/definitions/${definitionId}/states`, null, true);
            
            check(res, {
                'status is 200': (r) => r.status === 200,
                'response has states': (r) => {
                    const body = JSON.parse(r.body);
                    return Array.isArray(body) && body.length > 0;
                }
            });

            if (res.status !== 200) {
                workflowErrors.add(1);
            }
        });

        sleep(1);

        // Test 3: Create workflow instance
        group('Create Workflow Instance', () => {
            const workflowData = {
                workflowDefinitionId: 'default-workflow',
                contentId: `test-content-${__VU}-${__ITER}`,
                contentType: 'Piranha.Models.PageBase',
                currentState: 'Draft'
            };

            const startTime = new Date().getTime();
            const res = auth.makeRequest('POST', `${BASE_URL}/api/workflow/instances`, workflowData, true);
            const endTime = new Date().getTime();

            check(res, {
                'instance created': (r) => r.status === 200 || r.status === 201,
                'response has instance ID': (r) => {
                    if (r.status === 200 || r.status === 201) {
                        const body = JSON.parse(r.body);
                        return body.id !== undefined;
                    }
                    return false;
                }
            });

            if (res.status === 200 || res.status === 201) {
                workflowTransitionDuration.add(endTime - startTime);
                return JSON.parse(res.body);
            } else {
                workflowErrors.add(1);
                return null;
            }
        });

        sleep(2);

        // Test 4: Workflow state transitions
        group('Workflow State Transitions', () => {
            // Create a test page first
            const pageData = {
                title: `Test Page ${__VU}-${__ITER}`,
                slug: `test-page-${__VU}-${__ITER}`,
                typeId: 'StandardPage',
                siteId: 'default'
            };

            const pageRes = auth.makeRequest('POST', `${BASE_URL}/api/pages`, pageData, true);
            
            if (pageRes.status === 200 || pageRes.status === 201) {
                const page = JSON.parse(pageRes.body);
                
                // Transition workflow states
                const transitions = [
                    { from: 'Draft', to: 'Review' },
                    { from: 'Review', to: 'Approved' },
                    { from: 'Approved', to: 'Published' }
                ];

                transitions.forEach(transition => {
                    sleep(1);
                    
                    const transitionData = {
                        instanceId: page.id, // Assuming we have instance ID
                        toState: transition.to
                    };

                    const startTime = new Date().getTime();
                    const res = auth.makeRequest('POST', `${BASE_URL}/api/workflow/workflow-instances/${page.id}/transition`, transitionData, true);
                    const endTime = new Date().getTime();

                    check(res, {
                        [`transition ${transition.from} -> ${transition.to} successful`]: (r) => r.status === 200
                    });

                    if (res.status === 200) {
                        workflowTransitionDuration.add(endTime - startTime);
                        workflowStateChanges.add(1);
                    } else {
                        workflowErrors.add(1);
                    }
                });
            }
        });

        sleep(2);

        // Test 5: Get workflow instances
        group('Get Workflow Instances', () => {
            const res = auth.makeRequest('GET', `${BASE_URL}/api/workflow/instances`, null, true);
            
            check(res, {
                'status is 200': (r) => r.status === 200,
                'response has history': (r) => {
                    if (r.status === 200) {
                        const body = JSON.parse(r.body);
                        return Array.isArray(body);
                    }
                    return false;
                }
            });

            if (res.status !== 200) {
                workflowErrors.add(1);
            }
        });
    });

    sleep(think_time());
}

// Helper function for think time
function think_time() {
    return Math.random() * 2 + 1; // Random think time between 1-3 seconds
}

// Teardown function - runs once at the end
export function teardown(data) {
    console.log('Workflow load test completed');
}