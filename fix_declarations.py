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

    # The issue is multiple replacements added multiple declarations for _contextMock, etc.

    # We remove all matches for private fields except _context and _handler (which were original)
    # Then we add exactly ONE block of the new fields at the top of the class.

    # Let's extract the class name
    match = re.search(r"public class (\w+)", content)
    if not match:
        continue
    class_name = match.group(1)

    content = re.sub(r"private readonly Mock<ApplicationDbContext> _contextMock;\n?", "", content)
    content = re.sub(r"private readonly List<GesFer\.Product\.Back\.Domain\.Entities\.Company> _companies = new\(\);\n?", "", content)
    content = re.sub(r"private readonly List<ArticleFamily> _articleFamilies = new\(\);\n?", "", content)
    content = re.sub(r"private readonly List<TaxType> _taxTypes = new\(\);\n?", "", content)
    content = re.sub(r"private readonly ApplicationDbContext _context;\n?", "", content)

    # We will inject them right before the class constructor
    class_decl = f"public class {class_name}\n{{"
    new_fields = f"""    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();
"""

    content = content.replace(class_decl, class_decl + "\n" + new_fields)

    with open(filepath, "w") as f:
        f.write(content)
