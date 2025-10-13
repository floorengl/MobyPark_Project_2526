namespace MobyPark_api.tests.Utils
{
    [CollectionDefinition("SharedWholeApp")]
    public class SharedWholeAppCollection : ICollectionFixture<WholeAppFixture>
    {
        // doesn't need anything else here. it's just a way of making the database shared across all test classes
    }
}
