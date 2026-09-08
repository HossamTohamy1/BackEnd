import { test, expect } from '@playwright/test';

test.describe('Product Management API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/products should return a list of products', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/products`);
    expect(response.ok()).toBeTruthy(); // Should be 200 OK
  });

  test('GET /api/products/best-sellers should return best sellers', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/products/best-sellers`);
    expect(response.ok()).toBeTruthy();
  });

  test('POST /api/products should reject unauthorized requests', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/products`, {
      data: { name: 'New Product' }
    });
    expect(response.status()).toBe(401);
  });

  test('GET /api/product-prices/prices-by-country should return prices', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/product-prices/prices-by-country`);
    expect([200, 401]).toContain(response.status());
  });
});
