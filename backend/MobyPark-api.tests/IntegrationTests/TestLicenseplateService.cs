using MobyPark_api.Data.Models;
using MobyPark_api.Dtos;
using MobyPark_api.Dtos.Reservation;

namespace MobyPark_api.tests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class TestLicenseplateService
    {
        private readonly DatabaseFixture _fixture;
        public TestLicenseplateService(DatabaseFixture fixture) => _fixture = fixture;

        private (ILicenseplateService, AppDbContext) GetSut()
        {
            var db = _fixture.CreateContext();
            var serice = new LicenseplateService(db, new SessionService(db), new ReservationService(db));
            return (serice, db);
        }

        private ParkingLot AddParking(AppDbContext db)
        {
            var parkinglot = new ParkingLot() {
                Name = "Objectief te duur parkeren",
                Address = "niemandsland 23",
                Capacity = 2,
                Coordinates = "nergens",
                Location = "echt niet te vinden",
                CreatedAt = DateTime.UtcNow,
                DayTariff = 30,
                Tariff = 5 };
            db.ParkingLots.Add(parkinglot);
            db.SaveChanges();
            return parkinglot;
        }

        [Fact]
        public async Task Test_LicenseplatesAsync_HappyInsertion()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var checkinDTO = new CheckInDto() {ParkingLotId = lot.Id, LicensePlateName = "2-platy-1" };
            var canceler = new CancellationToken();

            // act
            await SUT.LicenseplatesAsync(checkinDTO, canceler);
            var plate = await SUT.GetByPlateAsync("2-platy-1", canceler);

            // assert
            Assert.NotNull(plate);
        }

        [Fact]
        public async Task Test_DeleteAsync_TestCheckout()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var checkinDTO = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "2-janman-1" };
            var canceler = new CancellationToken();

            // act
            // add and remove a vehicle. So it shouldn't be present anymore.
            await SUT.LicenseplatesAsync(checkinDTO, canceler);
            await SUT.DeleteAsync("2-janman-1", canceler);
            var plate = await SUT.GetByPlateAsync("2-janman-1", canceler);

            // assert
            Assert.Null(plate);
        }

        [Fact]
        public async Task Test_LicenseplatesAsync_CheckingInTooManyCars()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var checkinDTO = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "2-janman-1" };
            var canceler = new CancellationToken();

            // act
            // add and remove a vehicle. So it shouldn't be present anymore.
            await SUT.LicenseplatesAsync(checkinDTO, canceler);
            await SUT.DeleteAsync("2-janman-1", canceler);
            var plate = await SUT.GetByPlateAsync("2-janman-1", canceler);

            // assert
            Assert.Null(plate);
        }

        [Fact]
        public async Task Test_LicenseplatesAsync_DriveOutCarAndAddMore()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var checkinDTO1 = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "erikman" };
            var checkinDTO2 = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "benman" };
            var checkinDTO3TooMuch = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "gertman" };
            var canceler = new CancellationToken();

            // act
            // the first two vehicles should get an ID returned. But the last checkin will get 0 returend because the parkinglot is full.
            await SUT.LicenseplatesAsync(checkinDTO1, canceler);
            var actual_parked = await SUT.LicenseplatesAsync(checkinDTO2, canceler);
            var actual_overflow = await SUT.LicenseplatesAsync(checkinDTO3TooMuch, canceler);

            // assert
            Assert.True(actual_parked.Item1 != 0);
            Assert.Equal(0, actual_overflow.Item1);
        }

        [Fact]
        public async Task Test_LicenseplatesAsync_ReservationBlocksOtherCars()
        {
            // arrange
            await _fixture.ResetDB();
            var (SUT, db) = GetSut();
            var lot = AddParking(db);

            var reservationSerivce = new ReservationService(db);

            var reservation = new WriteReservationDto()
            {
                LicensePlate = "berendman",
                EndTime = DateTime.UtcNow.AddHours(3),
                StartTime = DateTime.UtcNow,
                ParkingLotId = lot.Id
            };

            var actualReservation = await reservationSerivce.Post(reservation);
            await reservationSerivce.PayForReservation(actualReservation.Id);

            var checkinDTO1 = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "gijsman" };
            var checkinDTO2TooMuch = new CheckInDto() { ParkingLotId = lot.Id, LicensePlateName = "albertman" };
            var canceler = new CancellationToken();

            // act
            // the first two vehicles should get an ID returned. But the last checkin will get 0 returend because the parkinglot is full.
            var actual_parked = await SUT.LicenseplatesAsync(checkinDTO1, canceler);
            var actual_overflow = await SUT.LicenseplatesAsync(checkinDTO2TooMuch, canceler);

            // assert
            Assert.NotNull(actualReservation);
            Assert.True(actual_parked.Item1 != 0);
            Assert.Equal(0, actual_overflow.Item1);
        }
    }
}