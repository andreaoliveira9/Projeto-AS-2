import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const USERNAME = __ENV.USERNAME || 'admin';
const PASSWORD = __ENV.PASSWORD || 'password';

// Test options
export const options = {
  stages: [
    { duration: '30s', target: 10 },  // Ramp up to 10 users
    { duration: '1m', target: 10 },   // Stay at 10 users
    { duration: '30s', target: 0 },   // Ramp down to 0
  ],
  thresholds: {
    http_req_failed: ['rate<0.1'], // http errors should be less than 10%
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
  },
};

// Helper function to get auth token
function authenticate() {
  const loginUrl = `${BASE_URL}/manager/login`;
  
  // First, get the login page to extract any CSRF tokens if needed
  const loginPageRes = http.get(loginUrl);
  
  // Perform login
  const loginRes = http.post(loginUrl, {
    username: USERNAME,
    password: PASSWORD,
  }, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    redirects: 0, // Don't follow redirects automatically
  });
  
  // Extract authentication cookie
  const cookies = loginRes.cookies;
  return cookies;
}

// Test data generators
function generateWorkflowDefinition() {
  const timestamp = Date.now();
  return {
    id: `00000000-0000-0000-0000-000000000000`, // Let server generate ID
    name: `Load Test Workflow ${timestamp}`,
    description: `Workflow created during load testing at ${new Date().toISOString()}`,
    isActive: true,
    version: 1,
    createdBy: USERNAME,
    lastModifiedBy: USERNAME,
  };
}

function generateWorkflowState(workflowDefinitionId, stateType) {
  const states = {
    draft: {
      stateId: 'draft',
      name: 'Draft',
      description: 'Content is being drafted',
      colorCode: '#808080',
      isInitial: true,
      isPublished: false,
      isFinal: false,
      sortOrder: 1,
    },
    review: {
      stateId: 'review',
      name: 'In Review',
      description: 'Content is under review',
      colorCode: '#FFA500',
      isInitial: false,
      isPublished: false,
      isFinal: false,
      sortOrder: 2,
    },
    published: {
      stateId: 'published',
      name: 'Published',
      description: 'Content is published',
      colorCode: '#008000',
      isInitial: false,
      isPublished: true,
      isFinal: true,
      sortOrder: 3,
    },
    rejected: {
      stateId: 'rejected',
      name: 'Rejected',
      description: 'Content was rejected',
      colorCode: '#FF0000',
      isInitial: false,
      isPublished: false,
      isFinal: true,
      sortOrder: 4,
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
    sortOrder: baseState.sortOrder,
  };
}

function generateTransitionRule(fromStateId, toStateId) {
  return {
    fromStateId: fromStateId,
    toStateId: toStateId,
    allowedRoles: JSON.stringify(['Admin', 'Editor']),
    description: `Transition from ${fromStateId} to ${toStateId}`,
    requiresComment: toStateId === 'rejected',
    isActive: true,
    sortOrder: 1,
  };
}

// Main test scenario
export default function () {
  // Authenticate once per VU
  if (!__VU.cookies) {
    __VU.cookies = authenticate();
  }
  
  const headers = {
    'Content-Type': 'application/json',
    'Cookie': Object.entries(__VU.cookies)
      .map(([name, value]) => `${name}=${value.value}`)
      .join('; '),
  };
  
  // Test workflow: Create complete workflow setup
  const workflowDef = generateWorkflowDefinition();
  
  // 1. Create workflow definition
  const createDefRes = http.post(
    `${BASE_URL}/api/workflow/definitions`,
    JSON.stringify(workflowDef),
    { headers }
  );
  
  check(createDefRes, {
    'workflow definition created': (r) => r.status === 201,
  });
  
  if (createDefRes.status === 201) {
    const createdWorkflow = JSON.parse(createDefRes.body);
    const workflowId = createdWorkflow.id;
    
    // 2. Create workflow states
    const states = ['draft', 'review', 'published', 'rejected'];
    const stateIds = {};
    
    for (const stateType of states) {
      const state = generateWorkflowState(workflowId, stateType);
      const createStateRes = http.post(
        `${BASE_URL}/api/workflow/states`,
        JSON.stringify(state),
        { headers }
      );
      
      check(createStateRes, {
        [`state ${stateType} created`]: (r) => r.status === 201,
      });
      
      if (createStateRes.status === 201) {
        const createdState = JSON.parse(createStateRes.body);
        stateIds[stateType] = createdState.id;
      }
      
      sleep(0.1); // Small delay between state creations
    }
    
    // 3. Create transition rules
    const transitions = [
      { from: 'draft', to: 'review' },
      { from: 'review', to: 'published' },
      { from: 'review', to: 'rejected' },
      { from: 'rejected', to: 'draft' },
    ];
    
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
        
        check(createRuleRes, {
          [`rule ${transition.from}->${transition.to} created`]: (r) => r.status === 201,
        });
        
        sleep(0.1);
      }
    }
    
    // 4. Test workflow operations
    // Get workflow definition
    const getDefRes = http.get(
      `${BASE_URL}/api/workflow/definitions/${workflowId}`,
      { headers }
    );
    
    check(getDefRes, {
      'get workflow definition': (r) => r.status === 200,
    });
    
    // Get workflow states
    const getStatesRes = http.get(
      `${BASE_URL}/api/workflow/definitions/${workflowId}/states`,
      { headers }
    );
    
    check(getStatesRes, {
      'get workflow states': (r) => r.status === 200,
    });
    
    // Get workflow rules
    const getRulesRes = http.get(
      `${BASE_URL}/api/workflow/definitions/${workflowId}/rules`,
      { headers }
    );
    
    check(getRulesRes, {
      'get workflow rules': (r) => r.status === 200,
    });
    
    // 5. Create workflow instance (this should trigger metrics)
    const createInstanceRes = http.post(
      `${BASE_URL}/api/workflow/instances`,
      JSON.stringify({
        workflowDefinitionId: workflowId,
        contentId: `test-content-${Date.now()}`,
        contentType: 'page',
      }),
      { headers }
    );
    
    check(createInstanceRes, {
      'workflow instance created': (r) => r.status === 201,
    });
    
    if (createInstanceRes.status === 201) {
      const instance = JSON.parse(createInstanceRes.body);
      
      // 6. Perform workflow transition (this should trigger transition metrics)
      sleep(1); // Wait a bit before transition
      
      const transitionRes = http.post(
        `${BASE_URL}/api/workflow/instances/${instance.id}/transition`,
        JSON.stringify('review'), // Target state
        { headers }
      );
      
      check(transitionRes, {
        'workflow transition successful': (r) => r.status === 200,
      });
    }
    
    // 7. Test additional endpoints
    // Get all definitions with stats
    const getDefsWithStatsRes = http.get(
      `${BASE_URL}/api/workflow/definitions/with-stats`,
      { headers }
    );
    
    check(getDefsWithStatsRes, {
      'get definitions with stats': (r) => r.status === 200,
    });
    
    // Debug endpoints
    const getRolesRes = http.get(
      `${BASE_URL}/api/workflow/debug/roles`,
      { headers }
    );
    
    check(getRolesRes, {
      'get system roles': (r) => r.status === 200,
    });
  }
  
  sleep(randomItem([1, 2, 3])); // Random think time between iterations
}