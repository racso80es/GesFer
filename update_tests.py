import os
import re

tests_dir = 'src/Product/Back/tests/GesFer.Product.UnitTests'
grep_output = [
    'Handlers/User/UpdateUserCommandHandlerTests.cs',
    'Handlers/User/CreateUserCommandHandlerTests.cs',
    'Handlers/User/DeleteUserCommandHandlerTests.cs',
    'ArticleFamilies/DeleteArticleFamilyTests.cs',
    'ArticleFamilies/GetArticleFamilyByIdTests.cs',
    'ArticleFamilies/UpdateArticleFamilyTests.cs',
    'ArticleFamilies/GetAllArticleFamiliesTests.cs',
    'ArticleFamilies/CreateArticleFamilyTests.cs',
    'TaxTypes/CreateTaxTypeTests.cs',
    'Services/SetupServiceTests.cs'
]

def process_file(file_path):
    print(f"Processing {file_path}")
    pass

for file in grep_output:
    process_file(os.path.join(tests_dir, file))
