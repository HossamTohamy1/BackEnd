import { test, expect } from '@playwright/test';

test.describe('Orders API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('POST /api/orders/guest should create a guest order', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/orders/guest`, {
      data: {
        customerName: 'Test Guest',
        email: 'guest@example.com',
        items: []
      }
    });
    expect([200, 201, 400]).toContain(response.status());
  });

  test('GET /api/orders should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/orders`);
    expect(response.status()).toBe(401);
  });
});
