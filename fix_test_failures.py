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

    # FindAsync uses object[] ids. However, entity.TaxTypeId is of type Guid?
    # Actually wait: The EF Core FindAsync accepts params object[]. The length is 1, and it's a Guid.

    # Why is "El tipo de tasa no existe o no pertenece a la empresa" failing in CreateArticleFamilyTests?
    # Because IsActive = true, CompanyId = companyId... Wait, `_context.TaxTypes.Add` is missing the SetupMock because `_context.TaxTypes.Add` in the test uses the `_context` which is MOCKED. If `_taxTypes.Add` is NOT called directly, then the mock's `.Add` MUST be defined to call `_taxTypes.Add`.

    # We replaced `_context.TaxTypes.Add` with `_taxTypes.Add` in some places but not all! Let's check the test:
    # `_context.TaxTypes.Add(new TaxType` - AH! It says `_context.TaxTypes.Add` in the file.
    # Why did my previous regex miss it? Because of newlines!

    content = content.replace("_context.TaxTypes.Add(", "_taxTypes.Add(")
    content = content.replace("_context.ArticleFamilies.Add(", "_articleFamilies.Add(")

    with open(filepath, "w") as f:
        f.write(content)
