import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/UpdateUserCommandHandlerTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('''        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,

            CompanyId = companyId,
            CompanyId = companyId,
            Username = "olduser",''', '''        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "olduser",''')

with open(filepath, 'w') as f:
    f.write(content)
