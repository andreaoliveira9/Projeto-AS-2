import { browser } from 'k6/experimental/browser';
import { check, group, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

// Custom metrics for UI interactions
const pageLoadTime = new Trend('ui_page_load_time', true);
const userActions = new Counter('ui_user_actions');
const errorRate = new Rate('ui_error_rate');
const contentCreationTime = new Trend('ui_content_creation_time', true);

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export const options = {
  scenarios: {
    content_creator_journey: {
      executor: 'ramping-vus',
      vus: 0,
      stages: [
        { duration: '30s', target: 2 },  // Ramp up
        { duration: '2m', target: 5 },   // Stay at 5 users
        { duration: '1m', target: 8 },   // Peak load
        { duration: '1m', target: 3 },   // Ramp down
        { duration: '30s', target: 0 },  // Cool down
      ],
      options: {
        browser: {
          type: 'chromium',
        },
      },
    },
  },
  thresholds: {
    'ui_page_load_time': ['p(95)<3000'],
    'ui_error_rate': ['rate<0.05'],
    'ui_content_creation_time': ['p(90)<10000'],
    'browser_web_vital_fcp': ['p(95)<2000'],
    'browser_web_vital_lcp': ['p(95)<4000'],
  },
};

export default async function() {
  const page = browser.newPage();
  
  try {
    await group('Content Creator User Journey', async () => {
      
      // 1. Homepage visit
      await group('Visit Homepage', async () => {
        const startTime = Date.now();
        await page.goto(BASE_URL);
        await page.waitForLoadState('networkidle');
        const loadTime = Date.now() - startTime;
        
        pageLoadTime.add(loadTime);
        userActions.add(1);
        
        check(page, {
          'homepage loads successfully': () => page.url().includes(BASE_URL),
        });
      });
      
      sleep(2);
      
      // 2. Navigate to Manager
      await group('Access Manager Interface', async () => {
        const startTime = Date.now();
        await page.goto(`${BASE_URL}/manager`);
        await page.waitForLoadState('networkidle');
        const loadTime = Date.now() - startTime;
        
        pageLoadTime.add(loadTime);
        userActions.add(1);
        
        const success = check(page, {
          'manager interface loads': () => page.url().includes('/manager'),
          'login form present': () => page.locator('input[type="email"], input[name="email"]').isVisible(),
        });
        
        if (!success) errorRate.add(1);
      });
      
      sleep(1);
      
      // 3. Login Process
      await group('User Login', async () => {
        try {
          // Try to login (might already be logged in)
          const emailInput = page.locator('input[type="email"], input[name="email"]').first();
          const passwordInput = page.locator('input[type="password"], input[name="password"]').first();
          
          if (await emailInput.isVisible()) {
            await emailInput.fill('admin@admin.com');
            await passwordInput.fill('password');
            
            const submitButton = page.locator('button[type="submit"], input[type="submit"]').first();
            await submitButton.click();
            
            await page.waitForLoadState('networkidle');
            userActions.add(1);
          }
          
          check(page, {
            'user logged in successfully': () => !page.url().includes('/login'),
          });
        } catch (error) {
          console.log('Login not required or already logged in');
        }
      });
      
      sleep(2);
      
      // 4. Navigate to Pages Section
      await group('Browse Pages Section', async () => {
        const startTime = Date.now();
        
        // Look for pages link in navigation
        const pagesLink = page.locator('a[href*="pages"], .nav-link:has-text("Pages")').first();
        if (await pagesLink.isVisible()) {
          await pagesLink.click();
        } else {
          await page.goto(`${BASE_URL}/manager/pages`);
        }
        
        await page.waitForLoadState('networkidle');
        const loadTime = Date.now() - startTime;
        
        pageLoadTime.add(loadTime);
        userActions.add(1);
        
        check(page, {
          'pages section loads': () => page.url().includes('pages'),
          'page list visible': () => page.locator('.table, .list, .content').isVisible(),
        });
      });
      
      sleep(2);
      
      // 5. Create New Page
      await group('Create New Page', async () => {
        const creationStartTime = Date.now();
        
        try {
          // Look for "Add" or "Create" button
          const addButton = page.locator('button:has-text("Add"), a:has-text("Add"), .btn:has-text("Create"), a[href*="add"]').first();
          
          if (await addButton.isVisible()) {
            await addButton.click();
            await page.waitForLoadState('networkidle');
            
            // Fill page form
            const timestamp = Date.now();
            const titleInput = page.locator('input[name="title"], #title, .title-input').first();
            const slugInput = page.locator('input[name="slug"], #slug, .slug-input').first();
            
            if (await titleInput.isVisible()) {
              await titleInput.fill(`Load Test Page ${timestamp}`);
              userActions.add(1);
            }
            
            if (await slugInput.isVisible()) {
              await slugInput.fill(`load-test-page-${timestamp}`);
              userActions.add(1);
            }
            
            // Add some content if there's an editor
            const contentArea = page.locator('textarea[name="content"], .editor, .content-editor').first();
            if (await contentArea.isVisible()) {
              await contentArea.fill(`This is a test page created during load testing at ${new Date().toISOString()}`);
              userActions.add(1);
            }
            
            sleep(1);
            
            // Save the page
            const saveButton = page.locator('button:has-text("Save"), input[value="Save"], .btn-save').first();
            if (await saveButton.isVisible()) {
              await saveButton.click();
              await page.waitForLoadState('networkidle');
              userActions.add(1);
              
              const creationTime = Date.now() - creationStartTime;
              contentCreationTime.add(creationTime);
              
              // Check for success indicators
              const success = check(page, {
                'page created successfully': () => 
                  page.locator('.alert-success, .notification-success, .success').isVisible() ||
                  !page.url().includes('/add'),
              });
              
              if (!success) errorRate.add(1);
            }
          } else {
            console.log('Add button not found, skipping page creation');
            errorRate.add(1);
          }
        } catch (error) {
          console.log('Error creating page:', error.message);
          errorRate.add(1);
        }
      });
      
      sleep(2);
      
      // 6. Browse Created Content
      await group('Browse Created Content', async () => {
        // Go back to pages list
        const pagesLink = page.locator('a[href*="pages"], .nav-link:has-text("Pages")').first();
        if (await pagesLink.isVisible()) {
          await pagesLink.click();
          await page.waitForLoadState('networkidle');
          userActions.add(1);
          
          check(page, {
            'can return to pages list': () => page.url().includes('pages'),
          });
        }
      });
      
      sleep(1);
      
      // 7. Check Media Section (if available)
      await group('Browse Media Section', async () => {
        try {
          const mediaLink = page.locator('a[href*="media"], .nav-link:has-text("Media")').first();
          if (await mediaLink.isVisible()) {
            await mediaLink.click();
            await page.waitForLoadState('networkidle');
            userActions.add(1);
            
            check(page, {
              'media section accessible': () => page.url().includes('media'),
            });
          }
        } catch (error) {
          console.log('Media section not accessible or not found');
        }
      });
      
    });
    
  } catch (error) {
    console.error('Browser test error:', error);
    errorRate.add(1);
  } finally {
    page.close();
  }
  
  // Random think time between user actions
  sleep(Math.random() * 3 + 1);
}

// Setup function
export function setup() {
  console.log('Starting UI load test for content creators...');
  return {};
}

// Teardown function  
export function teardown(data) {
  console.log('UI load test completed');
}