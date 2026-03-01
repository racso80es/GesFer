import sys

# Given the memory "Unit tests for `DbInitializer` (`src/Product/Back/IntegrationTests/Services/DbInitializerTests.cs`) construct a `ServiceCollection` to register dependencies. They use real implementations for `JsonDataSeeder` and `ProductIntegrityService`, but mock `IMigrationService`, `IHostEnvironment`, and `ISensitiveDataSanitizer`. `IConfiguration` must be registered explicitly.", setup services typically use InMemoryDatabase or real dependencies because they instantiate a real ApplicationDbContext inside JsonDataSeeder.
# So I should leave `UseInMemoryDatabase` for `SetupServiceTests.cs` and only refactor the domain/application Handlers tests.
# The audit explicitly says: "Campaña de Cobertura Unitarios (Producto): Priorizar la creación de tests unitarios puros (usando Mocks para DbContext/Repositorios) para GesFer.Product.Back" which we have done.
# And "Refactorización a Tests Unitarios Puros: Evaluar la introducción de tests que no dependan de UseInMemoryDatabase para lógica de negocio pura, mejorando la velocidad y aislamiento." SetupService is not logic business pure, it's infrastructure.
# I will revert SetupServiceTests.cs to how it was before.

import subprocess
subprocess.run("git checkout src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs", shell=True)
