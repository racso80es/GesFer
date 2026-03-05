import os
import re

files_to_update = [
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/TaxTypes/CreateTaxTypeTests.cs"
]

for filepath in files_to_update:
    if not os.path.exists(filepath):
        continue

    with open(filepath, "r") as f:
        content = f.read()

    # Move `mockArticleFamilies.Setup(m => m.Add` downwards or `mockTaxTypes` downwards
    content = content.replace("mockArticleFamilies.Setup(m => m.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>((s) => _articleFamilies.Add(s));", "")
    content = content.replace("_contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);", "_contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);\n        mockArticleFamilies.Setup(m => m.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>((s) => _articleFamilies.Add(s));")

    with open(filepath, "w") as f:
        f.write(content)
