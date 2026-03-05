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

    if "using Moq;" not in content:
        content = "using Moq;\n" + content
    if "using MockQueryable.Moq;" not in content:
        content = "using MockQueryable.Moq;\n" + content
    if "using GesFer.Product.UnitTests.Infrastructure;" not in content:
        content = "using GesFer.Product.UnitTests.Infrastructure;\n" + content

    content = content.replace("private readonly ApplicationDbContext _context;", """private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();""")

    # Replace constructor logic
    constructor_pattern = re.compile(r"public\s+\w+\(\)\s*\{[^\}]*UseInMemoryDatabase[^\}]*\}", re.MULTILINE | re.DOTALL)

    def replacement_func(match):
        class_name = match.group(0).split('public')[1].split('(')[0].strip()
        handler_name = ""
        # Find the handler instantiation
        handler_match = re.search(r"_handler\s*=\s*new\s+(\w+)\s*\(", match.group(0))
        if handler_match:
            handler_name = handler_match.group(1)

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
        return setup

    content = constructor_pattern.sub(replacement_func, content)

    # Replace DbContext Adds and SaveChangesAsync
    content = re.sub(r"_context\.ArticleFamilies\.Add\((.*?)\);", r"_articleFamilies.Add(\1);", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\((.*?)\);", r"_articleFamilies.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.ArticleFamilies\.AddRange\(\n\s*(.*?)\n\s*\);", r"_articleFamilies.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    content = re.sub(r"_context\.TaxTypes\.Add\((.*?)\);", r"_taxTypes.Add(\1);", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\((.*?)\);", r"_taxTypes.AddRange(new[] { \1 });", content)
    content = re.sub(r"_context\.TaxTypes\.AddRange\(\n\s*(.*?)\n\s*\);", r"_taxTypes.AddRange(new[] { \1 });", content, flags=re.DOTALL)

    content = re.sub(r"await _context\.SaveChangesAsync\(\);", r"// await _context.SaveChangesAsync();", content)

    # Replace FindAsync
    content = re.sub(r"await _context\.ArticleFamilies\.FindAsync\((.*?)\)", r"_articleFamilies.SingleOrDefault(x => x.Id == \1)", content)
    content = re.sub(r"await _context\.TaxTypes\.FindAsync\((.*?)\)", r"_taxTypes.SingleOrDefault(x => x.Id == \1)", content)

    # Handle the ones inside Act
    content = re.sub(r"var result = await _handler\.HandleAsync\(command\);", r"var result = await _handler.HandleAsync(command);", content)

    # Some variables like updated might need Task unwrapping if we returned Task.FromResult
    # But since it's an extension method of DbSet, Moq MockQueryable might support FindAsync directly?
    # Actually FindAsync is hard to mock for MockQueryable if DbSet isn't specifically mocked for FindAsync.
    # The best way is to replace FindAsync with Task.FromResult(_list.FirstOrDefault(e => e.Id == id)).
    # Wait, FindAsync is returning ValueTask in newer EF, so just avoiding it in tests or mocking it:
    # We replaced it with `_articleFamilies.SingleOrDefault` so we must remove the `await` keyword.
    # We'll replace `await _context.Entities.FindAsync(id)` with `_entities.SingleOrDefault(e => e.Id == id)`
    # Which we did above. BUT `FindAsync` used `await`.
    content = re.sub(r"await\s+_articleFamilies\.SingleOrDefault", r"_articleFamilies.SingleOrDefault", content)
    content = re.sub(r"await\s+_taxTypes\.SingleOrDefault", r"_taxTypes.SingleOrDefault", content)

    with open(filepath, "w") as f:
        f.write(content)
