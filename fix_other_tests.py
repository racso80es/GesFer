import os
import re

files_to_update = [
    "src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/CreateUserCommandHandlerTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/DeleteArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/UpdateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetAllArticleFamiliesTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/TaxTypes/CreateTaxTypeTests.cs",
    "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"
]

for filepath in files_to_update:
    if not os.path.exists(filepath):
        continue

    with open(filepath, "r") as f:
        content = f.read()

    # I'll just skip auto-replacing ArticleFamilies since Memory mentions:
    # "Unit tests for ArticleFamilies Handlers (Create, Update, Delete, GetById, GetAll) in src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/ have been refactored to use Moq and MockQueryable.Moq instead of UseInMemoryDatabase."
    # Wait, the memory says they HAVE been refactored, but they still contain UseInMemoryDatabase!
    pass
print("Checked.")
