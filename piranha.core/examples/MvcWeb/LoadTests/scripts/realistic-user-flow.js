import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

// Custom metrics for realistic user behavior
const pageViews = new Counter('realistic_page_views');
const userSessions = new Counter('realistic_user_sessions');
const contentInteractions = new Counter('realistic_content_interactions');
const responseTime = new Trend('realistic_response_time', true);
const errorRate = new Rate('realistic_error_rate');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export const options = {
  scenarios: {
    typical_website_visitors: {
      executor: 'ramping-arrival-rate',
      startRate: 1,
      timeUnit: '1s',
      preAllocatedVUs: 5,
      maxVUs: 20,
      stages: [
        { duration: '1m', target: 3 },   // Normal traffic
        { duration: '2m', target: 8 },   // Busy period
        { duration: '1m', target: 15 },  // Peak traffic
        { duration: '1m', target: 5 },   // Cool down
        { duration: '30s', target: 1 },  // Low traffic
      ],
    },
    content_creators: {
      executor: 'constant-vus',
      vus: 2,
      duration: '5m',
      exec: 'contentCreatorFlow',
    },
  },
  thresholds: {
    'realistic_response_time': ['p(95)<2000'],
    'realistic_error_rate': ['rate<0.05'],
    'http_req_duration': ['p(90)<1500'],
    'http_req_failed': ['rate<0.1'],
  },
};

// Typical website visitor flow
export default function() {
  let session = {
    sessionId: `session_${__VU}_${Date.now()}`,
    startTime: Date.now()
  };
  
  userSessions.add(1);
  
  group('Website Visitor Journey', () => {
    
    // 1. Landing on homepage
    group('Homepage Visit', () => {
      const startTime = Date.now();
      const response = http.get(BASE_URL, {
        headers: {
          'User-Agent': 'Mozilla/5.0 (compatible; k6-load-test)',
          'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        }
      });
      
      const duration = Date.now() - startTime;
      responseTime.add(duration);
      pageViews.add(1);
      
      const success = check(response, {
        'homepage loads': (r) => r.status === 200,
        'homepage has content': (r) => r.body.length > 1000,
        'homepage loads fast': (r) => r.timings.duration < 2000,
      });
      
      if (!success) errorRate.add(1);
    });
    
    sleep(2 + Math.random() * 3); // Realistic reading time
    
    // 2. Browse different pages/content
    group('Content Browsing', () => {
      const pages = [
        '/',
        '/manager',
        '/dashboard/metrics',
        '/api/workflow/definitions',
        '/api/workflow/instances',
        '/metrics',
      ];
      
      // Visit 2-4 random pages
      const pagesToVisit = Math.floor(Math.random() * 3) + 2;
      for (let i = 0; i < pagesToVisit; i++) {
        const randomPage = pages[Math.floor(Math.random() * pages.length)];
        
        const startTime = Date.now();
        const response = http.get(`${BASE_URL}${randomPage}`, {
          headers: {
            'Referer': BASE_URL,
            'User-Agent': 'Mozilla/5.0 (compatible; k6-load-test)',
          }
        });
        
        const duration = Date.now() - startTime;
        responseTime.add(duration);
        pageViews.add(1);
        contentInteractions.add(1);
        
        check(response, {
          [`page ${randomPage} accessible`]: (r) => r.status < 400,
        });
        
        sleep(1 + Math.random() * 2); // Reading time between pages
      }
    });
    
    // 3. Some users try to interact with content
    if (Math.random() < 0.3) { // 30% of users try to do something
      group('Content Interaction Attempt', () => {
        // Trigger our custom metrics endpoint
        const response = http.post(`${BASE_URL}/api/metricstest/trigger-page-metrics`, null, {
          headers: {
            'Content-Type': 'application/json',
          }
        });
        
        contentInteractions.add(1);
        
        check(response, {
          'interaction endpoint responds': (r) => r.status === 200,
        });
      });
    }
  });
  
  // Session end
  const sessionDuration = Date.now() - session.startTime;
  sleep(Math.random() * 2); // Exit delay
}

// Content creator workflow (more intensive)
export function contentCreatorFlow() {
  group('Content Creator Workflow', () => {
    
    // 1. Access manager interface  
    group('Manager Access', () => {
      const response = http.get(`${BASE_URL}/manager`, {
        headers: {
          'User-Agent': 'Mozilla/5.0 (compatible; k6-content-creator)',
        }
      });
      
      responseTime.add(response.timings.duration);
      
      check(response, {
        'manager accessible': (r) => r.status === 200,
      });
    });
    
    sleep(2);
    
    // 2. Create content (simulate)
    group('Content Creation', () => {
      // Trigger page creation metrics
      let response = http.post(`${BASE_URL}/api/metricstest/trigger-page-metrics`);
      contentInteractions.add(1);
      
      sleep(1);
      
      // Trigger workflow metrics
      response = http.post(`${BASE_URL}/api/metricstest/trigger-workflow-metrics`);
      contentInteractions.add(1);
      
      sleep(1);
      
      // Trigger media metrics
      response = http.post(`${BASE_URL}/api/metricstest/trigger-media-metrics`);
      contentInteractions.add(1);
      
      check(response, {
        'content creation simulated': (r) => r.status === 200,
      });
    });
    
    sleep(3);
    
    // 3. Check metrics dashboard
    group('Metrics Dashboard Check', () => {
      const response = http.get(`${BASE_URL}/dashboard/metrics`, {
        headers: {
          'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        }
      });
      
      responseTime.add(response.timings.duration);
      pageViews.add(1);
      
      check(response, {
        'dashboard loads': (r) => r.status === 200,
        'dashboard has metrics': (r) => r.body.includes('metrics') || r.body.includes('chart'),
      });
    });
    
    sleep(5); // Content creator thinking time
  });
}

export function setup() {
  console.log('Starting realistic user flow testing...');
  
  // Warm up the application
  const warmupResponse = http.get(BASE_URL);
  console.log(`Warmup response: ${warmupResponse.status}`);
  
  return {};
}

export function teardown(data) {
  console.log('Realistic user flow test completed');
  console.log(`Total page views: ${pageViews.count}`);
  console.log(`Total user sessions: ${userSessions.count}`);
  console.log(`Total content interactions: ${contentInteractions.count}`);
}