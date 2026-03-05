import os

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"
with open(filepath, "r") as f:
    content = f.read()

# SetupServiceTests does `var contextMock = new Mock<ApplicationDbContext>(); _dbContext = contextMock.Object;`
# But it fails `result.Success.Should().BeTrue();`. Why?
# Probably because `_dbContext.Database.MigrateAsync()` throws a NullReferenceException, since `_dbContext.Database` is null on a pure mock!
# Let's revert `SetupServiceTests` context creation to use `UseInMemoryDatabase`.
# This is an infrastructure service test, not a domain logic unit test, so InMemoryDatabase is completely fine here.

original_context = """var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);"""

content = content.replace("""var contextMock = new Mock<ApplicationDbContext>();
        _dbContext = contextMock.Object;""", original_context)

with open(filepath, "w") as f:
    f.write(content)
