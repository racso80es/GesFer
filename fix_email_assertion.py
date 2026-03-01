import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/UpdateUserCommandHandlerTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# Email is a ValueObject, so we need to access its Value property or compare with Email.Create()
content = content.replace('existingUser.Email.Should().Be("new@example.com");', 'existingUser.Email.Value.Value.Should().Be("new@example.com");')

with open(filepath, 'w') as f:
    f.write(content)
