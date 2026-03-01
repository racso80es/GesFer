import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# Let's fix the SetupServiceTests to not use Moq for JsonDataSeeder if it fails, and just revert to the original where JsonDataSeeder uses the mocked _dbContext.
content = content.replace('''var jsonSeederMock = new Mock<JsonDataSeeder>(_dbContext, _seederLoggerMock.Object, _sanitizerMock.Object, config);
        jsonSeederMock.Setup(s => s.SeedAsync()).Returns(Task.CompletedTask);
        _serviceProviderMock.Setup(x => x.GetService(typeof(JsonDataSeeder))).Returns(jsonSeederMock.Object);''', '''var jsonSeeder = new JsonDataSeeder(_dbContext, _seederLoggerMock.Object, _sanitizerMock.Object, config);
        _serviceProviderMock.Setup(x => x.GetService(typeof(JsonDataSeeder))).Returns(jsonSeeder);''')

# Let's also properly mock the Database so it doesn't throw about missing provider
content = content.replace('''        var databaseMock = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_dbContext);
        databaseMock.Setup(d => d.EnsureDeletedAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(true);
        databaseMock.Setup(d => d.EnsureCreatedAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(true);
        _contextMock.Setup(c => c.Database).Returns(databaseMock.Object);''', '''        var databaseMock = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_dbContext);
        _contextMock.Setup(c => c.Database).Returns(databaseMock.Object);''')

with open(filepath, 'w') as f:
    f.write(content)
