import { test, expect } from '@playwright/test';

test.describe('Invoices API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/invoices should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/invoices`);
    expect(response.status()).toBe(401);
  });
});
