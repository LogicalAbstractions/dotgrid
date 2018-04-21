using System.IO;
using DotGrid.Core.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace DotGrid.Binary.Tests
{
    public class RoundTripTests : JsonDataTestBase
    {
        public RoundTripTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void RoundTripShouldWork()
        {
            foreach (var entry in TestDocuments)
            {
                Assert.True(PerformRoundTrip(entry.JsonText,false,entry.Id),$"{entry.Id} roundtrip failed");
            }
        }
    }
}