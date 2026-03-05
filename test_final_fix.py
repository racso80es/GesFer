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

    # The issue is multiple replacements of `private readonly ApplicationDbContext _context;`
    # Let's remove them all first and then inject them once at the top of the class definition

    # Identify the class declaration
    class_match = re.search(r"public class (\w+)", content)
    if not class_match:
        continue
    class_name = class_match.group(1)

    # Erase any matching lines
    content = re.sub(r"private readonly Mock<ApplicationDbContext> _contextMock;\n?", "", content)
    content = re.sub(r"private readonly List<GesFer\.Product\.Back\.Domain\.Entities\.Company> _companies = new\(\);\n?", "", content)
    content = re.sub(r"private readonly List<ArticleFamily> _articleFamilies = new\(\);\n?", "", content)
    content = re.sub(r"private readonly List<TaxType> _taxTypes = new\(\);\n?", "", content)
    content = re.sub(r"private readonly ApplicationDbContext _context;\n?", "", content)

    # Re-insert the single block
    class_decl = f"public class {class_name}\n{{"
    new_fields = f"""    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();
"""

    # We do a replacement ONLY once for the class declaration
    content = content.replace(class_decl, class_decl + "\n" + new_fields, 1)

    with open(filepath, "w") as f:
        f.write(content)
