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

    if "UseInMemoryDatabase" not in content:
        continue

    if "using Moq;" not in content:
        content = "using Moq;\n" + content
    if "using MockQueryable.Moq;" not in content:
        content = "using MockQueryable.Moq;\n" + content
    if "using GesFer.Product.UnitTests.Infrastructure;" not in content:
        content = "using GesFer.Product.UnitTests.Infrastructure;\n" + content

    content = re.sub(r"private readonly ApplicationDbContext _context;", """private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();""", content)

    db_context_init = r"var\s+options\s*=\s*new\s+DbContextOptionsBuilder<ApplicationDbContext>\(\)\s*\n\s*\.UseInMemoryDatabase\(databaseName:\s*Guid\.NewGuid\(\)\.ToString\(\)\)\s*\n\s*\.Options;\s*\n\s*(_context|using var context)\s*=\s*new\s+ApplicationDbContext\(options\);"

    def replace_dbcontext_init(match):
        context_var_name = match.group(1)
        if context_var_name == "using var context":
            context_var_name = "var context"

        return f"""var mockCompanies = _companies.BuildMockDbSet();
        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        _contextMock = new Mock<ApplicationDbContext>();
        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);

        {context_var_name} = _contextMock.Object;"""

    content = re.sub(db_context_init, replace_dbcontext_init, content)

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

    # Replace Adds
    content = re.sub(r"_context\.ArticleFamilies\.Add\((.*?)\);", r"_articleFamilies.Add(\1);", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\((.*?)\);", r"_articleFamilies.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\(\n\s*(.*?)\n\s*\);", r"_articleFamilies.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    content = re.sub(r"_context\.TaxTypes\.Add\((.*?)\);", r"_taxTypes.Add(\1);", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\((.*?)\);", r"_taxTypes.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\(\n\s*(.*?)\n\s*\);", r"_taxTypes.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    # Handle context instead of _context
    content = re.sub(r"context\.TaxTypes\.Add\((.*?)\);", r"_taxTypes.Add(\1);", content)

    # SaveChangesAsync
    content = re.sub(r"await _context\.SaveChangesAsync\(\);", r"// await _context.SaveChangesAsync();", content)
    content = re.sub(r"await context\.SaveChangesAsync\(\);", r"// await context.SaveChangesAsync();", content)

    # FindAsync
    content = re.sub(r"await _context\.ArticleFamilies\.FindAsync\((.*?)\)", r"Task.FromResult(_articleFamilies.SingleOrDefault(x => x.Id == \1))", content)
    content = re.sub(r"await _context\.TaxTypes\.FindAsync\((.*?)\)", r"Task.FromResult(_taxTypes.SingleOrDefault(x => x.Id == \1))", content)

    content = re.sub(r"await context\.ArticleFamilies\.FindAsync\((.*?)\)", r"Task.FromResult(_articleFamilies.SingleOrDefault(x => x.Id == \1))", content)
    content = re.sub(r"await context\.TaxTypes\.FindAsync\((.*?)\)", r"Task.FromResult(_taxTypes.SingleOrDefault(x => x.Id == \1))", content)

    with open(filepath, "w") as f:
        f.write(content)

print("Applied rewritten regex")
