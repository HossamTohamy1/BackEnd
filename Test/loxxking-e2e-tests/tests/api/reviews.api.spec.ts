import { test, expect } from '@playwright/test';

test.describe('Reviews API Endpoints', () => {
  const baseURL = 'http://localhost:5050';

  test('GET /api/reviews/product/{id} should return reviews for a product', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-000000000000';
    const response = await request.get(`${baseURL}/api/reviews/product/${fakeId}`);
    expect(response.ok()).toBeTruthy(); // usually returns 200 [] even if not found
  });

  test('POST /api/reviews should reject unauthorized requests', async ({ request }) => {
    const response = await request.post(`${baseURL}/api/reviews`, {
      data: { rating: 5, comment: 'Great!' }
    });
    expect([401, 400]).toContain(response.status());
  });
});
