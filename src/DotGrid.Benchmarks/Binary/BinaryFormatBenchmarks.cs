using System;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Utf8;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using DotGrid.Binary;
using DotGrid.TestHelpers.Json;
using Newtonsoft.Json.Linq;

namespace DotGrid.Benchmarks.Binary
{
    [CoreJob]
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class BinaryFormatBenchmarks : JsonDataBenchmarkBase
    {
        [BenchmarkCategory("RandomAccess")]
        [Benchmark(Baseline = true)]
        public void RandomJsonTextAccess()
        {
            UsingJsonTextReader("test03", reader =>
            {
                var array = JArray.Load(reader);
                var personObject = array[4] as JObject;
                var nameObject = personObject["name"];
                var firstName = nameObject["first"].Value<string>();

                if (firstName != "Jillian")
                {
                    throw new InvalidOperationException("Could not resolve property");
                }
            });
        }
        
        [BenchmarkCategory("RandomAccess")]
        [Benchmark]
        public void RandomJTokenAccess()
        {
            var token = TestDocuments["test03"].JsonToken;

            var array = (JArray) token;
            var personObject = array[4] as JObject;
            var nameObject = personObject["name"];
            var firstName = nameObject["first"].Value<string>();

            if (firstName != "Jillian")
            {
                throw new InvalidOperationException("Could not resolve property");
            }
        }

        [BenchmarkCategory("RandomAccess")]
        [Benchmark]
        public void RandomBinaryAccess()
        {
            UsingBinaryReader("test03", reader =>
            {
                reader.Down(4);
                reader.TryGetPropertyId(new Utf8String("name"), out var nameId);
                reader.Down(nameId);
                reader.TryGetPropertyId(new Utf8String("first"), out var firstId);
                var firstName = reader.GetStringValue(firstId);

                if (firstName != "Jillian")
                {
                    throw new InvalidOperationException("Could not resolve property");
                }
            });    
        }
        
        [BenchmarkCategory("FullEncode")]
        [Benchmark(Baseline = true)]
        public void FullJsonTextEncode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullJsonTextEncode(entry);
            }
        }

        [BenchmarkCategory("FullEncode")]
        [Benchmark]
        public void FullJTokenEncode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullJTokenEncode(entry);
            }
        }

        [BenchmarkCategory("FullEncode")]
        [Benchmark]
        public void FullBinaryEncode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullBinaryEncode(entry);
            }
        }

        [BenchmarkCategory("FullDecode")]
        [Benchmark(Baseline = true)]
        public void FullJsonTextDecode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullJsonTextDecode(entry);
            }
        }

        [BenchmarkCategory("FullDecode")]
        [Benchmark]
        public void FullJTokenDecode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullJTokenDecode(entry);
            }
        }

        [BenchmarkCategory("FullDecode")]
        [Benchmark]
        public void FullBinaryDecode()
        {
            foreach (var entry in TestDocuments.Ids)
            {
                FullBinaryDecode(entry);
            }
        }
    }
}