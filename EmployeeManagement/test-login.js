const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ 
    headless: false, 
    slowMo: 1000,
    devtools: true 
  });
  
  const page = await browser.newPage();
  
  // Enable console logging
  page.on('console', msg => {
    console.log(`BROWSER CONSOLE [${msg.type()}]:`, msg.text());
  });
  
  // Enable error logging
  page.on('pageerror', err => {
    console.log('PAGE ERROR:', err.message);
  });
  
  try {
    console.log('Navigating to login page...');
    await page.goto('http://localhost:5004/login');
    
    console.log('Waiting for page to load...');
    await page.waitForTimeout(3000);
    
    console.log('Taking screenshot of login page...');
    await page.screenshot({ path: 'login-page.png' });
    
    console.log('Checking if login form elements exist...');
    const userIdField = await page.locator('input[aria-label="ユーザーID"]').first();
    const passwordField = await page.locator('input[aria-label="パスワード"]').first();
    const loginButton = await page.locator('button:has-text("ログイン")').first();
    
    console.log('UserID field exists:', await userIdField.count() > 0);
    console.log('Password field exists:', await passwordField.count() > 0);
    console.log('Login button exists:', await loginButton.count() > 0);
    
    if (await loginButton.count() > 0) {
      console.log('Login button is visible:', await loginButton.isVisible());
      console.log('Login button is enabled:', await loginButton.isEnabled());
    }
    
    console.log('Filling in login credentials...');
    await userIdField.fill('admin');
    await passwordField.fill('password');
    
    console.log('Taking screenshot before clicking login...');
    await page.screenshot({ path: 'before-login-click.png' });
    
    console.log('Clicking login button...');
    await loginButton.click();
    
    console.log('Waiting for response...');
    await page.waitForTimeout(5000);
    
    console.log('Taking screenshot after clicking login...');
    await page.screenshot({ path: 'after-login-click.png' });
    
    console.log('Current URL:', page.url());
    
    // Check for any snackbar messages
    const snackbar = await page.locator('.mud-snackbar').first();
    if (await snackbar.count() > 0) {
      console.log('Snackbar message:', await snackbar.textContent());
    }
    
  } catch (error) {
    console.error('Test failed:', error);
  } finally {
    await browser.close();
  }
})();