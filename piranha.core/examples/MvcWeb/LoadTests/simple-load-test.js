import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Custom metrics
const successfulRequests = new Counter('successful_requests');
const metricsTriggered = new Counter('metrics_triggered');

// Test options
export const options = {
  stages: [
    { duration: '30s', target: 5 },   // Ramp up to 5 users
    { duration: '1m', target: 5 },    // Stay at 5 users
    { duration: '30s', target: 0 },   // Ramp down to 0
  ],
  thresholds: {
    http_req_failed: ['rate<0.1'], // http errors should be less than 10%
    http_req_duration: ['p(95)<1000'], // 95% of requests should be below 1s
  },
};

export function setup() {
  console.log(`Starting load test against ${BASE_URL}`);
  
  // Test if application is responding
  const healthRes = http.get(`${BASE_URL}/test-workflow-metrics`);
  if (healthRes.status !== 200) {
    throw new Error(`Application not responding at ${BASE_URL}`);
  }
  
  console.log('Application is responding, starting load test...');
  return { baseUrl: BASE_URL };
}

export default function (data) {
  const testScenarios = [
    'trigger_metrics',
    'check_metrics',
    'test_debug_endpoints',
    'mixed_requests'
  ];
  
  // Pick a random scenario
  const scenario = testScenarios[Math.floor(Math.random() * testScenarios.length)];
  
  switch (scenario) {
    case 'trigger_metrics':
      triggerMetrics();
      break;
    case 'check_metrics':
      checkMetrics();
      break;
    case 'test_debug_endpoints':
      testDebugEndpoints();
      break;
    case 'mixed_requests':
      mixedRequests();
      break;
  }
  
  sleep(Math.random() * 2 + 1); // Sleep 1-3 seconds
}

function triggerMetrics() {
  // Trigger workflow metrics multiple times
  for (let i = 0; i < Math.floor(Math.random() * 3) + 1; i++) {
    const res = http.get(`${BASE_URL}/test-workflow-metrics`);
    
    const success = check(res, {
      'metrics triggered': (r) => r.status === 200,
      'response contains success message': (r) => r.body.includes('triggered successfully'),
    });
    
    if (success) {
      successfulRequests.add(1);
      metricsTriggered.add(1);
    }
    
    sleep(0.1);
  }
}

function checkMetrics() {
  // Get metrics endpoint
  const res = http.get(`${BASE_URL}/metrics`);
  
  const success = check(res, {
    'metrics endpoint accessible': (r) => r.status === 200,
    'contains workflow metrics': (r) => r.body.includes('workflow_'),
    'prometheus format': (r) => r.body.includes('# TYPE') && r.body.includes('# HELP'),
  });
  
  if (success) {
    successfulRequests.add(1);
    
    // Count workflow metrics
    const workflowMetricsCount = (res.body.match(/workflow_/g) || []).length;
    console.log(`Found ${workflowMetricsCount} workflow metric references`);
  }
}

function testDebugEndpoints() {
  const endpoints = [
    '/api/workflow/debug/database',
    '/api/workflow/debug/roles'
  ];
  
  for (const endpoint of endpoints) {
    const res = http.get(`${BASE_URL}${endpoint}`);
    
    const success = check(res, {
      [`${endpoint} accessible`]: (r) => r.status === 200,
    });
    
    if (success) {
      successfulRequests.add(1);
    }
    
    sleep(0.1);
  }
}

function mixedRequests() {
  // Mix of different endpoint calls
  const requests = [
    () => http.get(`${BASE_URL}/test-workflow-metrics`),
    () => http.get(`${BASE_URL}/metrics`),
    () => http.get(`${BASE_URL}/api/workflow/debug/database`),
    () => http.get(`${BASE_URL}/api/workflow/debug/roles`),
  ];
  
  // Execute 2-4 random requests
  const numRequests = Math.floor(Math.random() * 3) + 2;
  
  for (let i = 0; i < numRequests; i++) {
    const randomRequest = requests[Math.floor(Math.random() * requests.length)];
    const res = randomRequest();
    
    const success = check(res, {
      'request successful': (r) => r.status === 200,
    });
    
    if (success) {
      successfulRequests.add(1);
    }
    
    sleep(0.05);
  }
}

export function teardown(data) {
  console.log('Load test completed. Checking final metrics...');
  
  // Get final metrics
  const metricsRes = http.get(`${data.baseUrl}/metrics`);
  if (metricsRes.status === 200) {
    const workflowMetricsCount = (metricsRes.body.match(/workflow_/g) || []).length;
    console.log(`Final workflow metrics count: ${workflowMetricsCount}`);
    
    // Show some sample metrics
    const lines = metricsRes.body.split('\n');
    const workflowLines = lines.filter(line => 
      line.includes('workflow_') && 
      !line.includes('_bucket') && 
      !line.includes('_sum') && 
      !line.includes('_count') &&
      !line.startsWith('#')
    );
    
    console.log('Sample workflow metrics:');
    workflowLines.slice(0, 5).forEach(line => {
      console.log(`  ${line}`);
    });
  }
}