import os
import re

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"

with open(filepath, "r") as f:
    content = f.read()

# SetupService uses ApplicationDbContext to run seeders and also explicitly calls Database.MigrateAsync().
# Wait, if we use UseInMemoryDatabase, `MigrateAsync()` fails on InMemory!
# Yes! EF Core InMemory does NOT support Relational operations like Migrate().
# To fix this, we need to mock ApplicationDbContext completely or override the method in TestableSetupService to not call MigrateAsync, BUT SetupService is what we are testing!
# Actually, TestableSetupService can't override MigrateAsync because it's an EF Core extension method.
# Wait! Let's see SetupService.cs for what fails:
# "Expected result.Success to be True, but found False."
# Let's add console output in the test to see the errors!

content = content.replace("result.Success.Should().BeTrue();", 'if (!result.Success) { Console.WriteLine("Errors: " + string.Join(", ", result.Errors)); }\n        result.Success.Should().BeTrue();')

with open(filepath, "w") as f:
    f.write(content)
