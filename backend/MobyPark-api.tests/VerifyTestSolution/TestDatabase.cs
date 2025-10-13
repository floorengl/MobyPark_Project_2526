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
        public async Task CanAddToEmptyDatabase1()
        {
            //arrange
            await _fixture.ResetDB();
            using var context = _fixture.CreateContext();

            var entity = new User { FullName="Johann Backer <TEST>", Email="johann@theback.de", Password="Secretive!"};
            //act
            var alreadyPresent = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");
            context.Users.Add(entity);
            await context.SaveChangesAsync();
            var loaded = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");

            //assert
            Assert.Null(alreadyPresent);
            Assert.NotNull(loaded);
            Assert.Equal("Johann Backer <TEST>", loaded.FullName);
        }

        [Fact]
        public async Task CanAddToEmptyDatabase2()
        {
            //arrange
            await _fixture.ResetDB();
            using var context = _fixture.CreateContext();

            var entity = new User { FullName = "Johann Backer <TEST>", Email = "johann@theback.de", Password = "Secretive!" };
            //act
            var alreadyPresent = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");
            context.Users.Add(entity);
            await context.SaveChangesAsync();
            var loaded = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Johann Backer <TEST>");

            //assert
            Assert.Null(alreadyPresent);
            Assert.NotNull(loaded);
            Assert.Equal("Johann Backer <TEST>", loaded.FullName);
        }
    }
}
