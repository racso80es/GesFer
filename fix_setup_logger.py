import os

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"

with open(filepath, "r") as f:
    content = f.read()

content = content.replace("_serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);", """_serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<SetupService>))).Returns(_loggerMock.Object);""")

with open(filepath, "w") as f:
    f.write(content)
