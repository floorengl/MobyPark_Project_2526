using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobyPark_api.Data.Models;
using MobyPark_api.Enums;

namespace MobyPark_api.tests.UnitTests
{
    public class UnitTestPricingService
    {
        public ParkingLot MakeLot()
        {
            return new ParkingLot()
            {
                Id = 44,
                Name = "Lotto",
                Location = "Center",
                Address = "Baker Street 101",
                Capacity = 100,
                Tariff = 1,
                DayTariff = 10,
                CreatedAt = DateTime.Now,
                Coordinates = "Cool Place"
            };
        }

        [Fact]
        public void Test_CalculatePrice_NoDiscount_10_Hours_5Minutes()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 10, 5, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(11, price);
        }

        [Fact]
        public void Test_CalculatePrice_NoDiscount_23_Hours()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 23, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();
            
            
            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(24, price);
        }

        [Fact]
        public void Test_CalculatePrice_NoDiscount_23_Hours_59Minutes()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 23, 59, 59, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(24, price);
        }

        [Fact]
        public void Test_CalculatePrice_NoDiscount_1Day_DatTarriff()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(20, price);
        }

        [Fact]
        public void Test_CalculatePrice_2_Minutes_Free()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 0, 2, 59, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(0, price);
        }

        [Fact]
        public void Test_CalculatePrice_3_Minutes_HasCost()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 0, 3, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(1, price);
        }

        [Fact]
        public void Test_CalculatePrice_Negative_Time_Free()
        {
            // arrange
            DateTime start = new DateTime(1, 10, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(0, price);
        }

        [Fact]
        public void Test_CalculatePrice_3days()
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 4, 0, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(40, price);
        }

        [Fact]
        public void Test_CalculatePrice_200days() // test for float point inaccuracy
        {
            // arrange
            DateTime start = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(200);
            ParkingLot lot = MakeLot();


            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, []);

            // assert
            Assert.Equal(2010, price);
        }

        [Fact]
        public void Test_CalculatePrice_Discount_50percent_HalfPrice() // test for float point inaccuracy
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Start = new DateTime(19, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                End = new DateTime(21, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Amount = 0.5m,
                Operator = Operator.Multiply,
                ParkingLotId = lot.Id,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(5, price);
        }

        [Fact]
        public void Test_CalculatePrice_DiscountStopsHalfway() // test for float point inaccuracy
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Start = new DateTime(19, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                End = new DateTime(20, 1, 1, 4, 30, 0, DateTimeKind.Utc),
                Amount = 0.5m,
                Operator = Operator.Multiply,
                ParkingLotId = lot.Id,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(7.5m, price);
        }

        [Fact]
        public void Test_CalculatePrice_NoEnd_SumDiscount()
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Start = new DateTime(19, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                End = null,
                Amount = -5m,
                Operator = Operator.Plus,
                ParkingLotId = lot.Id,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(5m, price);
        }

        [Fact]
        public void Test_CalculatePrice_DiscountEndIntersectStartNull()
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Start = null,
                End = new DateTime(20, 1, 1, 4, 3, 0, DateTimeKind.Utc),
                Amount = 0.5m,
                Operator = Operator.Multiply,
                ParkingLotId = lot.Id,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(7.75m, price);
        }


        [Fact]
        public void Test_CalculatePrice_MultipleDiscounts()
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount1 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = 0.5m,
                Operator = Operator.Multiply,
                ParkingLotId = lot.Id,
            };

            Discount discount2 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = -1.5m,
                Operator = Operator.Plus,
                ParkingLotId = lot.Id,
            };

            Discount discount3 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = -0.5m,
                Operator = Operator.Plus,
                ParkingLotId = lot.Id,
            };

            Discount discount4 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = 0.25m,
                Operator = Operator.Multiply,
                ParkingLotId = lot.Id,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount1, discount2, discount3, discount4]);

            // assert
            Assert.Equal(3m, price);
        }
    }
}
//public long Id { get; set; }

//[Required]
//public string? Name { get; set; }

//[Required]
//public string? Location { get; set; }

//public string? Address { get; set; }

//public long Capacity { get; set; }

//public float? Tariff { get; set; }

//public float? DayTariff { get; set; }

//public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//public string? Coordinates { get; set; }