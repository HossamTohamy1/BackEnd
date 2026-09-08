import { test, expect } from '@playwright/test';

test.describe('Offers API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/offers should return list of offers', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/offers`);
    expect(response.ok()).toBeTruthy();
  });

  test('GET /api/offers-page-config should return config', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/offers-page-config`);
    expect(response.ok()).toBeTruthy();
  });

  test('GET /api/bundle-offers should return bundles', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/bundle-offers`);
    expect(response.ok()).toBeTruthy();
  });
});
