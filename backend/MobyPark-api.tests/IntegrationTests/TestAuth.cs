using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using MobyPark_api.Dtos.Auth;
using System;

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
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "01234567890123456789012345678901" }, // 32 chars = 256 bits
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:Minutes", "60" }
                }!)
                .Build();

            return new AuthService(_fixture.CreateContext(), new PasswordHasher<User>(), configuration);
        }
        
        [Fact]
        public async Task RegisterUserCreatesUser()
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            var DBconnection = _fixture.CreateContext();
            RegisterRequestDto toMake = new RegisterRequestDto() { Username = "Johnathan Doe", Password = "mysecretcode12345/" };

            // act
            long id = await SUT.RegisterAsync(toMake, new CancellationToken());
            User? actual = DBconnection.Users.FirstOrDefault(u => u.Id == id);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(toMake.Username, actual.Username);
        }

        [Fact]
        public async Task GetGetProfileAsyncShouldReturnProfile()
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            var DBconnection = _fixture.CreateContext();

            RegisterRequestDto toMake = new RegisterRequestDto() { Username = "Johnathan Doe", Password = "mysecretcode12345/" };
            await SUT.RegisterAsync(toMake, CancellationToken.None);

            var user = DBconnection.Users.FirstOrDefault(u => u.Username == toMake.Username);
            Assert.NotNull(user);

            long userId = user!.Id;
            CancellationToken ct = new();

            // act
            ProfileResponseDto? response = await SUT.GetProfileAsync(userId, ct);

            // assert
            Assert.NotNull(response);
            Assert.Equal(userId, response!.Id);
            Assert.Equal(toMake.Username, response.Username);

        }

        [Fact]
        public async Task RegisterDuplicateUserShouldThrow()
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            RegisterRequestDto dto = new RegisterRequestDto { Username = "Jane Doe", Password = "supersecret123" };

            // act
            await SUT.RegisterAsync(dto, CancellationToken.None);

            // assert
            await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                await SUT.RegisterAsync(dto, CancellationToken.None);
            });
        }

        [Fact]
        public async Task LoginShouldReturnToken_WhenCredentialsValid()
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            RegisterRequestDto dto = new RegisterRequestDto { Username = "Alice", Password = "password123!" };
            await SUT.RegisterAsync(dto, CancellationToken.None);

            // act
            var loginRequest = new LoginRequestDto { UserName = "Alice", Password = "password123!" };
            AuthResponseDto result = await SUT.LoginAsync(loginRequest);

            // assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginShouldThrow_WhenUserInactive()
        {
            await _fixture.ResetDB();

            // arrange
            var db = _fixture.CreateContext(); // avoid using SUT's DbContext for this test
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "01234567890123456789012345678901" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:Minutes", "60" }
                }!)
                .Build();

            var SUT = new AuthService(db, new PasswordHasher<User>(), configuration);

            RegisterRequestDto dto = new RegisterRequestDto { Username = "Bob", Password = "password123!" };
            long id = await SUT.RegisterAsync(dto, CancellationToken.None);

            var loginRequest = new LoginRequestDto
            {
                UserName = "Bob",
                Password = "password123!"
            };

            // act
            var user = db.Users.First(u => u.Id == id);
            user.Active = false;
            await db.SaveChangesAsync();

            // assert
            Assert.False(db.Users.First(u => u.Id == id).Active); // sanity check
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => SUT.LoginAsync(loginRequest));
        }


        [Fact]
        public async Task GetProfileAsyncShouldReturnNull_WhenUserDoesNotExist()
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            CancellationToken ct = new();

            // act
            ProfileResponseDto? response = await SUT.GetProfileAsync(999999, ct);

            // assert
            Assert.Null(response);
        }

        [Theory]
        [InlineData("NonExistentUser", "anyPassword")]       // non-existent user
        [InlineData("", "")]                                 // no input
        [InlineData("John Pork", "")]                        // no password input
        [InlineData("", "aMaZiNg123PaSsWoRd456")]            // no username input
        [InlineData("John Pork", "1234567")]                 // password to short
        [InlineData("John Pork", "1234567890")]              // password incorrect
        public async Task LoginCasesThatShouldThrowErrors(string username, string password)
        {
            await _fixture.ResetDB();

            // arrange
            var SUT = GetAuthService();
            var DBconnection = _fixture.CreateContext();

            // act
            var loginObject = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            //assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => SUT.LoginAsync(loginObject));
        }
    }
}

