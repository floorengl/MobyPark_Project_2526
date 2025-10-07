
namespace MobyPark_api.tests
{
    [CollectionDefinition("SharedDatabase")]
    public class SharedDatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // doesn't need anything else here. it's just a way of making the database shared across all test classes
    }
    //[CollectionDefinition("Database collection")]
    //public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
}
