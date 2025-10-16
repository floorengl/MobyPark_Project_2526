using System.Net;
using MobyPark_api.Services.Testing;

namespace MobyPark_api.tests.VerifyTestSolution
{
    public class TestConnectionToSUT
    {
        [Fact]
        public void VerifyAConnectionToTheSUTExists()
        {
            Return10 SUTclass = new Return10();

            var actual = SUTclass.Get10();

            Assert.Equal(10, actual);
        }
    }
}
