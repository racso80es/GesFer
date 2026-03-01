import sys
import re

files = [
    'src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetAllArticleFamiliesTests.cs',
    'src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs'
]

for file_path in files:
    with open(file_path, 'r') as f:
        content = f.read()

    content = content.replace('using GesFer.Application.Queries.ArticleFamilies;', 'using GesFer.Application.Commands.ArticleFamilies;')
    content = content.replace('GetAllArticleFamiliesQueryHandler', 'GetAllArticleFamiliesCommandHandler')
    content = content.replace('GetAllArticleFamiliesQuery', 'GetAllArticleFamiliesCommand')

    content = content.replace('GetArticleFamilyByIdQueryHandler', 'GetArticleFamilyByIdCommandHandler')
    content = content.replace('GetArticleFamilyByIdQuery', 'GetArticleFamilyByIdCommand')

    with open(file_path, 'w') as f:
        f.write(content)
