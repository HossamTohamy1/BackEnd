import { test, expect } from '@playwright/test';

test.describe('Favorites API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/favorites should reject unauthorized requests', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/favorites`);
    expect(response.status()).toBe(401);
  });

  test('GET /api/favorites-config should return config', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/favorites-config`);
    expect(response.ok()).toBeTruthy();
  });
});
