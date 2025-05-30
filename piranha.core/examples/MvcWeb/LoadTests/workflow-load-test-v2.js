import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomItem, randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Test options
export const options = {
  stages: [
    { duration: '30s', target: 5 }  // Ramp up to 5 users
    // { duration: '2m', target: 5 },    // Stay at 5 users
    // { duration: '30s', target: 10 },  // Ramp up to 10 users
    // { duration: '1m', target: 10 },   // Stay at 10 users
    // { duration: '30s', target: 0 },   // Ramp down to 0
  ],
  thresholds: {
    http_req_failed: ['rate<0.1'], // http errors should be less than 10%
    http_req_duration: ['p(95)<1000'], // 95% of requests should be below 1s
    workflow_operations_successful: ['rate>0.8'], // 80% of workflow operations should succeed
  },
};

// Custom metric to track workflow operations
import { Counter } from 'k6/metrics';
const workflowOperations = new Counter('workflow_operations_successful');

// Test data generators
function generateWorkflowDefinition() {
  const timestamp = Date.now();
  const randomId = Math.random().toString(36).substring(7);
  return {
    name: `LoadTest-Workflow-${randomId}-${timestamp}`,
    description: `Workflow created during load testing at ${new Date().toISOString()}`,
    isActive: true,
    version: 1,
  };
}

function generateWorkflowState(workflowDefinitionId, stateType, sortOrder) {
  const states = {
    draft: {
      stateId: 'draft',
      name: 'Draft',
      description: 'Content is being drafted',
      colorCode: '#808080',
      isInitial: true,
      isPublished: false,
      isFinal: false,
    },
    review: {
      stateId: 'review',
      name: 'In Review',
      description: 'Content is under review',
      colorCode: '#FFA500',
      isInitial: false,
      isPublished: false,
      isFinal: false,
    },
    published: {
      stateId: 'published',
      name: 'Published',
      description: 'Content is published',
      colorCode: '#008000',
      isInitial: false,
      isPublished: true,
      isFinal: true,
    },
    rejected: {
      stateId: 'rejected',
      name: 'Rejected',
      description: 'Content was rejected',
      colorCode: '#FF0000',
      isInitial: false,
      isPublished: false,
      isFinal: true,
    },
  };
  
  const baseState = states[stateType];
  return {
    stateId: baseState.stateId,
    name: baseState.name,
    description: baseState.description,
    colorCode: baseState.colorCode,
    isInitial: baseState.isInitial,
    isPublished: baseState.isPublished,
    isFinal: baseState.isFinal,
    workflowDefinitionId: workflowDefinitionId,
    sortOrder: sortOrder,
  };
}

function generateTransitionRule(fromStateId, toStateId) {
  return {
    fromStateId: fromStateId,
    toStateId: toStateId,
    allowedRoles: JSON.stringify(['Admin', 'Editor', 'Author']),
    description: `Transition from ${fromStateId} to ${toStateId}`,
    requiresComment: false,
    isActive: true,
    sortOrder: 1,
  };
}

function generateWorkflowInstance(workflowDefinitionId) {
  const timestamp = Date.now();
  const randomId = Math.random().toString(36).substring(7);
  return {
    workflowDefinitionId: workflowDefinitionId,
    contentId: `loadtest-content-${randomId}-${timestamp}`,
    contentType: randomItem(['page', 'post', 'media']),
  };
}

// Main test scenario
export default function () {
  const headers = {
    'Content-Type': 'application/json',
  };
  
  // Test scenarios with different weights
  const scenario = randomItem([
    'create_complete_workflow',
    'test_existing_endpoints',
    'trigger_metrics_only',
    'stress_test_debug_endpoints'
  ]);
  
  switch (scenario) {
    case 'create_complete_workflow':
      createCompleteWorkflow(headers);
      break;
    case 'test_existing_endpoints':
      testExistingEndpoints(headers);
      break;
    case 'trigger_metrics_only':
      triggerMetricsOnly(headers);
      break;
    case 'stress_test_debug_endpoints':
      stressTestDebugEndpoints(headers);
      break;
  }
  
  sleep(randomIntBetween(1, 3)); // Random think time
}

