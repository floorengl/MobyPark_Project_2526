using Microsoft.EntityFrameworkCore;

namespace MobyPark_api.tests.VerifyTestSolution
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture> { }

    [Collection("Database collection")]
    public class TestDatabase
    {
        private readonly TestDatabaseFixture _fixture;

        public TestDatabase(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanInsertAndRetrieveEntity()
        {
            using var context = _fixture.CreateContext();

            var entity = new User { FullName="Johann Backer <TEST>", Email="johann@theback.de", Password="Secretive!"};
            context.Users.Add(entity);
            await context.SaveChangesAsync();

            var loaded = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");
            Assert.NotNull(loaded);
            Assert.Equal("Johann Backer <TEST>", loaded.FullName);
        }
    }
}
