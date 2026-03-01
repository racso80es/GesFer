import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# SetupService probably calls Database.EnsureDeletedAsync() or similar, which fails if DbContext is not properly mocked.
content = content.replace('_serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);',
'''var databaseMock = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_dbContext);
        databaseMock.Setup(d => d.EnsureDeletedAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(true);
        databaseMock.Setup(d => d.EnsureCreatedAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(true);
        _contextMock.Setup(c => c.Database).Returns(databaseMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);''')

with open(filepath, 'w') as f:
    f.write(content)
