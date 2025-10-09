

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace MobyPark_api.tests.TestServices
{
    [Collection("SharedDatabase")]
    public class TestAuth
    {
        private readonly DatabaseFixture _fixture;

        public TestAuth(DatabaseFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Get a new auth service to test on
        /// I (Rein) who wrote this line vvv am not sure how AuthService should be instaniated. The configbuilder I am least certain of.
        /// </summary>
        /// <returns></returns>
        public AuthService GetAuthService()
        {
            return new AuthService(_fixture.CreateContext(), new PasswordHasher<User>(), new ConfigurationBuilder().Build());
        }

        [Fact]
        public async Task RegisterUserCreatesUser()
        {
            // arrange
            var SUT = GetAuthService();
            var DBconnection = _fixture.CreateContext();
            RegisterRequestDto toMake = new RegisterRequestDto() { Username = "Johnathan Doe", Password = "mysecretcode12345/"};
            // act
            long id = await SUT.RegisterAsync(toMake, new CancellationToken());
            User? actual = DBconnection.Users.FirstOrDefault(u => u.Id == id);
            // assert
            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(toMake.Username, actual.Username);

        }

    }
}

