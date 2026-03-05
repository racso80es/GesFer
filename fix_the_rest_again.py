import os
import re

files_to_update = [
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"
]

for filepath in files_to_update:
    if not os.path.exists(filepath):
        continue

    with open(filepath, "r") as f:
        content = f.read()

    # GetArticleFamilyByIdTests expects TaxTypeName to be "Tax 1". It calls FindAsync to get the TaxType.
    # Did we set up FindAsync correctly in GetArticleFamilyByIdTests?
    # Our SetupMocks contains:
    # _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);
    # But mockTaxTypes might not have FindAsync setup!

    # Wait, `GetArticleFamilyByIdCommandHandler` might use `_context.TaxTypes.FindAsync`?
    # Let's check `GetArticleFamilyByIdCommandHandler.cs` in the actual codebase (or just assume it does).
    # If it uses FindAsync, we MUST setup FindAsync.
    # Actually, `GetArticleFamilyByIdCommandHandler` returns a DTO which maps `TaxTypeName`.
    # Let's see how it gets it.
    # We can just add `.ReturnsAsync` to the DbSet for `FindAsync`.

    if "GetArticleFamilyByIdTests.cs" in filepath:
        find_async_setup = """
        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _taxTypes.SingleOrDefault(t => t.Id == id);
                return null;
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _articleFamilies.SingleOrDefault(t => t.Id == id);
                return null;
            });
"""
        content = content.replace("var mockArticleFamilies = _articleFamilies.BuildMockDbSet();\n        var mockTaxTypes = _taxTypes.BuildMockDbSet();", find_async_setup)


    # SetupServiceTests has: InitializeEnvironmentAsync_ShouldRunAllStepsAndSucceed
    # It says Docker failure? No, expected result.Success to be True, but found False.
    # The code: service.InitializeEnvironmentAsync();
    # Let's find why it returns false.
    # If there's an exception, it sets Success = false.
    # Let's fix the mock in SetupServiceTests.

    if "SetupServiceTests.cs" in filepath:
        # In InitializeEnvironmentAsync, it might be failing because we are using an invalid mock configuration or we are not mocking the Docker steps properly.
        # Wait, the docker steps were mocked by overriding `ExecuteDockerCommandAsync`.
        # Maybe `WaitForMySqlReadyAsync` was not mock-setup properly?
        # Maybe `JsonDataSeeder` failed because `_dbContext` cannot be saved because it's a mock with no `.SaveChangesAsync` setup?
        # SetupService calls `await jsonSeeder.SeedAllAsync();` and `await _dbContext.Database.MigrateAsync();`
        # Because `_dbContext` is a mock (`Mock<ApplicationDbContext>`), `_dbContext.Database` will be null, and `MigrateAsync()` will throw a `NullReferenceException`!

        # We need to mock `Database` property on DbContext, or just catch it.
        # Or better yet, we don't mock `Database.MigrateAsync` easily.
        # Wait, the original code used `UseInMemoryDatabase` for a reason: it provides a REAL DbContext with a working `Database` property!
        # Actually, maybe we should revert SetupServiceTests to use InMemoryDatabase?
        # The audit says: "Priorizar la creación de tests unitarios puros (usando Mocks para DbContext/Repositorios) para GesFer.Product.Back"
        # Since `SetupServiceTests` is not a domain entity but an infrastructure service, it might be allowed to use InMemory, OR we just mock `IApplicationDbContext`?
        # ApplicationDbContext doesn't have an interface.
        pass

    with open(filepath, "w") as f:
        f.write(content)
