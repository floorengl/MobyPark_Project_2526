using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Vehicle;
using MobyPark_api.Services.VehicleService;
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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);

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
            var sut = new VehicleService(db);
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
            var sut = new VehicleService(db);
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
            var sut = new VehicleService(db);
            var vehicleDto = new VehicleDto() { Id = 1, LicensePlate = "toffepeer" };

            db.Add(new User() { Id = 1, Username = "Arnout" });
            db.SaveChanges();

            //act
            await sut.CreateAsync(vehicleDto, 1);
            var dangerousCode = async () => await sut.CreateAsync(vehicleDto, 1);

            //assert
            await Assert.ThrowsAsync<ArgumentException>(dangerousCode);
        }
        
        [Fact]
        public async Task UpdateAsync_AddingNewInfo_AddedNewInformation()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var sut = new VehicleService(db);
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
            await sut.UpdateAsync(actualVehicle.Id, ElaborateVehicle, actualUser.Id);
            var allVehicles = db.Vehicles.ToList();
            var vehicle = db.Vehicles.FirstOrDefault(v => v.Color == "teal");

            //assert
            Assert.NotNull(vehicle);
            Assert.Equal("teal", vehicle.Color);
        }

        [Fact]
        public async Task UpdateAsync_AddingLessInfo_KeepsNonOverridenInfo()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var sut = new VehicleService(db);
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
            await sut.UpdateAsync(actualUser.Id, ElaborateVehicle, actualVehicle.Id);
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == "toffepeer");

            //assert
            Assert.NotNull(vehicle);
            Assert.Equal("teal", vehicle.Color);
        }

        [Fact]
        public async Task UpdateAsync_NoLicensePlate_ThrowsError()
        {
            // arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var sut = new VehicleService(db);
            var vehicleDto = new VehicleDto() { Id = 1, LicensePlate = "toffepeer", Color = "distinct" };
            var BadVehicle = new VehicleDto(){Id = 1, Color = "distinct" };

            var actualUser = db.Add(new User() { Id = 1, Username = "Arnout" }).Entity;
            db.SaveChanges();
            var actualVehicle = await sut.CreateAsync(vehicleDto, actualUser.Id);
            db.SaveChanges();

            //act
            await sut.UpdateAsync(actualUser.Id, BadVehicle, actualVehicle.Id);
            var vehicle = db.Vehicles.FirstOrDefault(v => v.Color == "distinct");

            //assert
            Assert.NotNull(vehicle);
            Assert.Equal("toffepeer", vehicle.LicensePlate);
        }

        [Fact]
        public async Task DeleteAsync_ValidCar_GetsRemoved()
        {
            //arrange
            await _fixture.ResetDB();
            var db = _fixture.CreateContext();
            var sut = new VehicleService(db);
            var user = db.Add(new User() { FullName = "Hendrick"}).Entity;
            db.SaveChanges();
            var car = db.Add(new Vehicle() { LicensePlate = "55-rvp-12", UserId=user.Id }).Entity;
            db.SaveChanges();

            //act
            await sut.DeleteAsync(car.Id, user.Id);
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == "55-rvp-12");

            Assert.Null(vehicle);
        }
    }
}



//Task<bool> DeleteAsync(long id, long userId);