using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GesFer.Api.Services;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace GesFer.Product.UnitTests.Services;

public class SetupServiceTests
{
    private readonly Mock<ILogger<SetupService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<ILogger<JsonDataSeeder>> _seederLoggerMock;
    private readonly Mock<ISensitiveDataSanitizer> _sanitizerMock;
    private readonly Mock<ISequentialGuidGenerator> _guidGeneratorMock;
    private readonly Mock<ILogger<MasterDataSeeder>> _masterDataSeederLoggerMock;
    private readonly ApplicationDbContext _dbContext;

    public SetupServiceTests()
    {
        _loggerMock = new Mock<ILogger<SetupService>>();
        _configurationMock = new Mock<IConfiguration>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _seederLoggerMock = new Mock<ILogger<JsonDataSeeder>>();
        _sanitizerMock = new Mock<ISensitiveDataSanitizer>();
        _guidGeneratorMock = new Mock<ISequentialGuidGenerator>();
        _masterDataSeederLoggerMock = new Mock<ILogger<MasterDataSeeder>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        // Setup Service Provider and Scopes
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);

        // Setup Dependency Resolution
        _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_dbContext);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<SetupService>))).Returns(_loggerMock.Object);

        // Setup JsonDataSeeder (using concrete class as we cannot mock it easily, but it handles missing files gracefully)
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        var jsonSeeder = new JsonDataSeeder(_dbContext, _seederLoggerMock.Object, _sanitizerMock.Object, config);
        _serviceProviderMock.Setup(x => x.GetService(typeof(JsonDataSeeder))).Returns(jsonSeeder);

        _serviceProviderMock.Setup(x => x.GetService(typeof(ISequentialGuidGenerator))).Returns(_guidGeneratorMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<MasterDataSeeder>))).Returns(_masterDataSeederLoggerMock.Object);
    }

    [Fact]
    public async Task InitializeEnvironmentAsync_ShouldRunAllStepsAndSucceed()
    {
        // Arrange
        var service = new TestableSetupService(_loggerMock.Object, _configurationMock.Object, _serviceProviderMock.Object);

        // Act
        var result = await service.InitializeEnvironmentAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Steps.Should().Contain(s => s != null && s.Contains("1. Deteniendo y eliminando contenedores Docker"));
        result.Steps.Should().Contain(s => s != null && s.Contains("4. Creando contenedores Docker"));
        result.Steps.Should().Contain(s => s != null && s.Contains("6. Creando base de datos"));

        // Check logs
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inicialización completada exitosamente")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task InitializeEnvironmentAsync_ShouldReportError_WhenDockerFails()
    {
        // Arrange
        var service = new TestableSetupService(_loggerMock.Object, _configurationMock.Object, _serviceProviderMock.Object);
        service.ForceDockerFailure = true;

        // Act
        var result = await service.InitializeEnvironmentAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Simulated Docker Failure"));
    }

    // Testable subclass to mock Docker interactions
    public class TestableSetupService : SetupService
    {
        public bool ForceDockerFailure { get; set; } = false;

        public TestableSetupService(ILogger<SetupService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
            : base(logger, configuration, serviceProvider)
        {
        }

        protected override Task<(bool Success, string? Error)> ExecuteDockerCommandAsync(string command)
        {
            if (ForceDockerFailure && command.Contains("up -d"))
            {
                return Task.FromResult<(bool, string?)>((false, "Simulated Docker Failure"));
            }
            return Task.FromResult<(bool, string?)>((true, null));
        }

        protected override Task<bool> WaitForMySqlReadyAsync(TimeSpan timeout, string containerName = "gesfer_product_db")
        {
            return Task.FromResult(true);
        }
    }
}
