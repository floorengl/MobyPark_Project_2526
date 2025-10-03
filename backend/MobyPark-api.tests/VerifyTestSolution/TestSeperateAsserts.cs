namespace MobyPark_api.tests.VerifyTestSolution
{
    public class TestSeperateAsserts
    {
        [Fact]
        public void Test_Assert()
        {
            Assert.Equal(1, 1);
        }

        [Fact]
        public void Test_AssertWithVariable()
        {
            string variable = "WHOOOOO";
            Assert.Equal("WHOOOOO", variable);
        }

        [Fact]
        public void SameTests()
        {
            string variable = "WHOOOOO";
            Assert.Equal("WHOOOOO", variable);
        }
    }
}
