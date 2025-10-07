

namespace MobyPark_api.tests.TestServices
{
    [Collection("SharedDatabase")]
    public class TestAuth
    {
        private readonly DatabaseFixture _fixture;

        public TestAuth(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

    }
}

