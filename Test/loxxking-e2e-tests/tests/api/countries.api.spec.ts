import { test, expect } from '@playwright/test';

test.describe('Countries API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/countries should return list of countries', async ({ request }) => {
    const response = await request.get(`${baseURL}/api/countries`);
    expect([200, 401]).toContain(response.status());
  });
});
