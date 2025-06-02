import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';
import { setupAuth, auth } from './auth.js';

// Custom metrics
const pageOperationDuration = new Trend('page_operation_duration', true);
const postOperationDuration = new Trend('post_operation_duration', true);
const pageCreationRate = new Rate('page_creation_success');
const postCreationRate = new Rate('post_creation_success');
const contentErrors = new Counter('content_errors');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// Test configuration
export const options = {
    scenarios: {
        content_operations: {
            executor: 'constant-vus',
            vus: 10,
            duration: '3m'
        },
        page_views: {
            executor: 'constant-arrival-rate',
            rate: 30,
            timeUnit: '1s',
            duration: '3m',
            preAllocatedVUs: 10,
            maxVUs: 50
        }
    },
    thresholds: {
        'http_req_duration': ['p(95)<1000', 'p(99)<2000'],
        'http_req_failed': ['rate<0.05'],
        'page_operation_duration': ['p(95)<1500', 'p(99)<2500'],
        'post_operation_duration': ['p(95)<1500', 'p(99)<2500'],
        'page_creation_success': ['rate>0.95'],
        'post_creation_success': ['rate>0.95']
    }
};

// Setup function
export function setup() {
    return setupAuth();
}

// Main test function
export default function (data) {
    const authData = data;
    const scenario = __ENV.SCENARIO || 'mixed';

    if (scenario === 'content_operations' || scenario === 'mixed') {
        performContentOperations();
    }
    
    if (scenario === 'page_views' || scenario === 'mixed') {
        performPageViews();
    }
}

function performContentOperations() {
    group('Page Operations', () => {
        // Test 1: Create a page
        group('Create Page', () => {
            const pageData = {
                title: `Load Test Page ${__VU}-${__ITER}`,
                slug: `load-test-page-${__VU}-${__ITER}-${Date.now()}`,
                typeId: 'StandardPage',
                siteId: 'default',
                navigationTitle: `Nav ${__VU}-${__ITER}`,
                metaDescription: 'Load test page for performance testing',
                blocks: [
                    {
                        type: 'Piranha.Extend.Blocks.HtmlBlock',
                        body: '<p>This is a load test page content.</p>'
                    }
                ]
            };

            const startTime = new Date().getTime();
            const res = auth.makeRequest('POST', `${BASE_URL}/api/pages`, pageData, true);
            const endTime = new Date().getTime();

            const success = check(res, {
                'page created': (r) => r.status === 200 || r.status === 201,
                'response has page ID': (r) => {
                    if (r.status === 200 || r.status === 201) {
                        const body = JSON.parse(r.body);
                        return body.id !== undefined;
                    }
                    return false;
                }
            });

            pageCreationRate.add(success ? 1 : 0);
            
            if (success) {
                pageOperationDuration.add(endTime - startTime);
                const page = JSON.parse(res.body);
                
                // Test 2: Update the page
                sleep(1);
                group('Update Page', () => {
                    page.title = `Updated ${page.title}`;
                    page.lastModified = new Date().toISOString();
                    
                    const updateStart = new Date().getTime();
                    const updateRes = auth.makeRequest('PUT', `${BASE_URL}/api/pages/${page.id}`, page, true);
                    const updateEnd = new Date().getTime();
                    
                    check(updateRes, {
                        'page updated': (r) => r.status === 200
                    });
                    
                    if (updateRes.status === 200) {
                        pageOperationDuration.add(updateEnd - updateStart);
                    } else {
                        contentErrors.add(1);
                    }
                });

                // Test 3: Publish the page
                sleep(1);
                group('Publish Page', () => {
                    const publishStart = new Date().getTime();
                    const publishRes = auth.makeRequest('POST', `${BASE_URL}/api/pages/${page.id}/publish`, null, true);
                    const publishEnd = new Date().getTime();
                    
                    check(publishRes, {
                        'page published': (r) => r.status === 200
                    });
                    
                    if (publishRes.status === 200) {
                        pageOperationDuration.add(publishEnd - publishStart);
                    } else {
                        contentErrors.add(1);
                    }
                });
                
                return page;
            } else {
                contentErrors.add(1);
                return null;
            }
        });
    });

    sleep(2);

    group('Post Operations', () => {
        // Test 1: Create a post
        group('Create Post', () => {
            const postData = {
                title: `Load Test Post ${__VU}-${__ITER}`,
                slug: `load-test-post-${__VU}-${__ITER}-${Date.now()}`,
                typeId: 'StandardPost',
                blogId: 'default-blog',
                excerpt: 'This is a load test post excerpt',
                blocks: [
                    {
                        type: 'Piranha.Extend.Blocks.HtmlBlock',
                        body: '<p>This is a load test post content with some text.</p>'
                    },
                    {
                        type: 'Piranha.Extend.Blocks.TextBlock',
                        body: 'Additional text block for the post.'
                    }
                ],
                tags: ['load-test', 'performance', `vu-${__VU}`],
                category: 'Testing'
            };

            const startTime = new Date().getTime();
            const res = auth.makeRequest('POST', `${BASE_URL}/api/posts`, postData, true);
            const endTime = new Date().getTime();

            const success = check(res, {
                'post created': (r) => r.status === 200 || r.status === 201,
                'response has post ID': (r) => {
                    if (r.status === 200 || r.status === 201) {
                        const body = JSON.parse(r.body);
                        return body.id !== undefined;
                    }
                    return false;
                }
            });

            postCreationRate.add(success ? 1 : 0);
            
            if (success) {
                postOperationDuration.add(endTime - startTime);
                const post = JSON.parse(res.body);
                
                // Test 2: Publish the post
                sleep(1);
                group('Publish Post', () => {
                    const publishStart = new Date().getTime();
                    const publishRes = auth.makeRequest('POST', `${BASE_URL}/api/posts/${post.id}/publish`, null, true);
                    const publishEnd = new Date().getTime();
                    
                    check(publishRes, {
                        'post published': (r) => r.status === 200
                    });
                    
                    if (publishRes.status === 200) {
                        postOperationDuration.add(publishEnd - publishStart);
                    } else {
                        contentErrors.add(1);
                    }
                });
                
                return post;
            } else {
                contentErrors.add(1);
                return null;
            }
        });
    });

    sleep(think_time());
}

function performPageViews() {
    group('Page Views', () => {
        // Simulate public page views
        const pages = [
            '/',
            '/about',
            '/blog',
            '/contact',
            `/page-${Math.floor(Math.random() * 100)}`
        ];
        
        const selectedPage = pages[Math.floor(Math.random() * pages.length)];
        
        const res = http.get(`${BASE_URL}${selectedPage}`, {
            headers: {
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
                'User-Agent': 'k6-load-test/1.0'
            }
        });
        
        check(res, {
            'page loaded': (r) => r.status === 200 || r.status === 404,
            'response time OK': (r) => r.timings.duration < 1000
        });
    });
    
    sleep(think_time(0.5, 2));
}

// Helper function for think time
function think_time(min = 1, max = 3) {
    return Math.random() * (max - min) + min;
}

// Teardown function
export function teardown(data) {
    console.log('Content load test completed');
}