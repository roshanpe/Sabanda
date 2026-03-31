using Sabanda.IntegrationTests.Fixtures;
using Xunit;

namespace Sabanda.IntegrationTests.Auth;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
