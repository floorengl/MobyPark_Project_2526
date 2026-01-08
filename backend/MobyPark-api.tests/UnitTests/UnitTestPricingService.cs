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

        public ParkingLot MakeLot2()
        {
            return new ParkingLot()
            {
                Id = 345,
                Name = "The Mall Parking Roof",
                Location = "Along Highway",
                Address = "Laan van VN 10",
                Capacity = 4405,
                Tariff = 2,
                DayTariff = 40,
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
        public void Test_CalculatePrice_Discount_50percent_HalfPrice()
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
                ParkingLotIds = [lot.Id],
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(5, price);
        }

        [Fact]
        public void Test_CalculatePrice_MegaDiscount_PriceIsSetToZero_DoesntBecomeNegative()
        {
            // arrange
            DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            ParkingLot lot = MakeLot();

            Discount discount = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = -10_000_000m,
                Operator = Operator.Plus,
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount]);

            // assert
            Assert.Equal(0, price);
        }

        [Fact]
        public void Test_CalculatePrice_DiscountStopsHalfway()
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
                ParkingLotIds = [lot.Id],
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
                ParkingLotIds = [lot.Id],
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
                ParkingLotIds = [lot.Id],
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
                ParkingLotIds = [lot.Id],
            };

            Discount discount2 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = -1.5m,
                Operator = Operator.Plus,
                ParkingLotIds = [lot.Id],
            };

            Discount discount3 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = -0.5m,
                Operator = Operator.Plus,
                ParkingLotIds = [lot.Id],
            };

            Discount discount4 = new()
            {
                DiscountType = DiscountType.NoExtraCriteria,
                Amount = 0.25m,
                Operator = Operator.Multiply,
                ParkingLotIds = [lot.Id],
            };

            // act
            decimal price = PricingService.CalculatePrice(start, end, lot, [discount1, discount2, discount3, discount4]);

            // assert
            Assert.Equal(3m, price);
        }

        [Fact]
        public void Test_CalculatePrice_DiscountForMultipleLots()
        {
            {
                // arrange
                DateTime start = new DateTime(20, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime end = new DateTime(20, 1, 1, 9, 0, 0, DateTimeKind.Utc);
                ParkingLot lot1 = MakeLot();
                ParkingLot lot2 = MakeLot2();

                Discount discount1 = new()
                {
                    DiscountType = DiscountType.NoExtraCriteria,
                    Amount = 0.5m,
                    Operator = Operator.Multiply,
                    ParkingLotIds = [lot1.Id, lot2.Id],
                };

                Discount discount2 = new()
                {
                    DiscountType = DiscountType.NoExtraCriteria,
                    Amount = -15m,
                    Operator = Operator.Plus,
                    ParkingLotIds = [lot2.Id],
                };

                // act
                decimal price1 = PricingService.CalculatePrice(start, end, lot1, [discount1, discount2]);
                decimal price2 = PricingService.CalculatePrice(start, end, lot2, [discount1, discount2]);

                // assert
                Assert.Equal(5m, price1);
                Assert.Equal(2.5m, price2);
            }
        }








    }
}