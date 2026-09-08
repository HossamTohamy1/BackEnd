import { test, expect } from '@playwright/test';

test.describe('Notifications API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/notifications should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/notifications`);
    expect(response.status()).toBe(401);
  });

  test('PATCH /api/notifications/read-all should reject unauthorized requests', async ({ request }) => {
    const response = await request.patch(`${baseURL}/api/notifications/read-all`);
    expect(response.status()).toBe(401);
  });
});
