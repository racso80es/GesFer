#!/bin/bash

# Start Mock API in background
node tests/mock-api.js > mock.log 2>&1 &
MOCK_PID=$!

echo "Mock API started with PID $MOCK_PID"

# Export env vars for Next.js to use Mock API
export ADMIN_API_URL=http://localhost:5050
export NEXT_PUBLIC_API_URL=http://localhost:5050
export AUTH_SECRET=secret-for-testing

# Run Playwright tests
# Ensure we only run the mocked test file
npx playwright test tests/admin-e2e-mocked.spec.ts

EXIT_CODE=$?

# Cleanup
kill $MOCK_PID
echo "Mock API stopped"

exit $EXIT_CODE
