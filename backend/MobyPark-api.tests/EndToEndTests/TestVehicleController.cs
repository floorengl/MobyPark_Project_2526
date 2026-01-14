using MobyPark_api.Dtos.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestVehicleController
    {
        private readonly WholeAppFixture _fixture;

        public TestVehicleController(WholeAppFixture fixture) => _fixture = fixture;

        // Helper method, returning a JWT token
        private async Task<string> CreateAndLoginTestUser(string username, string password)
        {
            await _fixture.ResetDB();

            var account = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            var client = _fixture.CreateClient();
            var content = JsonContent.Create(account);

            // Register
            var registerResponse = await client.PostAsync("register", content);
            registerResponse.EnsureSuccessStatusCode();

            // Login
            var loginResponse = await client.PostAsync("login", content);
            loginResponse.EnsureSuccessStatusCode();

            var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetProperty("accessToken").GetString()!;
        }

        [Fact]
        public async Task CanCreateAndRetrieveVehicle()
        {
            var jwt = await CreateAndLoginTestUser("Bob", "password123");

            var client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vehicleDto = new VehicleDto
            {
                LicensePlate = "ABC-123",
                Make = "Toyota",
                Model = "Corolla",
                Color = "Blue",
                Year = new DateTime(2020, 1, 1) // Fix: Use DateTime to match the expected type  
            };

            // Create vehicle
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicleDto);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);

            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);
            Assert.Equal(vehicleDto.LicensePlate, createdVehicle!.LicensePlate);

            // Retrieve vehicle by ID
            var getResponse = await client.GetAsync($"vehicles/{createdVehicle.Id}");
            Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);

            var retrievedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.Equal(vehicleDto.LicensePlate, retrievedVehicle!.LicensePlate);
        }

        [Fact]
        public async Task CannotCreateVehicleWithDuplicateLicensePlate()
        {
            var jwt = await CreateAndLoginTestUser("dupUser", "password123");
            var client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vehicle = new VehicleDto
            {
                LicensePlate = "DUP-123",
                Make = "Honda",
                Model = "Civic",
                Color = "Red",
                Year = new DateTime(2019, 1, 1) // Fix: Use DateTime to match the expected type  
            };

            // First creation
            var firstResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            Assert.Equal(System.Net.HttpStatusCode.Created, firstResponse.StatusCode);

            // Attempt duplicate
            var secondResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            Assert.Equal(422, (int)secondResponse.StatusCode);
        }

        [Fact]
        public async Task CanUpdateVehicle()
        {
            var jwt = await CreateAndLoginTestUser("updateUser", "password123");
            var client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vehicle = new VehicleDto
            {
                LicensePlate = "UPD-456",
                Make = "Ford",
                Model = "Focus",
                Color = "White",
                Year = new DateTime(2018, 1, 1) // Fix: Use DateTime to match the expected type  
            };

            // Create
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();

            Assert.NotNull(createdVehicle); // Fix: Ensure createdVehicle is not null before accessing its properties

            // Update
            var updatedVehicle = new VehicleDto
            {
                Id = createdVehicle.Id,
                LicensePlate = createdVehicle.LicensePlate,
                Make = createdVehicle.Make,
                Model = createdVehicle.Model,
                Color = "Black", // Updated value
                Year = new DateTime(2021, 1, 1), // Updated value
                UserId = createdVehicle.UserId,
                CreatedAt = createdVehicle.CreatedAt
            };

            var updateResponse = await client.PutAsJsonAsync($"vehicles/{createdVehicle.Id}", updatedVehicle);
            Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedVehicleResponse = await updateResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(updatedVehicleResponse);
            Assert.Equal("Black", updatedVehicleResponse!.Color);
            Assert.Equal(2021, updatedVehicleResponse.Year!.Value.Year);
        }

        [Fact]
        public async Task CanDeleteVehicle()
        {
            var jwt = await CreateAndLoginTestUser("deleteUser", "password123");
            var client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var vehicle = new VehicleDto
            {
                LicensePlate = "DEL-789",
                Make = "BMW",
                Model = "X5",
                Color = "Grey",
                Year = new DateTime(2022, 1, 1)
            };

            // Create
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>()!;

            // Delete
            var deleteResponse = await client.DeleteAsync($"vehicles/{createdVehicle.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify deletion
            var getResponse = await client.GetAsync($"vehicles/{createdVehicle.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserVehiclesReturnsOnlyUserVehicles()
        {
            var jwt1 = await CreateAndLoginTestUser("user1", "pass1");
            var jwt2 = await CreateAndLoginTestUser("user2", "pass2");

            var client = _fixture.CreateClient();

            // User 1 creates a vehicle
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt1);
            await client.PostAsJsonAsync("vehicles", new VehicleDto { LicensePlate = "U1-1", Make = "Tesla", Model = "Model 3", Color = "Red", Year = new DateTime(2020, 1, 1) });

            // User 2 creates a vehicle
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt2);
            await client.PostAsJsonAsync("vehicles", new VehicleDto { LicensePlate = "U2-1", Make = "Tesla", Model = "Model S", Color = "Blue", Year = new DateTime(2021, 1, 1) });

            // Verify user1 sees only their vehicle
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt1);
            var response = await client.GetAsync("vehicles");
            var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
            Assert.Single(vehicles);
            Assert.Equal("U1-1", vehicles![0].LicensePlate);
        }

    }
}
