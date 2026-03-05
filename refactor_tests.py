import os
import re

files_to_update = [
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/DeleteArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/UpdateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetAllArticleFamiliesTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/TaxTypes/CreateTaxTypeTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"
]

for filepath in files_to_update:
    if not os.path.exists(filepath):
        continue

    with open(filepath, "r") as f:
        content = f.read()

    # Add necessary usings
    if "using Moq;" not in content:
        content = "using Moq;\n" + content
    if "using MockQueryable.Moq;" not in content:
        content = "using MockQueryable.Moq;\n" + content
    if "using GesFer.Product.UnitTests.Infrastructure;" not in content:
        content = "using GesFer.Product.UnitTests.Infrastructure;\n" + content

    if "ApplicationDbContext _context;" in content:
        content = content.replace("ApplicationDbContext _context;", "Mock<ApplicationDbContext> _contextMock;\n    private readonly ApplicationDbContext _context;")

    if "public UpdateArticleFamilyTests()" in content:
        content = re.sub(
            r"public UpdateArticleFamilyTests\(\)\s*\{\s*var options = new DbContextOptionsBuilder<ApplicationDbContext>\(\)\s*\.UseInMemoryDatabase\(databaseName: Guid\.NewGuid\(\)\.ToString\(\)\)\s*\.Options;\s*_context = new ApplicationDbContext\(options\);\s*_handler = new UpdateArticleFamilyCommandHandler\(_context\);\s*\}",
            """public UpdateArticleFamilyTests()
    {
        _contextMock = new Mock<ApplicationDbContext>();
        _context = _contextMock.Object;
        _handler = new UpdateArticleFamilyCommandHandler(_context);
    }""", content
        )

    # I will process the files individually since they have different structures. Let's start by doing it manually using sed and python.

print("Initialized script for manual edits.")
