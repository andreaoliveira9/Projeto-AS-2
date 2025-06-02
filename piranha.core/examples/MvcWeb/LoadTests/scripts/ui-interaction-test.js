import { browser } from 'k6/experimental/browser';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    ui_test: {
      executor: 'constant-vus',
      vus: 2,
      duration: '2m',
      options: {
        browser: {
          type: 'chromium',
        },
      },
    },
  },
  thresholds: {
    checks: ['rate==1.0'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export default async function () {
  const page = browser.newPage();

  try {
    // Navigate to the manager interface
    await page.goto(`${BASE_URL}/manager`);
    
    // Wait for page load
    await page.waitForSelector('input[type="email"]', { timeout: 10000 });
    
    // Login to the manager (if there's a login form)
    await page.fill('input[type="email"]', 'admin@admin.com');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    
    // Wait for dashboard
    await page.waitForTimeout(2000);
    
    // Navigate to Pages section
    await page.click('a[href*="pages"]');
    await page.waitForTimeout(1000);
    
    // Create a new page
    await page.click('button:has-text("Add"), a:has-text("Add")');
    await page.waitForTimeout(1000);
    
    // Fill page details
    await page.fill('input[name="title"]', `Load Test Page ${new Date().getTime()}`);
    await page.fill('input[name="slug"]', `load-test-page-${new Date().getTime()}`);
    
    // Save the page
    await page.click('button:has-text("Save"), input[value="Save"]');
    await page.waitForTimeout(2000);
    
    // Check if page was created successfully
    const success = await page.isVisible('.alert-success, .notification-success');
    check(page, {
      'page created successfully': () => success,
    });
    
    // Navigate back to pages list to see the new page
    await page.click('a[href*="pages"]');
    await page.waitForTimeout(1000);
    
    sleep(2);
    
  } catch (error) {
    console.error('Browser test error:', error);
  } finally {
    page.close();
  }
}