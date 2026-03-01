import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('var options = new DbContextOptionsBuilder<ApplicationDbContext>()\n            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())\n            .Options;\n        _dbContext = new ApplicationDbContext(options);', '_contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());\n        _dbContext = _contextMock.Object;')

content = content.replace('private readonly ApplicationDbContext _dbContext;', 'private readonly Mock<ApplicationDbContext> _contextMock;\n    private readonly ApplicationDbContext _dbContext;')

with open(filepath, 'w') as f:
    f.write(content)
