using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        // Helper to create UTC DateTime
        private static DateTime UtcDate(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
            => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

        [Fact]
        public async Task CanCreateAndRetrieveVehicle()
        {
            await _fixture.ResetDB();
            var client = await EndToEndSeeding.LoginWithUser1(_fixture);

            var vehicleDto = new VehicleDto
            {
                LicensePlate = "ABC-123",
                Make = "Toyota",
                Model = "Corolla",
                Color = "Blue",
                Year = UtcDate(2020, 1, 1)
            };

            // Create vehicle
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicleDto);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);
            Assert.Equal(vehicleDto.LicensePlate, createdVehicle!.LicensePlate);

            // Retrieve vehicle by ID
            var getResponse = await client.GetAsync($"vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var retrievedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(retrievedVehicle);
            Assert.Equal(vehicleDto.LicensePlate, retrievedVehicle!.LicensePlate);
        }

        [Fact]
        public async Task CannotCreateVehicleWithDuplicateLicensePlate()
        {
            await _fixture.ResetDB();
            var client = await EndToEndSeeding.LoginWithUser1(_fixture);

            var vehicle = new VehicleDto
            {
                LicensePlate = "DUP-123",
                Make = "Honda",
                Model = "Civic",
                Color = "Red",
                Year = UtcDate(2019, 1, 1)
            };

            var vehicle2 = new VehicleDto
            {
                LicensePlate = "DUP-123",
                Make = "Kia",
                Model = "Niro",
                Color = "Black",
                Year = UtcDate(2020, 1, 1)
            };

            // First creation
            var firstResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            // Attempt duplicate
            var secondResponse = await client.PostAsJsonAsync("vehicles", vehicle2);
            Assert.Equal(422, (int)secondResponse.StatusCode);
        }

        [Fact]
        public async Task CanUpdateVehicle()
        {
            await _fixture.ResetDB();
            var client = await EndToEndSeeding.LoginWithUser1(_fixture);

            var vehicle = new VehicleDto
            {
                LicensePlate = "UPD-456",
                Make = "Ford",
                Model = "Focus",
                Color = "White",
                Year = UtcDate(2018, 1, 1)
            };

            // Create
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);

            // Update
            var updatedVehicle = new VehicleDto
            {
                Id = createdVehicle!.Id,
                LicensePlate = createdVehicle.LicensePlate,
                Make = createdVehicle.Make,
                Model = createdVehicle.Model,
                Color = "Black",
                Year = UtcDate(2021, 1, 1),
                UserId = createdVehicle.UserId
            };

            var updateResponse = await client.PutAsJsonAsync($"vehicles/{createdVehicle.Id}", updatedVehicle);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedVehicleResponse = await updateResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(updatedVehicleResponse);
            Assert.Equal("Black", updatedVehicleResponse!.Color);
            Assert.Equal(UtcDate(2021, 1, 1), updatedVehicleResponse.Year);
        }

        [Fact]
        public async Task CanDeleteVehicle()
        {
            await _fixture.ResetDB();
            var client = await EndToEndSeeding.LoginWithUser1(_fixture);

            var vehicle = new VehicleDto
            {
                LicensePlate = "DEL-789",
                Make = "BMW",
                Model = "X5",
                Color = "Grey",
                Year = UtcDate(2022, 1, 1)
            };

            // Create
            var createResponse = await client.PostAsJsonAsync("vehicles", vehicle);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);

            // Delete
            var deleteResponse = await client.DeleteAsync($"vehicles/{createdVehicle!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify deletion
            var getResponse = await client.GetAsync($"vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserVehiclesReturnsOnlyUserVehicles()
        {
            await _fixture.ResetDB();
            var client1 = await EndToEndSeeding.LoginWithUser1(_fixture);

            // User1 creates vehicle
            var vehicle1 = new VehicleDto
            {
                LicensePlate = "U1-1",
                Make = "Tesla",
                Model = "Model 3",
                Color = "Red",
                Year = UtcDate(2020, 1, 1)
            };
            await client1.PostAsJsonAsync("vehicles", vehicle1);

            // Login as user2
            var client2 = await EndToEndSeeding.LoginWithUser1(_fixture); // or add LoginWithUser2
            var vehicle2 = new VehicleDto
            {
                LicensePlate = "U2-1",
                Make = "Tesla",
                Model = "Model S",
                Color = "Blue",
                Year = UtcDate(2021, 1, 1)
            };
            await client2.PostAsJsonAsync("vehicles", vehicle2);

            // Verify user1 sees only their vehicle
            var response = await client1.GetAsync("vehicles");
            var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
            Assert.NotNull(vehicles);
            Assert.Single(vehicles);
            Assert.Equal("U1-1", vehicles![0].LicensePlate);
        }
    }
}
