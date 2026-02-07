from playwright.sync_api import sync_playwright

def run():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        try:
            print("Navigating to login page...")
            page.goto("http://localhost:3000/login")

            print("Waiting for network idle...")
            # Use wait_for_load_state in Python
            page.wait_for_load_state("networkidle")

            print("Checking content...")
            content = page.content()

            if "Organización" in content:
                print("SUCCESS: Found 'Organización' in page content.")
            else:
                print("FAILURE: Did not find 'Organización' in page content.")

            if "Empresa" in content:
                 print("WARNING: Found 'Empresa' in page content.")

            print("Taking screenshot...")
            page.screenshot(path="verification_login.png")
            print("Screenshot saved to verification_login.png")

        except Exception as e:
            print(f"Error: {e}")
        finally:
            browser.close()

if __name__ == "__main__":
    run()
