import os
import re

files_to_update = [
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/DeleteArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/UpdateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetAllArticleFamiliesTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/TaxTypes/CreateTaxTypeTests.cs"
]

for filepath in files_to_update:
    if not os.path.exists(filepath):
        continue

    with open(filepath, "r") as f:
        content = f.read()

    # The lists need to update the mocks BEFORE the test runs, and we also need to implement FindAsync() in Moq because that's what the Handler calls!
    # Wait! The handler calls:
    # var taxType = await _context.TaxTypes.FindAsync(new object[] { entity.TaxTypeId }, cancellationToken);
    # MockQueryable does NOT support FindAsync directly on the DbSet unless we set it up.
    # Actually, MockQueryable (version 8) supports FindAsync if we provide the Setup for it or if we use the underlying methods. But if we mock the DbSet completely, we can set up FindAsync.

    # Alternatively, the exception "El tipo de tasa no existe o no pertenece a la empresa" is thrown from `AnyAsync()`.
    # Why is AnyAsync failing?
    # Because `_context.TaxTypes.Add(...)` does not update the `_contextMock.Object.TaxTypes` automatically unless we run SetupMocks() AGAIN!

    # We must insert `SetupMocks();` right before calling `_handler.HandleAsync`!

    # Find every occurrence of `var result = await _handler.HandleAsync(command);`
    # Replace it with `SetupMocks();\n        var result = await _handler.HandleAsync(command);`
    content = content.replace("var result = await _handler.HandleAsync(command);", "SetupMocks();\n        var result = await _handler.HandleAsync(command);")
    content = content.replace("await _handler.Invoking(h => h.HandleAsync(command))", "SetupMocks();\n        await _handler.Invoking(h => h.HandleAsync(command))")

    # FindAsync mock setup in SetupMocks()
    find_async_setup = """
        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _taxTypes.SingleOrDefault(t => t.Id == id);
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _articleFamilies.SingleOrDefault(t => t.Id == id);
            });
    """

    content = content.replace("var mockArticleFamilies = _articleFamilies.BuildMockDbSet();\n        var mockTaxTypes = _taxTypes.BuildMockDbSet();", find_async_setup)

    with open(filepath, "w") as f:
        f.write(content)
