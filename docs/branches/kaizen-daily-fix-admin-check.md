# Branch Documentation: kaizen/daily-fix-admin-check

**Date:** 2026-02-24
**Author:** Agent (Jules)
**Purpose:** Fix missing `IAdminApiClient` registration in `GesFer.Console` to enable robust integrity checks during database initialization.

## Context
The `InitializeDatabaseCommand` in the Console application manually constructs the DI container but fails to register `IAdminApiClient`. This causes `DbInitializer.EnsureAdminUserAndSmokeTestAsync` to skip the validation of the Admin Company existence, potentially leaving the system in an inconsistent state where the Product DB references a non-existent Company in Admin.

## Changes
1.  **`src/Console/Commands/InitializeDatabaseCommand.cs`**:
    - Registered `IConfiguration` (singleton).
    - Registered `IAdminApiClient` via `AddHttpClient` with BaseUrl from config.
2.  **`src/Product/Back/Infrastructure/Data/DbInitializer.cs`**:
    - Added a warning log if `IAdminApiClient` is missing during initialization.

## Verification
- Build `GesFer.Console` successfully.
- Run `GesFer.Product.UnitTests` successfully.
- Verify code compilation and logic correctness.
