using System.Net.Http.Json;
using MobyPark_api.Dtos.Discount;
using MobyPark_api.Dtos.Reservation;
using MobyPark_api.tests.Utils;
using System.Net;
using MobyPark_api.Dtos;

namespace MobyPark_api.tests.EndToEndTests
{
    [Collection("SharedWholeApp")]
    public class TestDiscountController
    {
        private readonly WholeAppFixture _appfixutre;

        public TestDiscountController(WholeAppFixture appfixutre) => _appfixutre = appfixutre;

        [Fact]
        ///This test will go through all crud options in a happy flow.
        public async Task Test_HappyFlowCrudDiscount()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            // create
            var now = DateTime.Now;

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "BOGUS DISCOUNT",
                Amount = 0.75m,
                Operator = Enums.Operator.Multiply,
                ParkingLotIds = [lotId],
                Start = now,
                End = now.AddDays(30),
                DiscountType = Enums.DiscountType.NoExtraCriteria
            });

            var createResponse = await client.PostAsync("discount", discountToAdd);
            var createResponseDto = await createResponse.Content.ReadFromJsonAsync<ReadDiscountDto>();

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // read (get by ID)
            var read1Response = await client.GetAsync($"discount/{createResponseDto.Id}");
            var read1ResponseDto = await read1Response.Content.ReadFromJsonAsync<ReadDiscountDto>();
            
            Assert.Equal(HttpStatusCode.OK, read1Response.StatusCode);

            Assert.Equal(0.75m, read1ResponseDto.Amount);
            Assert.Equal(Enums.Operator.Multiply, read1ResponseDto.Operator);
            Assert.Equal(Enums.DiscountType.NoExtraCriteria, read1ResponseDto.DiscountType);

            // update
            var discountToUpdate = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "BOGUS DISCOUNT",
                Amount = 0.5m,
                Operator = Enums.Operator.Multiply,
                ParkingLotIds = [lotId],
            });

            var putresponse = await client.PutAsync($"discount/{read1ResponseDto.Id}", discountToUpdate);
            Assert.Equal(HttpStatusCode.OK, putresponse.StatusCode);

            // read (get all)
            var allDiscounts = await client.GetAsync("discount");
            var allDiscountsDto = await allDiscounts.Content.ReadFromJsonAsync<ReadDiscountDto[]>();
            Assert.Equal(HttpStatusCode.OK, allDiscounts.StatusCode);
            Assert.Single(allDiscountsDto);

            var onlyDiscount = allDiscountsDto[0];
            // check some values default state. These where set by the create. But have been overwritten by the default value via the put call.
            Assert.Null(onlyDiscount.Start);
            Assert.Null(onlyDiscount.End);
            Assert.Equal(Enums.DiscountType.NoExtraCriteria, onlyDiscount.DiscountType);
            Assert.Null(onlyDiscount.TypeSpecificData);

            var delResponse = await client.DeleteAsync($"discount/{onlyDiscount.Id}");
            Assert.Equal(HttpStatusCode.NoContent, delResponse.StatusCode);

            var allDiscountsAfterDelete = await client.GetAsync("discount");
            var allDiscountsAfterDeleteDto = await allDiscountsAfterDelete.Content.ReadFromJsonAsync<ReadDiscountDto[]>();

            Assert.NotNull(allDiscountsAfterDeleteDto);
            Assert.Empty(allDiscountsAfterDeleteDto);
        }

        [Fact]
        public async Task Test_CannotAccessDiscountsAsUser()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithUser1(_appfixutre);

            var getAllResponse = await client.GetAsync("discount");
            Assert.Equal(HttpStatusCode.Forbidden, getAllResponse.StatusCode);

        }

        [Fact]
        public async Task Test_DiscountChangesPriceOfReservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "GreatDeals.nl",
                Amount = 0.75m,
                Operator = Enums.Operator.Multiply,
            });

            var discountResponse = await client.PostAsync("discount", discountToAdd);
            Assert.Equal(HttpStatusCode.Created, discountResponse.StatusCode);


            var start = DateTime.UtcNow.AddHours(16).AddMinutes(30);
            var end = DateTime.UtcNow.AddHours(20);


            var reservationToAdd = JsonContent.Create(new WriteReservationDto()
            {
                LicensePlate = "uvt-54-tt",
                StartTime = start,
                EndTime = end, // reservation for 4 hours
                ParkingLotId = lotId,
            });

            var reservationResponse = await client.PostAsync("reservations", reservationToAdd);
            Assert.Equal(HttpStatusCode.OK, reservationResponse.StatusCode);

            var responseDto = await reservationResponse.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(start, responseDto.StartTime);
            Assert.Equal(end, responseDto.EndTime);
            Assert.Equal(15, responseDto.Cost);
        }


        [Fact]
        public async Task Test_PriceForReservationIsCorrect_PartialOverlap()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "GreatDeals.nl",
                Amount = 0.75m,
                Operator = Enums.Operator.Multiply,
                Start = DateTime.UtcNow.AddHours(40),
                End = DateTime.UtcNow.AddHours(48),
                ParkingLotIds = [lotId]
            });

            var discountResponse = await client.PostAsync("discount", discountToAdd);
            Assert.Equal(HttpStatusCode.Created, discountResponse.StatusCode);


            var start = DateTime.UtcNow.AddHours(42); // reservation for 5 hours
            var end = DateTime.UtcNow.AddHours(52);


            var reservationToAdd = JsonContent.Create(new WriteReservationDto()
            {
                LicensePlate = "uvt-54-tt",
                StartTime = start,
                EndTime = end,
                ParkingLotId = lotId,
            });

            var reservationResponse = await client.PostAsync("reservations", reservationToAdd);
            Assert.Equal(HttpStatusCode.OK, reservationResponse.StatusCode);

            var responseDto = await reservationResponse.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(start, responseDto.StartTime);
            Assert.Equal(end, responseDto.EndTime);
            Assert.Equal(90, responseDto.Cost); // if the cost is 110 no discount has been applied
        }

        [Fact]
        public async Task Test_PriceForReservationIsCorrect_DaysLongReservation()
        {
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            //var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            //{
            //    Title = "GreatDeals.nl",
            //    Amount = -10,
            //    Operator = Enums.Operator.Plus,
            //    Start = DateTime.UtcNow.AddDays(16),
            //    End = DateTime.UtcNow.AddDays(22),
            //    ParkingLotIds = [lotId]
            //});

            //var discountResponse = await client.PostAsync("discount", discountToAdd);
            //Assert.Equal(HttpStatusCode.OK, discountResponse.StatusCode);


            var start = DateTime.UtcNow.AddHours(20); // reservation for 5 hours
            var end = DateTime.UtcNow.AddHours(27);


            var reservationToAdd = JsonContent.Create(new WriteReservationDto()
            {
                LicensePlate = "uvt-54-tt",
                StartTime = start,
                EndTime = end,
                ParkingLotId = lotId,
            });

            var reservationResponse = await client.PostAsync("reservations", reservationToAdd);
            Assert.Equal(HttpStatusCode.OK, reservationResponse.StatusCode);

            var responseDto = await reservationResponse.Content.ReadFromJsonAsync<ReadReservationDto>();
            Assert.Equal(start, responseDto.StartTime);
            Assert.Equal(end, responseDto.EndTime);
            Assert.Equal(90, responseDto.Cost); // if the cost is 110 no discount has been applied
        }
        

        [Fact]
        public async Task Test_InvalidDiscountRejected_EndBeforeStart()
        {
            // arrange
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            var now = DateTime.Now;

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "BOGUS DISCOUNT",
                Amount = 0.75m,
                Operator = Enums.Operator.Multiply,
                ParkingLotIds = [lotId],
                Start = now.AddDays(800),
                End = now.AddDays(30),
                DiscountType = Enums.DiscountType.NoExtraCriteria
            });

            // act
            var createResponse = await client.PostAsync("discount", discountToAdd);
            var createResponseDto = await createResponse.Content.ReadAsStringAsync();

            //assert
            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
            Assert.Equal("end cannot be before start", createResponseDto);
        }

        [Fact]
        public async Task Test_InvalidDiscountRejected_NoExtraCriteriaCantHaveTypeSpecificData()
        {
            // arrange
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            var now = DateTime.Now;

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "BOGUS DISCOUNT",
                Amount = 0.75m,
                Operator = Enums.Operator.Multiply,
                ParkingLotIds = [lotId],
                Start = now,
                End = now.AddDays(30),
                DiscountType = Enums.DiscountType.NoExtraCriteria,
                TypeSpecificData = "55-tvg-75,66-dvl-6"
                
            });

            // act
            var createResponse = await client.PostAsync("discount", discountToAdd);
            var createResponseDto = await createResponse.Content.ReadAsStringAsync();

            //assert
            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
            Assert.Equal("without extra criteria the typespecific data must be null. but it has a value", createResponseDto);
        }


        [Fact]
        public async Task Test_InvalidDiscountRejected_CannotCreateNegativeMultiplyAmount()
        {
            // arrange
            await _appfixutre.ResetDB();
            var lotId = await EndToEndSeeding.SeedDatabase(_appfixutre);
            var client = await EndToEndSeeding.LoginWithAdmin(_appfixutre);

            var now = DateTime.Now;

            var discountToAdd = JsonContent.Create(new WriteDiscountDto()
            {
                Title = "Deal Of The Century",
                Amount = -0.75m,
                Operator = Enums.Operator.Multiply,
                ParkingLotIds = [lotId],
            });

            // act
            var createResponse = await client.PostAsync("discount", discountToAdd);
            var createResponseDto = await createResponse.Content.ReadAsStringAsync();

            //assert
            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
            Assert.Equal("discount cannot be a factor and have a negative amount, this would result in negative prices", createResponseDto);
        }
    }
}
