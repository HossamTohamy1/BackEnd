import { test, expect } from '@playwright/test';

test.describe('Users & Auth API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('POST /api/users/login should reject invalid credentials', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/users/login`, {
      data: {
        email: 'fakeuser@example.com',
        password: 'wrongpassword'
      }
    });

    expect(response.status()).toBeGreaterThanOrEqual(400);
  });

  test('GET /api/users/me should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/users/me`);
    expect(response.status()).toBe(401);
  });

  test('GET /api/users/staff should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/users/staff`);
    expect(response.status()).toBe(401);
  });

});
