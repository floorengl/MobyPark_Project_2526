

namespace MobyPark_api.tests.UnitTests
{
    public class UnitTestReservationService
    {

        public ((DateTime, DateTime, DateTime, DateTime), bool) GetDataFor_Test_DoTimesOverlap(int i)
        {
            return new ((DateTime, DateTime, DateTime, DateTime), bool)[] {
                ((new DateTime(10), new DateTime(20), new DateTime(30), new DateTime(40)), false), // no overlap
                ((new DateTime(10), new DateTime(20), new DateTime(12), new DateTime(19)), true), // second fits in first
                ((new DateTime(10), new DateTime(20), new DateTime(9), new DateTime(21)), true), // first fits in second
                ((new DateTime(10), new DateTime(20), new DateTime(6), new DateTime(11)), true), // second overlaps with start first
                ((new DateTime(10), new DateTime(20), new DateTime(19), new DateTime(21)), true), // second overlaps with end first
                ((new DateTime(6), new DateTime(11), new DateTime(10), new DateTime(20)), true), // first overlaps with start second
                ((new DateTime(10), new DateTime(20), new DateTime(20), new DateTime(40)), false), // ends touch no overlap
                ((new DateTime(2015, 5, 1, 10, 0, 0), new DateTime(2015, 5, 1, 11, 0, 0), new DateTime(2025, 5, 1, 9, 0, 0), new DateTime(2025, 5, 1, 10, 0 ,0)), false), // no overlap with regular dates ends touch
            }[i];
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void Test_DoTimesOverlap(int index)
        {
            // arrange
            (var data, bool expected) = GetDataFor_Test_DoTimesOverlap(index);
            //act
            var actual = ReservationService.DoTimesOverlap(data.Item1, data.Item2, data.Item3, data.Item4);
            Assert.Equal(expected, actual);
        }
    }
}
