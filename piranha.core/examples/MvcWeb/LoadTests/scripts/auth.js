import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const ENVIRONMENT = __ENV.ENVIRONMENT || 'Testing';

export class AuthHelper {
    constructor() {
        this.baseUrl = BASE_URL;
        this.headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        this.cookies = {};
    }

    /**
     * Login to Piranha CMS Manager
     * @param {string} username - Username for login
     * @param {string} password - Password for login
     * @returns {object} - Authentication token/cookies
     */
    login(username = 'admin', password = 'password') {
        const loginUrl = `${this.baseUrl}/manager/login`;
        
        // First, get the login page to retrieve any CSRF tokens
        const loginPageRes = http.get(loginUrl);
        
        // Extract CSRF token if present
        const csrfToken = this.extractCsrfToken(loginPageRes.body);
        
        // Prepare login payload
        const payload = JSON.stringify({
            Username: username,
            Password: password,
            ReturnUrl: '/manager'
        });

        const params = {
            headers: {
                ...this.headers,
                'X-CSRF-TOKEN': csrfToken || ''
            },
            redirects: 0
        };

        // Perform login
        const loginRes = http.post(loginUrl, payload, params);
        
        check(loginRes, {
            'login successful': (r) => r.status === 200 || r.status === 302,
            'authentication cookie received': (r) => r.cookies && Object.keys(r.cookies).length > 0
        });

        // Store cookies for subsequent requests
        if (loginRes.cookies) {
            this.cookies = loginRes.cookies;
        }

        // Store the authentication cookie
        const authCookie = this.extractAuthCookie(loginRes);
        if (authCookie) {
            this.headers['Cookie'] = authCookie;
        }

        return {
            cookies: this.cookies,
            headers: this.headers,
            isAuthenticated: true
        };
    }

    /**
     * Get headers for anonymous testing (when in Testing environment)
     * @returns {object} - Headers for testing
     */
    getTestingHeaders() {
        return {
            ...this.headers,
            'X-Testing-Environment': 'true',
            'X-Environment': ENVIRONMENT
        };
    }

    /**
     * Get authenticated headers
     * @returns {object} - Headers with authentication
     */
    getAuthHeaders() {
        return this.headers;
    }

    /**
     * Extract CSRF token from HTML response
     * @param {string} html - HTML response body
     * @returns {string} - CSRF token or null
     */
    extractCsrfToken(html) {
        const match = html.match(/name="__RequestVerificationToken".*?value="([^"]+)"/);
        return match ? match[1] : null;
    }

    /**
     * Extract authentication cookie from response
     * @param {object} response - HTTP response
     * @returns {string} - Cookie string
     */
    extractAuthCookie(response) {
        if (!response.cookies) return null;
        
        const cookieStrings = [];
        for (const [name, value] of Object.entries(response.cookies)) {
            cookieStrings.push(`${name}=${value}`);
        }
        
        return cookieStrings.join('; ');
    }

    /**
     * Create a request with proper authorization
     * @param {string} method - HTTP method
     * @param {string} url - URL to request
     * @param {object} body - Request body
     * @param {boolean} requiresAuth - Whether request requires authentication
     * @returns {object} - HTTP response
     */
    makeRequest(method, url, body = null, requiresAuth = true) {
        const params = {
            headers: requiresAuth ? this.getAuthHeaders() : this.getTestingHeaders()
        };

        switch (method.toUpperCase()) {
            case 'GET':
                return http.get(url, params);
            case 'POST':
                return http.post(url, body ? JSON.stringify(body) : null, params);
            case 'PUT':
                return http.put(url, body ? JSON.stringify(body) : null, params);
            case 'DELETE':
                return http.del(url, params);
            default:
                throw new Error(`Unsupported method: ${method}`);
        }
    }
}

/**
 * Shared authentication instance
 */
export const auth = new AuthHelper();

/**
 * Setup function to be called in test setup
 */
export function setupAuth() {
    // In Testing environment, we might not need real authentication
    if (ENVIRONMENT === 'Testing') {
        console.log('Running in Testing environment - using anonymous access');
        return {
            headers: auth.getTestingHeaders(),
            isAuthenticated: false,
            environment: ENVIRONMENT
        };
    }
    
    // For other environments, perform actual login
    console.log('Performing authentication...');
    return auth.login();
}