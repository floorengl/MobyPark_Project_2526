using System.Net.Http.Json;
using System.Text.Json;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestAuthController
    {
        private readonly WholeAppFixture _appfixutre;

        public TestAuthController(WholeAppFixture appfixutre) => _appfixutre = appfixutre;

        [Fact]
        public async Task CanCreateNewAccount()
        {
            await _appfixutre.ResetDB();

            Dictionary<string, string> NewAccount = new()
            {
                {"username", "ben" },
                {"password", "bennietgekgenwoonbenben" }
            };
            JsonContent requestBody = JsonContent.Create(NewAccount);
            var SUT = _appfixutre.CreateClient();
            HttpResponseMessage response = await SUT.PostAsync("register", requestBody);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CanLoginWithCreatedAccount()
        {
            await _appfixutre.ResetDB();

            Dictionary<string, string> Account = new()
            {
                {"username", "jan" },
                {"password", "jannemandeechteman" }
            };

            JsonContent CreatedAccount = JsonContent.Create(Account);
            var SUT = _appfixutre.CreateClient();
            HttpResponseMessage createResponse = await SUT.PostAsync("register", CreatedAccount);

            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);

            // Then login with created account
            HttpResponseMessage loginResponse = await SUT.PostAsync("login", CreatedAccount);

            Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);
        }

        [Fact]
        public async Task CanRetrieveProfileWithValidJWT()
        {
            await _appfixutre.ResetDB();

            // Arrange: create a test account
            var account = new Dictionary<string, string>
            {
                { "username", "piet" },
                { "password", "pietervantoettoet" }
            };

            var client = _appfixutre.CreateClient();
            var createContent = JsonContent.Create(account);

            // Act: Register the account
            var registerResponse = await client.PostAsync("register", createContent);
            Assert.Equal(System.Net.HttpStatusCode.Created, registerResponse.StatusCode);

            // Act: Login with the account
            var loginResponse = await client.PostAsync("login", createContent);
            Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);


            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            // Act: Retrieve profile
            var profileResponse = await client.GetAsync("profile");
            Assert.Equal(System.Net.HttpStatusCode.OK, profileResponse.StatusCode);


            var profile = await profileResponse.Content.ReadFromJsonAsync<ProfileResponseDto>()!;

            // Assert: username matches
            Assert.Equal(account["username"], profile.Username);
        }
    }
}
