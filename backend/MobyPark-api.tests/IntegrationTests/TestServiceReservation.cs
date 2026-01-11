using Microsoft.Identity.Client;
using MobyPark_api.Data.Models;
using MobyPark_api.Data.Repositories;
using MobyPark_api.Dtos;
using MobyPark_api.Dtos.Reservation;

namespace MobyPark_api.tests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class TestReservations
    {
        private readonly DatabaseFixture _fixture;
        public TestReservations(DatabaseFixture fixture) => _fixture = fixture;


        private (IReservationService, AppDbContext) GetSut()
        {
            var db = _fixture.CreateContext();
            var lotRepo = new ParkingLotRepository(db);
            var serice = new ReservationService(new ReservationRepository(db), lotRepo, new PricingService(new DiscountRepository(db), lotRepo), new PaymentService(new PaymentRepository(db)));
            return (serice, db);
        }


        private ParkingLot AddParking(AppDbContext db)
        {
            var parkinglot = new ParkingLot()
            {
                Name = "Objectief te duur parkeren",
                Address = "niemandsland 23",
                Capacity = 2,
                Coordinates = "nergens",
                Location = "echt niet te vinden",
                CreatedAt = DateTime.UtcNow,
                DayTariff = 30,
                Tariff = 5
            };
            db.ParkingLots.Add(parkinglot);
            db.SaveChanges();
            return parkinglot;
        }


        private static WriteReservationDto makeNormal(long lotId) =>
             new WriteReservationDto() { LicensePlate = "regular", StartTime = DateTime.UtcNow.AddHours(9), EndTime = DateTime.UtcNow.AddHours(11), ParkingLotId = lotId };


        private static WriteReservationDto makeLong(long lotId) =>
            new WriteReservationDto() { LicensePlate = "longer", StartTime = DateTime.UtcNow.AddHours(10), EndTime = DateTime.UtcNow.AddHours(38), ParkingLotId = lotId };


        private static WriteReservationDto makeShort(long lotId) =>
            new WriteReservationDto() { LicensePlate = "shorter", StartTime = DateTime.UtcNow.AddHours(10), EndTime = DateTime.UtcNow.AddHours(10.30), ParkingLotId = lotId };


        [Fact]
        public async Task Test_GetAll_GetsEverything()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            await SUT.Post(makeShort(lot.Id));
            await SUT.Post(makeLong(lot.Id));

            // act
            var content = await SUT.GetAll();

            // assert
            Assert.NotEmpty(content);
            Assert.Equal(2, content.Length);
            Assert.Contains(content, (dto => dto.LicensePlate == "longer"));
        }


        [Fact]
        public async Task Test_GetAll_ReturnsEmptyArr()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            // act
            var content = await SUT.GetAll();

            // assert
            Assert.NotNull(content);
            Assert.Empty(content);
        }


        [Fact]
        public async Task Test_GetById_CanBeRetrieved()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            await SUT.Post(makeShort(lot.Id));
            var dto = await SUT.Post(makeLong(lot.Id));


            // act
            var actual = await SUT.GetById(dto!.Id);

            // assert
            Assert.Equal("longer", actual.LicensePlate);
        }


        [Fact]
        public async Task Test_GetById_NonExistant_ReturnsNull()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            await SUT.Post(makeShort(lot.Id));
            var dto = await SUT.Post(makeLong(lot.Id));


            // act
            var actual = await SUT.GetById(new Guid().ToString());

            // assert
            Assert.Null(actual);
        }


        [Fact]
        public async Task Test_Put_CanEdit_First()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            await SUT.Post(makeShort(lot.Id));

            var before = await SUT.Post(makeLong(lot.Id));

            //act
            var after = await SUT.Put(before.Id, makeShort(lot.Id));

            Assert.NotNull(after);
            Assert.Equal(before.Id, after.Id);
            Assert.Equal("shorter", after.LicensePlate);
        }


        [Fact]
        public async Task Test_Put_AtCapactiyGarage_CanStillEdit()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            var r1 = await SUT.Post(makeShort(lot.Id));
            //await SUT.PayForReservation(r1.Id);

            var r2 = await SUT.Post(makeShort(lot.Id));
            //await SUT.PayForReservation(r2.Id);

            var before = await SUT.Post(makeLong(lot.Id));

            //act
            var after = await SUT.Put(before.Id, makeNormal(lot.Id));

            Assert.NotNull(after);
            Assert.Equal(before.Id, after.Id);
            Assert.Equal("regular", after.LicensePlate);
        }


        [Fact]
        public async Task Test_Delete_NonExistant_CannotRemove()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            var existantId = (await SUT.Post(makeShort(lot.Id))).Id;

            //act
            var returned = await SUT.Delete(new Guid().ToString());

            var stillExists = SUT.GetById(existantId);

            //assert
            Assert.NotNull(stillExists);
            Assert.Null(returned);
        }


        [Fact]
        public async Task Test_Delete_Exists_IsRemoved()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            var existantId = (await SUT.Post(makeShort(lot.Id))).Id;

            //act
            var returned = await SUT.Delete(existantId);

            var stillExists = SUT.GetById(existantId);

            //assert
            Assert.NotNull(existantId);
        }


        [Fact]
        public async Task Test_IsReservationAllowed_Normal_Allowed()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            var existantId = (await SUT.Post(makeShort(lot.Id))).Id;

            //act
            var actual = await SUT.IsReservationAllowed(makeShort(lot.Id));

            //assert
            Assert.True(actual.Item1);
        }


        [Fact]
        public async Task Test_IsReservationAllowed_ReservationStartsTooSoon_Forbidden()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "platy",
                StartTime = DateTime.UtcNow.AddMinutes(29),
                EndTime = DateTime.UtcNow.AddHours(11),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("Reservation cannot start in the past or within 30 minutes from now", actual.Item2);
        }

        [Fact]
        public async Task Test_IsReservationAllowed_EndTimeSmallerThanStart()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "platy",
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(8),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("EndTime is smaller than StartTime", actual.Item2);
        }


        [Fact]
        public async Task Test_IsReservationAllowed_14MinutesReservation_Forbidden()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "platy",
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(10).AddMinutes(14),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("Reservation must be at least 15 minutes", actual.Item2);
        }


        [Fact]
        public async Task Test_IsReservationAllowed_15MinutesReservation_Allowed()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "12345678901234567890",
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(10).AddMinutes(15),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.True(actual.Item1);
        }

        [InlineData("")]
        [InlineData(null)]
        [Theory]
        public async Task Test_IsReservationAllowed_LiceseplateEmpty_Forbidden(string plate)
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = plate,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(11),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("licenseplate is empty", actual.Item2);
        }

        [Fact]
        public async Task Test_IsReservationAllowed_LiceseplateNull_TooLong()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "1234567890-1234567890",
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(11),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("numberplate contains too many characters", actual.Item2);
        }


        [Fact]
        public async Task Test_IsReservationAllowed_NonexistantLot_Forbidden()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "22-tgv-43",
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(11),
                ParkingLotId = 473562
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("Parkinglot ID does not exist in the database", actual.Item2);
        }

        [Fact]
        public async Task Test_IsReservationAllowed_Overflow_Forbidden()
        {
            //arange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);
            var r1 = await SUT.Post(makeShort(lot.Id));
            //await SUT.PayForReservation(r1.Id);

            var r2 = await SUT.Post(makeNormal(lot.Id));
            //await SUT.PayForReservation(r2.Id);

            var dto = new WriteReservationDto()
            {
                LicensePlate = "22-tgv-43",
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(11),
                ParkingLotId = lot.Id
            };
            //act
            var actual = await SUT.IsReservationAllowed(dto);

            //assert
            Assert.False(actual.Item1);
            Assert.Equal("Parkinglot is full", actual.Item2);
        }
    }
}