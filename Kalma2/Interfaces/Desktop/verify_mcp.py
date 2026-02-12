from playwright.sync_api import sync_playwright

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    page = browser.new_page()

    # Mock window.calmaAPI
    page.add_init_script("""
        window.calmaAPI = {
            onStatusChange: (callback) => {
                // Simulate initial status
                setTimeout(() => callback({}), 100);
                return () => {};
            },
            startSequence: () => console.log('startSequence'),
            stopAll: () => console.log('stopAll'),
            runAudit: () => console.log('runAudit'),
            clearCache: () => console.log('clearCache'),
            syncSpec: () => console.log('syncSpec'),
        };
    """)

    try:
        print("Navigating to app...")
        page.goto("http://localhost:4173")

        # Check MCP Section
        print("Checking for MCP Project Header...")
        page.wait_for_selector("text=Active Project (MCP)", timeout=5000)
        print("Found MCP Header.")

        # Check Button
        print("Checking for Audit Button...")
        button = page.locator("button:has-text('Audit Process (IOTA)')")
        if button.is_visible():
            print("Audit Button Visible.")
            button.click()
            print("Clicked Audit Button.")
        else:
            print("Audit Button NOT Visible.")

        # Wait for Success State
        # It might take a few seconds due to network or simulation delay
        print("Waiting for Verification...")
        try:
            # Wait for the "VERIFIED" badge
            page.wait_for_selector("text=VERIFIED", timeout=15000)
            print("Verification Successful!")
        except Exception as e:
            print(f"Verification Timed Out: {e}")

        # Take Screenshot
        page.screenshot(path="/home/jules/verification/mcp_audit_verified.png", full_page=True)
        print("Screenshot saved to /home/jules/verification/mcp_audit_verified.png")

    except Exception as e:
        print(f"Error: {e}")
        page.screenshot(path="/home/jules/verification/error_state.png", full_page=True)
    finally:
        browser.close()

with sync_playwright() as playwright:
    run(playwright)
