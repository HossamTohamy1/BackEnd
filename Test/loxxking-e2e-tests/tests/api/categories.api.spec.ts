import { test, expect } from '@playwright/test';

test.describe('Categories API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/categories should return a list of categories', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/categories`);
    expect(response.ok()).toBeTruthy(); 
  });

  test('POST /api/categories should reject unauthorized requests', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/categories`, {
      data: { name: 'New Category' }
    });
    expect(response.status()).toBe(401);
  });
});
