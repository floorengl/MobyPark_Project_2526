using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos;

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
                Capacity = 10,
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
            // Reset DB and create test Licenseplate
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

            //Create test admin
            using (var scope = _appfixture.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var admin = new User
                {
                    Username = "admin",
                    Password = "adminpassword",
                    Role = "ADMIN"
                };
                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }

            //Login as admin
            var loginPayload = new Dictionary<string, string>
            {
                { "username", "admin" },
                { "password", "adminpassword" }
            };
            var loginResponse = await client.PostAsync("login", JsonContent.Create(loginPayload));
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            //Delete Licenseplate
            var deleteResponse = await client.DeleteAsync($"licenseplate/{payload.LicensePlateName}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            //Verify Licenseplate is deleted
            using (var scope = _appfixture.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var stored = await db.LicensePlates.SingleOrDefaultAsync(p => p.LicensePlateName == payload.LicensePlateName);
                Assert.NotNull(stored);
            }
        }

        [Fact]
        public async Task CanGetAllLicenseplate()
        {
            // Reset DB and create test Licenseplate
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
        }

        [Fact]
        public async Task CanGetOneLicenseplate()
        {
            
        }
    }
}