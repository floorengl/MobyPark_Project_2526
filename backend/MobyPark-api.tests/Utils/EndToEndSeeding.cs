using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MobyPark_api.Data.Models;

namespace MobyPark_api.tests.Utils
{
    internal class EndToEndSeeding
    {
        internal static async Task<HttpClient> LoginWithUser1(WholeAppFixture appFixture)
        {
            var account = new Dictionary<string, string>
        {
            { "username", "user1" },
            { "password", "password123-" }
        };

            var client = appFixture.CreateClient();
            var createContent = JsonContent.Create(account);


            var loginResponse = await client.PostAsync("login", createContent);

            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            return client;
        }

        internal static async Task<HttpClient> LoginWithAdmin(WholeAppFixture appFixture)
        {
            var account = new Dictionary<string, string>
        {
            { "username", "admin1" },
            { "password", "password123-" }
        };

            var client = appFixture.CreateClient();
            var createContent = JsonContent.Create(account);


            var loginResponse = await client.PostAsync("login", createContent);

            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            return client;
        }

        /// <summary>
        /// Creates a parkinglot:
        /// Name: mobysMegaPark
        /// Capacity: 1
        /// Tariff: 10
        /// DayTariff: 100
        /// </summary>
        /// <param name="appFixture"></param>
        /// <returns></returns>
        internal static async Task<long> SeedDatabase(WholeAppFixture appFixture)
        {
            var db = appFixture.GetDatabaseFixture().CreateContext();

            IParkingLotRepository lots = new ParkingLotRepository(db);
            ParkingLot lot = new()
            {
                Name = "MobysMegaPark",
                Location = "Center",
                Address = "Moby Dick Street 101",
                Capacity = 1,
                Tariff = 10,
                DayTariff = 100,
                Coordinates = "Just Over The Horizon"
            };
            await db.AddAsync(lot);
            await db.SaveChangesAsync();
            var lotId = lot.Id;

            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "01234567890123456789012345678901" }, // 32 chars = 256 bits
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:Minutes", "60" }
            }!)
            .Build();

            IAuthService userService = new AuthService(new UserRepository(db), new PasswordHasher<User>(), configuration);
            IUserRepository userRepository = new UserRepository(db);
            RegisterRequestDto user1 = new()
            {
                Username = "user1",
                Password = "password123-",
            };
            await userService.RegisterAsync(user1, new CancellationToken());

            RegisterRequestDto user2 = new()
            {
                Username = "user2",
                Password = "password123-",
            };
            await userService.RegisterAsync(user2, new CancellationToken());

            User admin1 = new()
            {
                Username = "admin1",
                Password = "AQAAAAIAAYagAAAAEB7nHmznKhQAU+8TooW4FwVYqKpEkM1s0bpQiXVZuro3HcvA5/1wrjd3utR0Aep1cw==", // unhashed == password123-
                Role = "ADMIN",
            };
            await userRepository.AddAsync(admin1, new CancellationToken());
            await userRepository.SaveChangesAsync();



            return lotId;
        }
    }
}
