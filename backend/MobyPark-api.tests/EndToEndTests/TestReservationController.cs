using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Reservation;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestReservationController
    {
        private readonly WholeAppFixture _appfixutre;

        public TestReservationController(WholeAppFixture appfixutre) => _appfixutre = appfixutre;

        public async Task<HttpClient> LoginWithUser1()
        {
            var account = new Dictionary<string, string>
            {
                { "username", "user1" },
                { "password", "password123-" }
            };

            var client = _appfixutre.CreateClient();
            var createContent = JsonContent.Create(account);


            var loginResponse = await client.PostAsync("login", createContent);

            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            return client;
        }

        public async Task<HttpClient> LoginWithAdmin()
        {
            var account = new Dictionary<string, string>
            {
                { "username", "admin1" },
                { "password", "password123-" }
            };

            var client = _appfixutre.CreateClient();
            var createContent = JsonContent.Create(account);


            var loginResponse = await client.PostAsync("login", createContent);

            var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string jwt = loginJson.GetProperty("token").GetProperty("accessToken").GetString()!;


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            return client;
        }

        private async Task<long> SeedDatabase()
        {
            var db = _appfixutre.GetDatabaseFixture().CreateContext();

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

        // unfinished test
        [Fact]
        public async Task Test_CannotUseUnpaidReservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithUser1();

            var reservationrequest = JsonContent.Create(new WriteReservationDto() {
                StartTime = DateTime.UtcNow.AddHours(8), 
                EndTime = DateTime.UtcNow.AddHours(10), 
                LicensePlate = "tt-uu-123", 
                ParkingLotId = lotId 
            });

            var response = await client.PostAsync("reservations", reservationrequest);
            var responseDto = await response.Content.ReadFromJsonAsync<ReadReservationDto>();
        }


        [Fact]
        public async Task Test_Add_Get_Delete_Reservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithUser1();

            var reservationrequest = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "vv-76-lkt",
                ParkingLotId = lotId
            });

            // add a new reservation
            var response = await client.PostAsync("reservations", reservationrequest);
            var responseDto = await response.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.NotNull(responseDto);
            Assert.Equal("vv-76-lkt", responseDto.LicensePlate);

            // get the reservation from the database
            var ReservationId = responseDto.Id;
            var getresponse = await client.GetAsync($"reservations/{ReservationId}");
            var getResponseDto = await getresponse.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.NotNull(getResponseDto);
            Assert.Equal("vv-76-lkt", getResponseDto.LicensePlate);

            // delete reservation
            var deleteResponse = await client.DeleteAsync($"reservations/{ReservationId}");
            var deleteResponseDto = await deleteResponse.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.NotNull(deleteResponseDto);
            Assert.Equal("vv-76-lkt", deleteResponseDto.LicensePlate);

            // reservation no longer exists
            var getresponse2 = await client.GetAsync($"reservations/{ReservationId}");
            Assert.Equal(HttpStatusCode.NotFound, getresponse2.StatusCode);
        }

        [Fact]
        public async Task Test_Add_InvalidDTOIsRejected()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithUser1();

            var reservationrequest = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "",
                ParkingLotId = lotId
            });

            // add a new reservation
            var response = await client.PostAsync("reservations", reservationrequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Test_GetAll()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithAdmin();

            // we should be logged in with the admin account now
            Assert.NotNull(client);

            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "",
                ParkingLotId = lotId
            });

            // try adding a bad reservation, no license plate
            var response1 = await client.PostAsync("reservations", reservationrequest1);
            Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);


            var reservationrequest2 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "tv-245-rw",
                ParkingLotId = lotId
            });

            var response2 = await client.PostAsync("reservations", reservationrequest2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            // reservation edge overlaps with previous reservation. This is allowed
            var reservationrequest3 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(11),
                LicensePlate = "kz-34-pl",
                ParkingLotId = lotId
            });

            var response3 = await client.PostAsync("reservations", reservationrequest3);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            var response4 = await client.GetAsync("reservations");
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
            var body = await response4.Content.ReadFromJsonAsync<ReadReservationDto[]>();
            Assert.NotNull(body);
            Assert.Equal("tv-245-rw", body[0].LicensePlate);
        }

        [Fact]
        public async Task Test_HasActiveReservation_No_Active_Reservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithUser1();

            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "12-gh-542",
                ParkingLotId = lotId
            });

            var response1 = await client.PostAsync("reservations", reservationrequest1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss")}");
            Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await SeedDatabase();
            var client = await LoginWithUser1();

            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "12-gh-542",
                ParkingLotId = lotId
            });

            var response1 = await client.PostAsync("reservations", reservationrequest1);
            var body = await response1.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.Now.AddHours(9).ToString("yyyy-MM-dd hh:mm:ss")}");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }
    }
}