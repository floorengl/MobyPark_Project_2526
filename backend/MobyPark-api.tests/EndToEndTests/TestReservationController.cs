using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using MobyPark_api.tests.Utils;
using MobyPark_api.Dtos.Reservation;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestReservationController
    {
        private readonly WholeAppFixture _appfixutre;

        public TestReservationController(WholeAppFixture appfixutre) => _appfixutre = appfixutre;

        // unfinished test
        [Fact]
        public async Task Test_CannotUseUnpaidReservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

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
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(10),
                LicensePlate = "12-gh-542",
                ParkingLotId = lotId
            });

            var response1 = await client.PostAsync("reservations", reservationrequest1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(4),
                EndTime = DateTime.UtcNow.AddHours(20),
                LicensePlate = "12-gh-542",
                ParkingLotId = lotId
            });

            var response1 = await client.PostAsync("reservations", reservationrequest1);
            var body = await response1.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation_OnStart()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation_OnEnd()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);
            var now = DateTime.UtcNow;
            var reservationrequest1 = JsonContent.Create(new WriteReservationDto()
            {
                StartTime = now.AddHours(8),
                EndTime = now.AddHours(10),
                LicensePlate = "12-gh-542",
                ParkingLotId = lotId
            });

            var response1 = await client.PostAsync("reservations", reservationrequest1);
            var body = await response1.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{now.AddHours(10).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation_BeforeEnd()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(9).AddMinutes(59).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_Active_Reservation_AfterEnd()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

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

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(10).AddMinutes(1).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        }

        [Fact]
        public async Task Test_HasActiveReservation_NoReservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

            var response2 = await client.GetAsync($"reservations/For/12-gh-542/{DateTime.UtcNow.AddHours(10).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")}");
            Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        }
    }
}