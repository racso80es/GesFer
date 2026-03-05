import os
import re

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"

with open(filepath, "r") as f:
    content = f.read()

# Make sure we add Moq and MockQueryable
if "using Moq;" not in content:
    content = "using Moq;\n" + content

# Replace DbContext
content = re.sub(
    r"var config = new ConfigurationBuilder\(\)\s*\n\s*\.AddInMemoryCollection\(new Dictionary<string, string\?>\s*\{\s*\{ \"ConnectionStrings:DefaultConnection\", \"Server=localhost;Database=test;Uid=root;Pwd=test;\" \}\s*\}\)\s*\n\s*\.Build\(\);\s*\n\s*var options = new DbContextOptionsBuilder<ApplicationDbContext>\(\)\s*\n\s*\.UseInMemoryDatabase\(databaseName: Guid\.NewGuid\(\)\.ToString\(\)\)\s*\n\s*\.Options;\s*\n\s*_dbContext = new ApplicationDbContext\(options\);",
    """var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=root;Pwd=test;" }
            })
            .Build();

        var contextMock = new Mock<ApplicationDbContext>();
        _dbContext = contextMock.Object;""",
    content
)

with open(filepath, "w") as f:
    f.write(content)

print("Fixed SetupServiceTests.cs")