function createCompleteWorkflow(headers) {
  console.log('Creating complete workflow...');
  
  // 1. Create workflow definition
  const workflowDef = generateWorkflowDefinition();
  const createDefRes = http.post(
    `${BASE_URL}/api/workflow/definitions`,
    JSON.stringify(workflowDef),
    { headers }
  );
  
  const defSuccess = check(createDefRes, {
    'workflow definition created': (r) => r.status === 201,
  });
  
  if (defSuccess) {
    workflowOperations.add(1);
    const createdWorkflow = JSON.parse(createDefRes.body);
    const workflowId = createdWorkflow.id;
    
    sleep(0.2);
    
    // 2. Create workflow states
    const states = ['draft', 'review', 'published', 'rejected'];
    const stateIds = {};
    let stateCreationCount = 0;
    
    for (let i = 0; i < states.length; i++) {
      const stateType = states[i];
      const state = generateWorkflowState(workflowId, stateType, i + 1);
      
      const createStateRes = http.post(
        `${BASE_URL}/api/workflow/states`,
        JSON.stringify(state),
        { headers }
      );
      
      const stateSuccess = check(createStateRes, {
        [`state ${stateType} created`]: (r) => r.status === 201,
      });
      
      if (stateSuccess) {
        stateCreationCount++;
        const createdState = JSON.parse(createStateRes.body);
        stateIds[stateType] = createdState.id;
        workflowOperations.add(1);
      }
      
      sleep(0.1);
    }
    
    console.log(`Created ${stateCreationCount} states`);
    
    // 3. Create transition rules (only if we have states)
    if (stateCreationCount >= 2) {
      const transitions = [
        { from: 'draft', to: 'review' },
        { from: 'review', to: 'published' },
        { from: 'review', to: 'rejected' },
      ];
      
      let ruleCreationCount = 0;
      for (const transition of transitions) {
        if (stateIds[transition.from] && stateIds[transition.to]) {
          const rule = generateTransitionRule(
            stateIds[transition.from],
            stateIds[transition.to]
          );
          
          const createRuleRes = http.post(
            `${BASE_URL}/api/workflow/rules`,
            JSON.stringify(rule),
            { headers }
          );
          
          const ruleSuccess = check(createRuleRes, {
            [`rule ${transition.from}->${transition.to} created`]: (r) => r.status === 201,
          });
          
          if (ruleSuccess) {
            ruleCreationCount++;
            workflowOperations.add(1);
          }
          
          sleep(0.1);
        }
      }
      
      console.log(`Created ${ruleCreationCount} transition rules`);
      
      // 4. Create and test workflow instance
      if (ruleCreationCount > 0) {
        const instance = generateWorkflowInstance(workflowId);
        
        const createInstanceRes = http.post(
          `${BASE_URL}/api/workflow/instances`,
          JSON.stringify(instance),
          { headers }
        );
        
        const instanceSuccess = check(createInstanceRes, {
          'workflow instance created': (r) => r.status === 201,
        });
        
        if (instanceSuccess) {
          workflowOperations.add(1);
          console.log('Workflow instance created - metrics should be triggered');
          
          // Try a transition if we have the instance
          const createdInstance = JSON.parse(createInstanceRes.body);
          sleep(0.5);
          
          const transitionRes = http.post(
            `${BASE_URL}/api/workflow/instances/${createdInstance.id}/transition`,
            JSON.stringify('review'),
            { headers }
          );
          
          const transitionSuccess = check(transitionRes, {
            'workflow transition successful': (r) => r.status === 200,
          });
          
          if (transitionSuccess) {
            workflowOperations.add(1);
            console.log('Workflow transition completed - transition metrics should be triggered');
          }
        }
      }
    }
  }
}

function testExistingEndpoints(headers) {
  console.log('Testing existing endpoints...');
  
  // Test read operations
  const getDefsRes = http.get(
    `${BASE_URL}/api/workflow/definitions/with-stats`,
    { headers }
  );
  
  check(getDefsRes, {
    'get definitions with stats': (r) => r.status === 200,
  });
  
  sleep(0.2);
  
  // Test debug endpoints
  const getRolesRes = http.get(
    `${BASE_URL}/api/workflow/debug/roles`,
    { headers }
  );
  
  check(getRolesRes, {
    'get system roles': (r) => r.status === 200,
  });
  
  sleep(0.2);
  
  const getDbRes = http.get(
    `${BASE_URL}/api/workflow/debug/database`,
    { headers }
  );
  
  check(getDbRes, {
    'database connection test': (r) => r.status === 200,
  });
}

function triggerMetricsOnly(headers) {
  console.log('Triggering metrics...');
  
  // Trigger test metrics multiple times
  for (let i = 0; i < randomIntBetween(1, 3); i++) {
    const metricsRes = http.get(`${BASE_URL}/test-workflow-metrics`);
    
    check(metricsRes, {
      'test metrics triggered': (r) => r.status === 200,
    });
    
    sleep(0.1);
  }
  
  // Check metrics endpoint
  const checkMetricsRes = http.get(`${BASE_URL}/metrics`);
  
  const metricsCheck = check(checkMetricsRes, {
    'metrics endpoint accessible': (r) => r.status === 200,
    'workflow metrics present': (r) => r.body.includes('workflow_'),
  });
  
  if (metricsCheck) {
    workflowOperations.add(1);
  }
}

function stressTestDebugEndpoints(headers) {
  console.log('Stress testing debug endpoints...');
  
  // Rapidly hit debug endpoints
  const endpoints = [
    '/api/workflow/debug/database',
    '/api/workflow/debug/roles',
    '/test-workflow-metrics',
    '/metrics'
  ];
  
  for (let i = 0; i < 5; i++) {
    const endpoint = randomItem(endpoints);
    const res = http.get(`${BASE_URL}${endpoint}`, { headers });
    
    check(res, {
      [`${endpoint} accessible`]: (r) => r.status === 200,
    });
    
    sleep(0.05); // Very short sleep for stress testing
  }
}

// Setup function to initialize any needed data
export function setup() {
  console.log('Starting workflow load test...');
  console.log(`Base URL: ${BASE_URL}`);
  
  // Verify the application is running
  const healthRes = http.get(`${BASE_URL}/test-workflow-metrics`);
  if (healthRes.status !== 200) {
    throw new Error(`Application not responding at ${BASE_URL}`);
  }
  
  return { baseUrl: BASE_URL };
}

// Teardown function
export function teardown(data) {
  console.log('Load test completed.');
  
  // Get final metrics count
  const metricsRes = http.get(`${data.baseUrl}/metrics`);
  if (metricsRes.status === 200) {
    const workflowMetricsCount = (metricsRes.body.match(/workflow_/g) || []).length;
    console.log(`Final workflow metrics count: ${workflowMetricsCount}`);
  }
}