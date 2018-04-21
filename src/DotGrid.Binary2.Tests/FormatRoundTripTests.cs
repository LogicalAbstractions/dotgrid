using Xunit;
using Xunit.Abstractions;

namespace DotGrid.Binary2.Tests
{
    public class FormatRoundTripTests : JsonDataTestBase
    {
        public FormatRoundTripTests(ITestOutputHelper output) 
            : base(output)
        {
        }

        [Fact]
        public void WritingShouldWork()
        {
            foreach (var entry in TestDocuments)
            {
                PerformWriteOnly(entry.JsonText,entry.Id);
            }
        }

        [Fact]
        public void RoundTripShouldWork()
        {
            foreach (var entry in TestDocuments)
            {
                //Assert.True(PerformRoundTrip(entry.JsonText,false,entry.Id),$"{entry.Id} roundtrip failed");
            }
        }
    }
}