using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos;
using MobyPark_api.tests.Utils;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestLicenseplateController
    {
        private readonly WholeAppFixture _appfixture;

        public TestLicenseplateController(WholeAppFixture appfixture) => _appfixture = appfixture;


        private async Task<long> MakeTestParkingLotAsync()
        {
            using var scope = _appfixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var lot = new ParkingLot
            {
                Name = "Test Lot",
                Location = "Test City",
                Address = "Test Street",
                Capacity = 1000,
                Tariff = 2,
                DayTariff = 12
            };

            db.ParkingLots.Add(lot);
            await db.SaveChangesAsync();
            return lot.Id;
        }

        [Fact]
        public async Task CanCreateLicenseplate()
        {
            //Create test Licenseplate
            await _appfixture.ResetDB();
            var lotId = await MakeTestParkingLotAsync();

            var payload = new CheckInDto
            {
                LicensePlateName = "Tes-t-001",
                ParkingLotId = lotId
            };

            var client = _appfixture.CreateClient();
            var createResponse = await client.PostAsync("licenseplate", JsonContent.Create(payload));
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            //Verify if Licenseplate is stored in DB
            using var scope = _appfixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.LicensePlates.SingleOrDefaultAsync(p => p.LicensePlateName == payload.LicensePlateName);
            Assert.NotNull(stored);
        }

        [Fact]
        public async Task CanDeleteLicenseplate()
        {
            // Reset DB and seed admin user first
            await _appfixture.ResetDB();
            await EndToEndSeeding.SeedDatabase(_appfixture);
            
            var lotId = await MakeTestParkingLotAsync();

            var payload = new CheckInDto
            {
                LicensePlateName = "Tes-t-001",
                ParkingLotId = lotId
            };

            var client = _appfixture.CreateClient();
            var createResponse = await client.PostAsync("licenseplate", JsonContent.Create(payload));
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Login as admin and delete the license plate
            var adminClient = await EndToEndSeeding.LoginWithAdmin(_appfixture);
            var deleteResponse = await adminClient.DeleteAsync($"licenseplate/{payload.LicensePlateName}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify the license plate is deleted from the database
            using var scope = _appfixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.LicensePlates.SingleOrDefaultAsync(p => p.LicensePlateName == payload.LicensePlateName);
            Assert.Null(stored);
        }

        [Fact]
        public async Task CanGetAllLicenseplate()
        {
            // Reset DB and create testparkinglot first
            await _appfixture.ResetDB();
            var lotId = await MakeTestParkingLotAsync();
            
            // Then seed users for authentication
            await EndToEndSeeding.SeedDatabase(_appfixture);

            var payload1 = new CheckInDto
            {
                LicensePlateName = "Tes-t-001",
                ParkingLotId = lotId
            };
            var payload2 = new CheckInDto
            {
                LicensePlateName = "Tes-t-002",
                ParkingLotId = lotId
            };

            // Post licenseplates
            var client = _appfixture.CreateClient();
            var createResponse1 = await client.PostAsync("licenseplate", JsonContent.Create(payload1));
            Assert.Equal(HttpStatusCode.Created, createResponse1.StatusCode);

            var createResponse2 = await client.PostAsync("licenseplate", JsonContent.Create(payload2));
            Assert.Equal(HttpStatusCode.Created, createResponse2.StatusCode);

            // Get all license plates with Admin login
            var adminClient = await EndToEndSeeding.LoginWithAdmin(_appfixture);
            var getResponse = await adminClient.GetAsync("licenseplate");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var plates = await getResponse.Content.ReadFromJsonAsync<List<LicenseplateDto>>();
            Assert.NotNull(plates);
            Assert.Contains(plates, p => p.LicensePlateName == payload1.LicensePlateName);
            Assert.Contains(plates, p => p.LicensePlateName == payload2.LicensePlateName);
        }

        [Fact]
        public async Task CanGetOneLicenseplate()
        {
            // Reset DB and create testparkinglot first
            await _appfixture.ResetDB();
            var lotId = await MakeTestParkingLotAsync();

            // Then seed users for authentication
            await EndToEndSeeding.SeedDatabase(_appfixture);

            var payload = new CheckInDto
            {
                LicensePlateName = "Tes-t-001",
                ParkingLotId = lotId
            };

            // Post licenseplate
            var client = _appfixture.CreateClient();
            var createResponse = await client.PostAsync("licenseplate", JsonContent.Create(payload));
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Get the specific license plate with Admin login
            var adminClient = await EndToEndSeeding.LoginWithAdmin(_appfixture);
            var getResponse = await adminClient.GetAsync($"licenseplate/{payload.LicensePlateName}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var plate = await getResponse.Content.ReadFromJsonAsync<LicenseplateDto>();
            Assert.NotNull(plate);
            Assert.Equal(payload.LicensePlateName, plate.LicensePlateName);
        }
    }
}