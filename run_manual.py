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

    # Step 1: Replace usings
    if "using Moq;" not in content:
        content = "using Moq;\n" + content
    if "using MockQueryable.Moq;" not in content:
        content = "using MockQueryable.Moq;\n" + content
    if "using GesFer.Product.UnitTests.Infrastructure;" not in content:
        content = "using GesFer.Product.UnitTests.Infrastructure;\n" + content

    # Replace old constructor string
    constructor_string = r"""var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);"""

    content = content.replace(constructor_string, """var mockCompanies = _companies.BuildMockDbSet();
        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        _contextMock = new Mock<ApplicationDbContext>();
        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);

        _context = _contextMock.Object;""")

    # In UpdateArticleFamilyTests, handle the specific cases

    # Check if there are internal UseInMemoryDatabase uses
    content = re.sub(
        r"var options\s*=\s*new DbContextOptionsBuilder<ApplicationDbContext>\(\)\s*\n\s*\.UseInMemoryDatabase\(databaseName: Guid\.NewGuid\(\)\.ToString\(\)\)\s*\n\s*\.Options;\s*\n\s*using var context = new ApplicationDbContext\(options\);",
        """var mockCompanies = _companies.BuildMockDbSet();
        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        var contextMock = new Mock<ApplicationDbContext>();
        contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);

        var context = contextMock.Object;""", content
    )

    # Replaces
    content = re.sub(r"private readonly ApplicationDbContext _context;", """private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();""", content)

    # Step 5: Inline Act Uses
    content = re.sub(r"_context\.ArticleFamilies\.Add\((.*?)\);", r"_articleFamilies.Add(\1);", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\((.*?)\);", r"_articleFamilies.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\(\n\s*(.*?)\n\s*\);", r"_articleFamilies.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    content = re.sub(r"_context\.TaxTypes\.Add\((.*?)\);", r"_taxTypes.Add(\1);", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\((.*?)\);", r"_taxTypes.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\(\n\s*(.*?)\n\s*\);", r"_taxTypes.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    content = re.sub(r"await _context\.SaveChangesAsync\(\);", r"// await _context.SaveChangesAsync();", content)

    # Step 6: FindAsync replacing
    content = re.sub(r"await _context\.ArticleFamilies\.FindAsync\((.*?)\)", r"_articleFamilies.SingleOrDefault(x => x.Id == \1)", content)
    content = re.sub(r"await _context\.TaxTypes\.FindAsync\((.*?)\)", r"_taxTypes.SingleOrDefault(x => x.Id == \1)", content)

    with open(filepath, "w") as f:
        f.write(content)
