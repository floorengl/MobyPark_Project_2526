using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Vehicle;
using MobyPark_api.Services;
using Xunit.Sdk;

namespace MobyPark_api.tests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class TestServicesVehicle
    {
        private readonly DatabaseFixture _fixture;
        public TestServicesVehicle(DatabaseFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task TestGetAllVehiclesShouldGet2Rows()
        {
            await _fixture.ResetDB();


            var car1 = new Vehicle() { LicensePlate = "jx-232-t", Color = "red", UserId = 1 };
            var car1DTO = new VehicleDto() { LicensePlate = "jx-232-t", Color = "red" };

            var car2 = new Vehicle() { LicensePlate = "tsx-554-j", Color = "beige", UserId = 1 };
            var car2DTO = new VehicleDto() { LicensePlate = "tsx-554-j", Color = "beige" };

            // arrange
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            db.Add(new User() { Username="Testuser", Id = 1});
            db.SaveChanges();
            db.AddRange(car1, car2);
            db.SaveChanges();

            // act
            var actual = (await sut.GetAllVehicles()).ToArray();
            Assert.Equal(actual[0].LicensePlate, car1DTO.LicensePlate);
            Assert.Equal(actual[1].Color, car2DTO.Color);
            Assert.Equal(2, actual.Length);
        }

        [Fact]
        public async Task GetUserVehiclesAsync_NoVehicles_EmptyCollection()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            db.Add(new User() { Id = 0 });
            db.SaveChanges();
            var cars = await sut.GetUserVehiclesAsync(0);

            //act
            Assert.Empty(cars);

        }

        [Fact]
        public async Task GetUserVehiclesAsync_OneVehicle_GetsOneVehicle()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            db.Add(new User() { Id = 1 });
            db.SaveChanges();
            var car1 = new Vehicle() { LicensePlate = "jx-232-t", Color = "red", UserId = 1 };
            db.Add(car1);
            db.SaveChanges();

            var cars = await sut.GetUserVehiclesAsync(1);

            //act
            Assert.NotEmpty(cars);
        }

        [Fact]
        public async Task GetUserVehiclesAsync_UnknownUser_Empty()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            var cars = await sut.GetUserVehiclesAsync(1);

            //act
            Assert.Empty(cars);
        }

        [Fact]
        public async Task GetVehicleByUsernameAsync_UserWithOneCar_GetsCar()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            db.Add(new User() { Id = 1 , Username="Arnout"});
            db.SaveChanges();
            var car1 = new Vehicle() { LicensePlate = "jx-232-t", Color = "red", UserId = 1 };
            db.Add(car1);
            db.SaveChanges();

            var cars = await sut.GetVehiclesByUsernameAsync("Arnout");

            //act
            Assert.Equal(car1.LicensePlate, cars.First().LicensePlate);
        }

        [Fact]
        public async Task GetByIdAsync_UserWithOneCar_GetsCar()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            var user = db.Add(new User() { Id = 1, Username = "Arnout" }).Entity;
            db.SaveChanges();
            var car1 = db.Add(new Vehicle() { LicensePlate = "jx-232-t", Color = "red", UserId = 1, CreatedAt = DateTime.UtcNow }).Entity;
            db.SaveChanges();
            var car = await sut.GetByIdAsync(car1.Id, user.Id);

            //act
            Assert.NotNull(car);
            Assert.Equal(car1.LicensePlate, car.LicensePlate);
        }

        [Fact]
        public async Task GetByIdAsync_UserWithoutCars_ReturnsNull()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);

            db.Add(new User() { Id = 1, Username = "Arnout" });
            db.SaveChanges();

            var car = await sut.GetByIdAsync(3, 1);

            //act
            Assert.Null(car);
        }

        [Fact]
        public async Task GetByIdAsync_UnkownUser_ReturnsNull()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            //act
            var car = await sut.GetByIdAsync(12, 4);

            //assert
            Assert.Null(car);
        }

        [Fact]
        public async Task CreateAsync_NewVehicle_CreatesVehicle()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            var vehicleDto = new VehicleDto() { Id = 1, LicensePlate = "toffepeer" };

            db.Add(new User() { Id = 1, Username = "Arnout" });
            db.SaveChanges();

            //act
            await sut.CreateAsync(vehicleDto, 1);
            var car = db.Vehicles.FirstOrDefault(x => x.LicensePlate == vehicleDto.LicensePlate);

            //assert
            Assert.NotNull(car);

        }

        [Fact]
        public async Task CreateAsync_TwoSameCars_CannotAddSecond()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            var vehicleDto = new VehicleDto() { Id = 1, LicensePlate = "toffepeer" };

            db.Add(new User() { Id = 1, Username = "Arnout" });
            db.SaveChanges();

            //act
            await sut.CreateAsync(vehicleDto, 1);
            var secondCar = await sut.CreateAsync(vehicleDto, 1);

            //assert
            Assert.Null(secondCar);
        }
        
        [Fact]
        public async Task UpdateAsync_AddingNewInfo_AddedNewInformation()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            var vehicleDto = new VehicleDto() {LicensePlate = "boe" };
            var ElaborateVehicle = new VehicleDto()
            {
                LicensePlate = "boe",
                Color = "teal",
            };

            var actualUser = db.Add(new User() {Username = "Arnout" }).Entity;
            db.SaveChanges();
            var actualVehicle = await sut.CreateAsync(vehicleDto, actualUser.Id);

            //act
            await sut.UpdateAsync(1, ElaborateVehicle, actualUser.Id);
            var allVehicles = db.Vehicles.ToList();
            var vehicle = db.Vehicles.FirstOrDefault(v => v.Color == "teal");

            //assert
            Assert.NotNull(vehicle);
            Assert.Equal("teal", vehicle.Color);
        }

        [Fact]
        public async Task UpdateAsync_AddingLessInfo_RemoveOldMaterial()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            var vehicleDto = new VehicleDto() { Id = 1, LicensePlate = "toffepeer", Color = "teal", };
            var ElaborateVehicle = new VehicleDto()
            {
                Id = 1,
                LicensePlate = "toffepeer",
            };

            var actualUser = db.Add(new User() { Id = 1, Username = "Arnout" }).Entity;
            db.SaveChanges();
            var actualVehicle = await sut.CreateAsync(vehicleDto, actualUser.Id);

            //act
            await sut.UpdateAsync(actualUser.Id, ElaborateVehicle, 1);
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == "toffepeer");

            //assert
            Assert.NotNull(vehicle);
            Assert.Null(vehicle.Color);
        }

        [Fact]
        public async Task DeleteAsync_ValidCar_GetsRemoved()
        {
            //arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var repo = new VehicleRepository(db);
            var sut = new VehicleService(repo);
            var user = db.Add(new User() { FullName = "Hendrick"}).Entity;
            db.SaveChanges();
            var car = db.Add(new Vehicle() { LicensePlate = "55-rvp-12", UserId=user.Id }).Entity;
            db.SaveChanges();

            //act
            await sut.DeleteAsync(1, user.Id);
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == "55-rvp-12");

            Assert.Null(vehicle);
        }
    }
}



//Task<bool> DeleteAsync(long id, long userId);