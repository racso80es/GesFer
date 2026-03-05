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

    # The actual reason might be that MockDbSet isn't behaving well if we modify the list AFTER it's mocked,
    # OR maybe we forgot to await the Result since we changed the async behaviour.
    # Actually, BuildMockDbSet is meant to be called ONCE after the list is FULLY populated.
    # Let's fix this by finding every [Fact] and adding the correct setup before `_handler.HandleAsync`.

    content = content.replace("        _contextMock.Setup(c => c.Companies).Returns(_companies.BuildMockDbSet().Object);\n        _contextMock.Setup(c => c.ArticleFamilies).Returns(_articleFamilies.BuildMockDbSet().Object);\n        _contextMock.Setup(c => c.TaxTypes).Returns(_taxTypes.BuildMockDbSet().Object);\n", "")

    # Let's just create a more robust SetupMocks:
    robust_setup = """
        var mockCompanies = _companies.BuildMockDbSet();
        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);
"""
    content = content.replace("private void SetupMocks()\n    {\n", "private void SetupMocks()\n    {\n" + robust_setup)

    with open(filepath, "w") as f:
        f.write(content)
