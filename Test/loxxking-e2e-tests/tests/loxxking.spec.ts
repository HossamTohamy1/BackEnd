import { test, expect } from '@playwright/test';

test.describe('Loxxking Fullstack E2E Tests', () => {

  test('Homepage loads correctly and Angular boots', async ({ page }) => {
    await page.goto('http://localhost:4200/');
    const body = await page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Storefront Login API integration', async ({ page }) => {
    await page.goto('http://localhost:4200/login');
    
    const emailInput = page.locator('input[type="email"], input[name="email"], .lk-login-field--email input');
    await expect(emailInput.first()).toBeVisible();
    await emailInput.first().fill('test@example.com');

    const passwordInput = page.locator('input[type="password"], input[name="password"], .lk-login-field--password input');
    await passwordInput.first().fill('wrongpassword123');

    const submitBtn = page.locator('button[type="submit"], button:has-text("ØªØ³Ø¬ÙŠÙ„ Ø§Ù„Ø¯Ø®ÙˆÙ„"), button:has-text("Login")');
    
    const responsePromise = page.waitForResponse(response => 
      response.url().includes('/api/users/login') || response.url().includes('/api/auth/login')
    );

    await submitBtn.first().click();

    const response = await responsePromise;
    
    expect(response.status()).toBeGreaterThanOrEqual(400); 
  });

});

