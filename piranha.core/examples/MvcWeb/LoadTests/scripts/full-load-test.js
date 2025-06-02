import { check, group, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { setupAuth, auth } from './auth.js';
import { Counter, Trend, Rate, Gauge } from 'k6/metrics';

// Custom metrics for comprehensive monitoring
const metrics = {
    // Workflow metrics
    workflowTransitions: new Counter('piranha_workflow_transitions_total'),
    workflowDuration: new Trend('piranha_workflow_duration_ms'),
    workflowErrors: new Counter('piranha_workflow_errors_total'),
    
    // Page metrics
    pagesCreated: new Counter('piranha_pages_created_total'),
    pagesPublished: new Counter('piranha_pages_published_total'),
    pageLoadTime: new Trend('piranha_page_load_time_ms'),
    pageSuccessRate: new Rate('piranha_page_success_rate'),
    
    // Post metrics
    postsCreated: new Counter('piranha_posts_created_total'),
    postsPublished: new Counter('piranha_posts_published_total'),
    postSuccessRate: new Rate('piranha_post_success_rate'),
    
    // Media metrics
    mediaUploaded: new Counter('piranha_media_uploaded_total'),
    mediaSize: new Trend('piranha_media_size_bytes'),
    
    // System metrics
    activeUsers: new Gauge('piranha_active_users'),
    cacheHitRate: new Rate('piranha_cache_hit_rate')
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const TEST_DURATION = __ENV.TEST_DURATION || '5m';

// Test configuration with multiple scenarios
export const options = {
    scenarios: {
        // Scenario 1: Normal load with gradual ramp-up
        normal_load: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 20 },
                { duration: '3m', target: 50 },
                { duration: '1m', target: 0 }
            ],
            gracefulRampDown: '30s',
            exec: 'normalLoadTest'
        },
        
        // Scenario 2: Spike test
        spike_test: {
            executor: 'ramping-arrival-rate',
            startRate: 10,
            timeUnit: '1s',
            preAllocatedVUs: 100,
            maxVUs: 200,
            stages: [
                { duration: '30s', target: 10 },
                { duration: '1m', target: 100 },
                { duration: '30s', target: 10 }
            ],
            exec: 'spikeTest',
            startTime: '2m'
        },
        
        // Scenario 3: Sustained load
        sustained_load: {
            executor: 'constant-vus',
            vus: 30,
            duration: '5m',
            exec: 'sustainedLoadTest'
        },
        
        // Scenario 4: API stress test
        api_stress: {
            executor: 'per-vu-iterations',
            vus: 25,
            iterations: 100,
            maxDuration: '5m',
            exec: 'apiStressTest'
        }
    },
    
    thresholds: {
        // General HTTP thresholds
        'http_req_duration': ['p(95)<2000', 'p(99)<3000'],
        'http_req_failed': ['rate<0.05'],
        
        // Workflow thresholds
        'piranha_workflow_duration_ms': ['p(95)<2500', 'p(99)<4000'],
        'piranha_workflow_errors_total': ['count<50'],
        
        // Page thresholds
        'piranha_page_load_time_ms': ['p(95)<1000', 'p(99)<2000'],
        'piranha_page_success_rate': ['rate>0.95'],
        
        // Post thresholds
        'piranha_post_success_rate': ['rate>0.95'],
        
        // Media thresholds
        'piranha_media_size_bytes': ['p(95)<5000000'] // 5MB
    },
    
    // Enable additional telemetry
    ext: {
        loadimpact: {
            projectID: 3478725,
            name: 'Piranha CMS Full Load Test',
            distribution: {
                distributionLabel1: { loadZone: 'amazon:us:ashburn', percent: 50 },
                distributionLabel2: { loadZone: 'amazon:eu:dublin', percent: 50 }
            }
        }
    }
};

// Setup function - runs once before all tests
export function setup() {
    console.log('Setting up load test environment...');
    const authData = setupAuth();
    
    // Warm up the application
    warmupApplication();
    
    return {
        auth: authData,
        testStartTime: new Date().toISOString()
    };
}

// Warmup function to prepare the application
function warmupApplication() {
    const warmupRequests = [
        { method: 'GET', url: `${BASE_URL}/` },
        { method: 'GET', url: `${BASE_URL}/api/pages` },
        { method: 'GET', url: `${BASE_URL}/api/posts` },
        { method: 'GET', url: `${BASE_URL}/api/editorialworkflow/definitions` }
    ];
    
    warmupRequests.forEach(req => {
        const res = auth.makeRequest(req.method, req.url, null, false);
        check(res, {
            [`warmup ${req.url} successful`]: (r) => r.status < 400
        });
        sleep(0.5);
    });
}

// Test scenarios
export function normalLoadTest(data) {
    metrics.activeUsers.add(__VU);
    
    group('Normal Load Operations', () => {
        // Mix of read and write operations
        const operations = [
            () => createAndPublishPage(),
            () => createAndPublishPost(),
            () => browsePages(),
            () => uploadMedia(),
            () => workflowOperations()
        ];
        
        const operation = operations[Math.floor(Math.random() * operations.length)];
        operation();
    });
    
    sleep(think_time());
}

export function spikeTest(data) {
    group('Spike Test', () => {
        // Rapid page views to simulate traffic spike
        for (let i = 0; i < 5; i++) {
            browsePages();
            sleep(0.1);
        }
    });
}

export function sustainedLoadTest(data) {
    group('Sustained Load', () => {
        // Consistent mix of operations
        if (__ITER % 5 === 0) createAndPublishPage();
        if (__ITER % 7 === 0) createAndPublishPost();
        if (__ITER % 3 === 0) workflowOperations();
        browsePages();
    });
    
    sleep(think_time(2, 4));
}

