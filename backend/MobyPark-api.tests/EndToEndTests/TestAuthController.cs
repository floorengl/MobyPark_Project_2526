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
            Dictionary<string, string> dict = new()
            {
                {"username", "ben" },
                {"password", "bennietgekgenwoonbenben" }
            };
            JsonContent requestBody = JsonContent.Create(dict);
            var SUT = _appfixutre.CreateClient();
            HttpResponseMessage response = await SUT.PostAsync("register", requestBody);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }
    }
}
