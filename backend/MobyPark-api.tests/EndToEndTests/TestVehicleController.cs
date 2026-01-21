using System.Net;
using System.Net.Http.Json;
using MobyPark_api.Dtos.Vehicle;
using MobyPark_api.tests.Utils;
using Xunit;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestVehicleController
    {
        private readonly WholeAppFixture _fixture;

        public TestVehicleController(WholeAppFixture fixture) => _fixture = fixture;

        private async Task<System.Net.Http.HttpClient> LoginVehicleUserAsync()
                => await EndToEndSeeding.LoginWithVehicleUser(_fixture);

        [Fact]
        public async Task CanCreateAndRetrieveVehicle()
        {
            await _fixture.ResetDB();
            var client = await LoginVehicleUserAsync();

            var vehicleDto = new VehicleDto
            {
                LicensePlate = "AA-11-BB",
                Make = "Tesla",
                Model = "Model S",
                Color = "Red",
                Year = new DateTimeOffset(new DateTime(2020, 1, 1), TimeSpan.Zero)
            };

            // Create vehicle
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicleDto);

            string createBody = await createResponse.Content.ReadAsStringAsync();
            if (!createResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Create failed. Status: {createResponse.StatusCode}, Body: {createBody}");
            }

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(created);
            Assert.Equal(vehicleDto.LicensePlate, created!.LicensePlate);

            // Retrieve vehicle by ID
            var getResponse = await client.GetAsync($"vehicles/{created.Id}");
            string getBody = await getResponse.Content.ReadAsStringAsync();
            if (!getResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Get by ID failed. Status: {getResponse.StatusCode}, Body: {getBody}");
            }

            var retrieved = await getResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(retrieved);
            Assert.Equal(vehicleDto.LicensePlate, retrieved!.LicensePlate);
        }

        [Fact]
        public async Task CannotCreateVehicleWithDuplicateLicensePlate()
        {
            await _fixture.ResetDB();
            var client = await LoginVehicleUserAsync();

            var vehicleDto = new VehicleDto
            {
                LicensePlate = "BB-22-CC",
                Make = "Ford",
                Model = "Mustang",
                Color = "Blue",
                Year = new DateTimeOffset(new DateTime(2019, 1, 1), TimeSpan.Zero)
            };

            // Create vehicle
            var createResponse1 = await client.PostAsJsonAsync("vehicles", vehicleDto);
            string body1 = await createResponse1.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, createResponse1.StatusCode);

            // Attempt to create again with same license plate
            var createResponse2 = await client.PostAsJsonAsync("vehicles", vehicleDto);
            string body2 = await createResponse2.Content.ReadAsStringAsync();

            Assert.True(createResponse2.StatusCode == HttpStatusCode.UnprocessableEntity ||
                        createResponse2.StatusCode == HttpStatusCode.Conflict,
                        $"Expected 422 or 409 but got {createResponse2.StatusCode}. Body: {body2}");
        }

        [Fact]
        public async Task CanUpdateVehicle()
        {
            await _fixture.ResetDB();
            var client = await LoginVehicleUserAsync();

            // Create vehicle
            var vehicleDto = new VehicleDto
            {
                LicensePlate = "CC-33-DD",
                Make = "Toyota",
                Model = "Corolla",
                Color = "Silver",
                Year = new DateTimeOffset(new DateTime(2018, 1, 1), TimeSpan.Zero)
            };

            var createResponse = await client.PostAsJsonAsync("vehicles", vehicleDto);
            string bodyCreate = await createResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(created);

            // Update
            var updatedDto = new VehicleDto
            {
                LicensePlate = "CC-33-DD",
                Make = "Toyota",
                Model = "Camry",
                Color = "Black",
                Year = new DateTimeOffset(new DateTime(2021, 1, 1), TimeSpan.Zero)
            };

            var updateResponse = await client.PutAsJsonAsync($"vehicles/{created!.Id}", updatedDto);
            string updateBody = await updateResponse.Content.ReadAsStringAsync();
            if (!updateResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Update failed. Status: {updateResponse.StatusCode}, Body: {updateBody}");
            }

            var updated = await updateResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(updated);
            Assert.Equal(updatedDto.Model, updated!.Model);
            Assert.Equal(updatedDto.Color, updated.Color);
        }

        [Fact]
        public async Task CanDeleteVehicle()
        {
            await _fixture.ResetDB();
            var client = await LoginVehicleUserAsync();

            // Create vehicle
            var vehicleDto = new VehicleDto
            {
                LicensePlate = "DD-44-EE",
                Make = "Honda",
                Model = "Civic",
                Color = "White",
                Year = new DateTimeOffset(new DateTime(2017, 1, 1), TimeSpan.Zero)
            };

            var createResponse = await client.PostAsJsonAsync("vehicles", vehicleDto);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(created);

            // Delete vehicle
            var deleteResponse = await client.DeleteAsync($"vehicles/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await client.GetAsync($"vehicles/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserVehiclesReturnsOnlyUserVehicles()
        {
            await _fixture.ResetDB();
            var client = await LoginVehicleUserAsync();

            // Create 2 vehicles
            var vehicle1 = new VehicleDto
            {
                LicensePlate = "EE-55-FF",
                Make = "BMW",
                Model = "X5",
                Color = "Blue",
                Year = new DateTimeOffset(new DateTime(2019, 1, 1), TimeSpan.Zero)
            };

            var vehicle2 = new VehicleDto
            {
                LicensePlate = "FF-66-GG",
                Make = "Audi",
                Model = "Q7",
                Color = "Black",
                Year = new DateTimeOffset(new DateTime(2020, 1, 1), TimeSpan.Zero)
            };

            await client.PostAsJsonAsync("vehicles", vehicle1);
            await client.PostAsJsonAsync("vehicles", vehicle2);

            var getResponse = await client.GetAsync("vehicles");
            string getBody = await getResponse.Content.ReadAsStringAsync();
            if (!getResponse.IsSuccessStatusCode)
            {
                throw new Exception($"GetUserVehicles failed. Status: {getResponse.StatusCode}, Body: {getBody}");
            }

            var vehicles = await getResponse.Content.ReadFromJsonAsync<VehicleDto[]>();
            Assert.NotNull(vehicles);
            Assert.Equal(2, vehicles!.Length);
        }
    }
}
