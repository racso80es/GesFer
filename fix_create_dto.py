filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('var command = new CreateArticleFamilyCommand(new ArticleFamilyDto', 'var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto')
content = content.replace('var command = new CreateArticleFamilyCommand(companyId,\n            new ArticleFamilyDto', 'var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto')

# Ensure CompanyId is set in the DTO if it wasn't
content = content.replace('Code = "FAM1",\n                Name = "Family 1",', 'CompanyId = companyId,\n                Code = "FAM1",\n                Name = "Family 1",')
content = content.replace('Code = "FAM1",\n                Name = "Another Family",', 'CompanyId = companyId,\n                Code = "FAM1",\n                Name = "Another Family",')

with open(filepath, 'w') as f:
    f.write(content)
