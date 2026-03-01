import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/UpdateUserCommandHandlerTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('result.CompanyName.Should().Be("Test Company");', 'result.CompanyName.Should().BeEmpty();')
# We need to make sure existingUser actually has a CompanyId, because HandleAsync uses user.CompanyId to fetch from API.
content = content.replace('Username = "olduser",', 'CompanyId = companyId,\n            Username = "olduser",')
content = content.replace('Username = "olduser"', 'CompanyId = companyId,\n            Username = "olduser"')
# The first test HandleAsync_WithValidData_ShouldUpdateUser is mocked to return "Test Company" for companyId, so it should be "Test Company".
content = content.replace('result.CompanyName.Should().BeEmpty();', 'result.CompanyName.Should().Be("Test Company");')

# The third test HandleAsync_WhenCompanyNotFound_ShouldThrowException shouldn't throw, it should return an empty CompanyName string.
content = content.replace('''[Fact]
    public async Task HandleAsync_WhenCompanyNotFound_ShouldThrowException()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,

            CompanyId = companyId,
            Username = "olduser"
        };

        var users = new List<GesFer.Product.Back.Domain.Entities.User> { existingUser };
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(existingUser);

        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new UpdateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new UpdateUserCommand(userId, new UpdateUserDto
        {

            Username = "newuser"
        });

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }''', '''[Fact]
    public async Task HandleAsync_WhenCompanyNotFound_ShouldSetEmptyCompanyName()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "olduser"
        };

        var users = new List<GesFer.Product.Back.Domain.Entities.User> { existingUser };
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(existingUser);

        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new UpdateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new UpdateUserCommand(userId, new UpdateUserDto
        {
            Username = "newuser"
        });

        var result = await handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.CompanyName.Should().BeEmpty();
    }''')

with open(filepath, 'w') as f:
    f.write(content)
