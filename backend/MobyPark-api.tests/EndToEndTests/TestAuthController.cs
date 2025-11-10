using System.Net.Http.Json;

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
            Dictionary<string, string> Account = new()
            {
                {"username", "piet" },
                {"password", "pietervantoettoet" }
            };

            JsonContent CreatedAccount = JsonContent.Create(Account);
            var SUT = _appfixutre.CreateClient();
            HttpResponseMessage createResponse = await SUT.PostAsync("register", CreatedAccount);

            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);

            // Then login with created account
            HttpResponseMessage loginResponse = await SUT.PostAsync("login", CreatedAccount);

            Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);

            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            string token = loginContent!["token"];

            // Then retrieve profile with JWT
            SUT.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage profileResponse = await SUT.GetAsync("profile");

            Assert.Equal(System.Net.HttpStatusCode.OK, profileResponse.StatusCode);
        }
    }
}