export function apiStressTest(data) {
    group('API Stress Test', () => {
        // Rapid API calls
        const endpoints = [
            '/api/pages',
            '/api/posts',
            '/api/media',
            '/api/test/workflow/instances'
        ];
        
        endpoints.forEach(endpoint => {
            const res = auth.makeRequest('GET', `${BASE_URL}${endpoint}`, null, true);
            check(res, {
                [`${endpoint} responds`]: (r) => r.status === 200
            });
        });
    });
}

// Helper functions for different operations
function createAndPublishPage() {
    const pageData = {
        title: `Performance Test Page ${__VU}-${__ITER}`,
        slug: `perf-test-page-${__VU}-${__ITER}-${Date.now()}`,
        typeId: 'StandardPage',
        siteId: 'default',
        blocks: [
            {
                type: 'Piranha.Extend.Blocks.HtmlBlock',
                body: generateContent()
            }
        ]
    };
    
    const startTime = new Date().getTime();
    const res = auth.makeRequest('POST', `${BASE_URL}/api/pages`, pageData, true);
    const duration = new Date().getTime() - startTime;
    
    const success = res.status === 200 || res.status === 201;
    metrics.pageSuccessRate.add(success ? 1 : 0);
    
    if (success) {
        metrics.pagesCreated.add(1);
        metrics.pageLoadTime.add(duration);
        
        const page = JSON.parse(res.body);
        
        // Publish the page
        const publishRes = auth.makeRequest('POST', `${BASE_URL}/api/pages/${page.id}/publish`, null, true);
        if (publishRes.status === 200) {
            metrics.pagesPublished.add(1);
        }
    }
}

function createAndPublishPost() {
    const postData = {
        title: `Performance Test Post ${__VU}-${__ITER}`,
        slug: `perf-test-post-${__VU}-${__ITER}-${Date.now()}`,
        typeId: 'StandardPost',
        blogId: 'default-blog',
        excerpt: 'Performance test post excerpt',
        blocks: [
            {
                type: 'Piranha.Extend.Blocks.HtmlBlock',
                body: generateContent()
            }
        ],
        tags: ['performance', 'load-test'],
        category: 'Testing'
    };
    
    const res = auth.makeRequest('POST', `${BASE_URL}/api/posts`, postData, true);
    const success = res.status === 200 || res.status === 201;
    metrics.postSuccessRate.add(success ? 1 : 0);
    
    if (success) {
        metrics.postsCreated.add(1);
        
        const post = JSON.parse(res.body);
        
        // Publish the post
        const publishRes = auth.makeRequest('POST', `${BASE_URL}/api/posts/${post.id}/publish`, null, true);
        if (publishRes.status === 200) {
            metrics.postsPublished.add(1);
        }
    }
}

function browsePages() {
    const pages = ['/', '/about', '/blog', '/contact'];
    const page = pages[Math.floor(Math.random() * pages.length)];
    
    const startTime = new Date().getTime();
    const res = auth.makeRequest('GET', `${BASE_URL}${page}`, null, false);
    const duration = new Date().getTime() - startTime;
    
    if (res.status === 200) {
        metrics.pageLoadTime.add(duration);
        metrics.cacheHitRate.add(res.headers['X-Cache'] === 'HIT' ? 1 : 0);
    }
}

function uploadMedia() {
    // Simulate media upload with metadata
    const mediaData = {
        filename: `test-image-${__VU}-${__ITER}.jpg`,
        type: 'image/jpeg',
        size: Math.floor(Math.random() * 1000000) + 100000, // 100KB to 1MB
        folderId: null
    };
    
    const res = auth.makeRequest('POST', `${BASE_URL}/api/media/metadata`, mediaData, true);
    
    if (res.status === 200) {
        metrics.mediaUploaded.add(1);
        metrics.mediaSize.add(mediaData.size);
    }
}

function workflowOperations() {
    const contentId = `workflow-content-${__VU}-${__ITER}`;
    const states = ['Draft', 'Review', 'Approved', 'Published'];
    
    const startTime = new Date().getTime();
    
    // Create workflow instance
    const instanceData = {
        workflowDefinitionId: 'default-workflow',
        contentId: contentId,
        contentType: 'Piranha.Models.PageBase',
        currentState: states[0]
    };
    
    const res = auth.makeRequest('POST', `${BASE_URL}/api/test/workflow/instances`, instanceData, true);
    
    if (res.status === 200 || res.status === 201) {
        const duration = new Date().getTime() - startTime;
        metrics.workflowDuration.add(duration);
        metrics.workflowTransitions.add(1);
    } else {
        metrics.workflowErrors.add(1);
    }
}

// Helper functions
function generateContent() {
    const paragraphs = [
        'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
        'Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
        'Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris.',
        'Duis aute irure dolor in reprehenderit in voluptate velit esse cillum.',
        'Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia.'
    ];
    
    const numParagraphs = Math.floor(Math.random() * 3) + 1;
    let content = '';
    
    for (let i = 0; i < numParagraphs; i++) {
        content += `<p>${paragraphs[Math.floor(Math.random() * paragraphs.length)]}</p>`;
    }
    
    return content;
}

function think_time(min = 1, max = 3) {
    return Math.random() * (max - min) + min;
}

// Teardown function - runs once after all tests
export function teardown(data) {
    console.log('Load test completed at:', new Date().toISOString());
    console.log('Test duration:', data.testStartTime, '-', new Date().toISOString());
}

// Custom summary generation
export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        'summary.html': htmlReport(data),
        'summary.json': JSON.stringify(data, null, 2)
    };
}