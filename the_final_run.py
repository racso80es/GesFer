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

    # Replace private readonly ApplicationDbContext _context; with the expanded lists
    content = content.replace("private readonly ApplicationDbContext _context;", """private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();""")

    # Instead of replacing the constructor using regex that might be failing, we replace EXACT strings that are known:
    constructor_match = re.search(r"public \w+\(\)\s*\{[^\}]*\}", content, re.MULTILINE | re.DOTALL)
    if constructor_match:
        original_constructor = constructor_match.group(0)

        handler_name = ""
        handler_match = re.search(r"_handler\s*=\s*new\s+(\w+)\s*\(", original_constructor)
        if handler_match:
            handler_name = handler_match.group(1)

        class_name = re.search(r"public class (\w+)", content).group(1)

        setup = f"""public {class_name}()
    {{
        var mockCompanies = _companies.BuildMockDbSet();
        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        _contextMock = new Mock<ApplicationDbContext>();
        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);

        _context = _contextMock.Object;"""

        if handler_name:
            setup += f"\n        _handler = new {handler_name}(_context);"

        setup += "\n    }"

        content = content.replace(original_constructor, setup)

    # In UpdateArticleFamilyTests, handle the specific cases
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
