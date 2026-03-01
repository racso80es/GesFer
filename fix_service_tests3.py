import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# The issue is that JsonDataSeeder uses _dbContext directly (it requires an instance, not a Mock proxy, because of how EF works internally when not fully mocked).
# Actually, the requirement was to replace `UseInMemoryDatabase` with pure mocks where appropriate.
# SetupServiceTests uses EF Core to test the Seeder setup which depends heavily on DatabaseFacade etc.
# Wait, let's just make the Mock<ApplicationDbContext> provide a mock DatabaseFacade that returns true for EnsureDeleted and EnsureCreated, and also maybe skip the Seeder or mock the Seeder.
# "Setup JsonDataSeeder (using concrete class as we cannot mock it easily, but it handles missing files gracefully)"

# If we just mock JsonDataSeeder:
content = content.replace('''var jsonSeeder = new JsonDataSeeder(_dbContext, _seederLoggerMock.Object, _sanitizerMock.Object, config);
        _serviceProviderMock.Setup(x => x.GetService(typeof(JsonDataSeeder))).Returns(jsonSeeder);''',
'''var jsonSeederMock = new Mock<JsonDataSeeder>(_dbContext, _seederLoggerMock.Object, _sanitizerMock.Object, config);
        jsonSeederMock.Setup(s => s.SeedAsync()).Returns(Task.CompletedTask);
        _serviceProviderMock.Setup(x => x.GetService(typeof(JsonDataSeeder))).Returns(jsonSeederMock.Object);''')

# But wait, JsonDataSeeder doesn't have a virtual SeedAsync? Let's check JsonDataSeeder.
# Or we can just mock the DbContext properly. Or maybe we can revert SetupServiceTests to UseInMemoryDatabase if it's too complex and focus on the Domain/Business Logic unit tests?
# "Refactor TaxType and Service tests to pure mocks: Refactor TaxTypes/CreateTaxTypeTests.cs and Services/SetupServiceTests.cs to remove UseInMemoryDatabase in favor of pure mocks."

# The error "No database provider has been configured for this DbContext" happens because _dbContext is a mocked instance but its options aren't configured with a provider.
# We need to supply DbContextOptions.
content = content.replace('var options = new DbContextOptionsBuilder<ApplicationDbContext>()\\n            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())\\n            .Options;\\n        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());',
'var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options; _contextMock = new Mock<ApplicationDbContext>(options);')

with open(filepath, 'w') as f:
    f.write(content)
