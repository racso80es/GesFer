import { _electron as electron, test, expect } from '@playwright/test';
import path from 'path';

test('Application launch', async () => {
  // Launch the application using the local electron binary and pointing to the current directory
  // This requires the app to be built (dist-electron/main.js must exist)
  const electronApp = await electron.launch({
    args: [path.join(__dirname, '../../')], // Points to package root
    env: {
      NODE_ENV: 'test',
    },
  });

  // Get the first window
  const window = await electronApp.firstWindow();

  // Wait for the window to load
  await window.waitForLoadState('domcontentloaded');

  // Check title
  const title = await window.title();
  // Expect title to be 'Calma Desktop' or similar (depends on main.ts config)
  // Let's log it to be sure if it fails
  console.log(`Window title: ${title}`);

  // Basic check
  expect(title).not.toBe('');

  // Close app
  await electronApp.close();
});
