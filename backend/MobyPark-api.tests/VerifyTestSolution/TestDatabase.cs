using Microsoft.EntityFrameworkCore;

namespace MobyPark_api.tests.VerifyTestSolution
{
    [Collection("SharedDatabase")]
    public class TestDatabase
    {
        private readonly DatabaseFixture _fixture;

        public TestDatabase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DoesStateBleedBetweenTestsAboveEntry() // this test exists twice once before (this one) and once after the state has been changed.
        {
            using var context = _fixture.CreateContext();
            User? PossiblyJohann = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");
            Assert.Null(PossiblyJohann);
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

        [Fact]
        public async Task DoesStateBleedBetweenTestsBelowEntry() // this test exists twice once before and once after (this one)  the state has been changed.
        {
            using var context = _fixture.CreateContext();
            User? PossiblyJohann = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");
            Assert.Null(PossiblyJohann);
        }
    }
}
