import { test, expect } from '@playwright/test';

test.describe('Inventories API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/inventories should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/inventories`);
    expect(response.status()).toBe(401);
  });
});
