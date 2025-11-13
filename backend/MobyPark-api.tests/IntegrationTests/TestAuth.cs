using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace MobyPark_api.tests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class TestAuth
    {
        private readonly DatabaseFixture _fixture;
        public TestAuth(DatabaseFixture fixture) => _fixture = fixture;


        /// <summary>
        /// Get a new auth service to test on
        /// </summary>
        /// <returns></returns>
        public AuthService GetAuthService()
        {
            // I (Rein) who wrote this line vvv am not sure how AuthService should be instaniated. The configbuilder I am least certain of.
            return new AuthService(_fixture.CreateContext(), new PasswordHasher<User>(), new ConfigurationBuilder().Build());
        }

        [Fact]
        public async Task RegisterUserCreatesUser()
        {
            // arrange
            var SUT = GetAuthService();
            var DBconnection = _fixture.CreateContext();
            RegisterRequestDto toMake = new RegisterRequestDto() { Username = "Johnathan Doe", Password = "mysecretcode12345/"};

            RegisterRequestDto toTestPipeline = new RegisterRequestDto() { Username = "should", Password = "fail"};
            // act
            long id = await SUT.RegisterAsync(toMake, new CancellationToken());
            User? actual = DBconnection.Users.FirstOrDefault(u => u.Id != id);
            // assert
            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(toTestPipeline.Username, actual.Username);
        }

    }
}

