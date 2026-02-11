using Xunit;

namespace GesFer.Admin.IntegrationTests;

[CollectionDefinition("AdminIntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<AdminWebAppFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
