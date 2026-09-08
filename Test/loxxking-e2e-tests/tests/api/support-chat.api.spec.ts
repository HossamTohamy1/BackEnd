import { test, expect } from '@playwright/test';

test.describe('Support Chat API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/support-chat/conversations should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/support-chat/conversations`);
    expect(response.status()).toBe(401);
  });

  test('POST /api/support-chat/send should reject unauthorized requests', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/support-chat/send`, {
      data: { message: 'Hello' }
    });
    expect(response.status()).toBe(401);
  });
});
