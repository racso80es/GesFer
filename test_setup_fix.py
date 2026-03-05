import os
import re

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"

with open(filepath, "r") as f:
    content = f.read()

# SetupService fails because of "No service for type 'GesFer.Infrastructure.Data.ApplicationDbContext' has been registered."
# Where does it get ApplicationDbContext?
# In SetupService.cs:
# `using var scope = _serviceProvider.CreateScope();`
# `var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();`
# But in our test we setup:
# _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
# _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
# _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

# However, GetRequiredService uses GetService! So we must setup GetService for ApplicationDbContext!
# We just need to add:
# `_serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);`

if "_serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);" not in content:
    content = content.replace("_serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<MasterDataSeeder>))).Returns(_masterDataSeederLoggerMock.Object);", "_serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<MasterDataSeeder>))).Returns(_masterDataSeederLoggerMock.Object);\n        _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);")

with open(filepath, "w") as f:
    f.write(content)
