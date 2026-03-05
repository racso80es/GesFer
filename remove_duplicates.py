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

    # The previous regex run multiple times. I'll just restore and run once.
    pass
