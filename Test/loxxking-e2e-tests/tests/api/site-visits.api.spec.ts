import { test, expect } from '@playwright/test';

test.describe('Site Visits API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('POST /api/site-visits should log a visit', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/site-visits`);
    expect(response.ok()).toBeTruthy();
  });

  test('GET /api/site-visits/today-count should return count', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/site-visits/today-count`);
    expect(response.ok()).toBeTruthy();
  });
});
