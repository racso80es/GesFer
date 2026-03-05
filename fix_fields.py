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

    # The issue is that python's "replace" does not just replace it once, but rather it's duplicating it
    # We will remove ALL declarations of those lists and Mock contexts,
    # and then add them back right after the class declaration line.

    # 1. Clean up ALL private readonly for these specific variables
    content = re.sub(r"^\s*private readonly ApplicationDbContext _context;\s*\n", "", content, flags=re.MULTILINE)
    content = re.sub(r"^\s*private readonly Mock<ApplicationDbContext> _contextMock;\s*\n", "", content, flags=re.MULTILINE)
    content = re.sub(r"^\s*private readonly List<GesFer\.Product\.Back\.Domain\.Entities\.Company> _companies[^\n]*\n", "", content, flags=re.MULTILINE)
    content = re.sub(r"^\s*private readonly List<ArticleFamily> _articleFamilies[^\n]*\n", "", content, flags=re.MULTILINE)
    content = re.sub(r"^\s*private readonly List<TaxType> _taxTypes[^\n]*\n", "", content, flags=re.MULTILINE)

    # Also clean up any that might have been accidentally inline

    match = re.search(r"public class (\w+)", content)
    if not match:
        continue
    class_name = match.group(1)

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
