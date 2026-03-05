import os
import re

dir_path = "src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User"
files = ["CreateUserCommandHandlerTests.cs", "DeleteUserCommandHandlerTests.cs", "UpdateUserCommandHandlerTests.cs"]

for filename in files:
    filepath = os.path.join(dir_path, filename)
    with open(filepath, "r") as f:
        content = f.read()

    # Add MockQueryable.Moq using
    if "using MockQueryable.Moq;" not in content:
        content = content.replace("using Moq;", "using Moq;\nusing MockQueryable.Moq;")

    # Replace ApplicationDbContext creation with Mock
    content = re.sub(
        r"var options = new DbContextOptionsBuilder<ApplicationDbContext>\(\)\s*\n\s*\.UseInMemoryDatabase\(databaseName: Guid\.NewGuid\(\)\.ToString\(\)\)\s*\n\s*\.Options;\s*\n\s*using var context = new ApplicationDbContext\(options\);",
        "var users = new List<Product.Back.Domain.Entities.User>();\n        var mockUsersDbSet = users.BuildMockDbSet();\n        var contextMock = new Mock<ApplicationDbContext>();\n        contextMock.Setup(c => c.Users).Returns(mockUsersDbSet.Object);\n        var context = contextMock.Object;",
        content
    )

    # In UpdateUserCommandHandlerTests, handle context.Users.Add and SaveChangesAsync mock
    if filename == "UpdateUserCommandHandlerTests.cs" or filename == "CreateUserCommandHandlerTests.cs" or filename == "DeleteUserCommandHandlerTests.cs":
        content = re.sub(r"context\.Users\.Add\(user\);\s*\n\s*await context\.SaveChangesAsync\(\);", r"users.Add(user);", content)
        content = re.sub(r"context\.Users\.AddRange\(user1, user2\);\s*\n\s*await context\.SaveChangesAsync\(\);", r"users.AddRange(new[] { user1, user2 });", content)

        # Replace FindAsync with a FindAsync mock or just SingleOrDefault
        content = re.sub(r"await context\.Users\.FindAsync\(userId\)", r"users.SingleOrDefault(u => u.Id == userId)", content)
        content = re.sub(r"await context\.Users\.FindAsync\(result\.Id\)", r"users.SingleOrDefault(u => u.Id == result.Id)", content)

    with open(filepath, "w") as f:
        f.write(content)

print("Updated user tests.")
