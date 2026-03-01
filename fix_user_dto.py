import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/UpdateUserCommandHandlerTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('PasswordSalt = "oldsalt",', '')
content = content.replace('CompanyId = companyId,', '')
content = content.replace('CompanyId = Guid.NewGuid(),', '')

with open(filepath, 'w') as f:
    f.write(content)
