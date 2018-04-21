using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DotGrid.Core.Json;
using DotGrid.TestHelpers.Io;
using DotGrid.TestHelpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace DotGrid.Binary2.Tests
{
    public abstract class JsonDataTestBase
    {
        private readonly JsonTestDocumentCollection testDocuments = 
            JsonTestDocumentCollection.FromDirectory(PathResolver.ResolveUpward(PathResolver.ApplicationRoot,"data/binary/benchmarks"),
        (reader,size) => reader.ToByteArray(size * 4));

        protected JsonTestDocumentCollection TestDocuments => testDocuments;
        
        protected readonly ITestOutputHelper Output;

        protected JsonDataTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        protected unsafe void PerformWriteOnly(string jsonText, [CallerMemberName]string id = null)
        {
            var jsonToken = JToken.Parse(jsonText);

            using (var reader = new JTokenReader(jsonToken))
            {
                var bytes = reader.ToByteArray(jsonText.Length * 4);
                
                var minifiedJson = jsonToken.ToString(Formatting.None);

                var dataSizes = new Dictionary<string, int>()
                {
                    {"Json Text (Formatted)", Size(jsonText)},
                    {"Json Text (Formatted & Compressed)", CompressedSize(jsonText)},
                    {"Json Text (Minified)", Size(minifiedJson)},
                    {"Json Text (Minified & Compressed)", CompressedSize(minifiedJson)},
                    {"Binary", Size(bytes)},
                    {"Binary (Compressed)", CompressedSize(bytes)}
                };

                var bestSize = dataSizes.OrderBy(d => d.Value).First();

                Output.WriteLine("----------------------------------------------------------------");
                Output.WriteLine($"{id} sizes");

                foreach (var size in dataSizes.OrderBy(d => d.Value))
                {
                    // Factor
                    double sizeIncrease = (((double) size.Value / (double) bestSize.Value) - 1.0) * 100.0;

                    Output.WriteLine($"{size.Key}: {size.Value} bytes, +{sizeIncrease:F1} %");
                }
            }
        }

        /*protected unsafe bool PerformRoundTrip(string jsonText, bool writeOut = false,
            [CallerMemberName] string id = null)
        {
            // Parse top token
            var jsonToken = JToken.Parse(jsonText);

            using (var reader = new JTokenReader(jsonToken))
            {
                var bytes = reader.ToByteArray(jsonText.Length * 4);

                var readContext = new BinaryFormatReadingContext();

                if (writeOut)
                {
                    File.WriteAllText($"{id}-expected.json", jsonToken.ToString());
                }

                fixed (byte* ptr = &bytes[0])
                {
                    var binaryReader = new BinaryFormatReader(readContext,
                        new BinaryFormatMemoryReader(ptr, 0, bytes.Length));

                    using (var writer = new JTokenWriter())
                    {
                        binaryReader.WriteTo(writer);

                        var finalToken = writer.Token;

                        if (writeOut)
                        {
                            File.WriteAllText($"{id}-actual.json", finalToken.ToString());
                        }

                        var result = jsonToken.IsStructurallyEqual(finalToken);

                        var minifiedJson = jsonToken.ToString(Formatting.None);

                        var dataSizes = new Dictionary<string, int>()
                        {
                            {"Json Text (Formatted)", Size(jsonText)},
                            {"Json Text (Formatted & Compressed)", CompressedSize(jsonText)},
                            {"Json Text (Minified)", Size(minifiedJson)},
                            {"Json Text (Minified & Compressed)", CompressedSize(minifiedJson)},
                            {"Binary", Size(bytes)},
                            {"Binary (Compressed)", CompressedSize(bytes)}
                        };

                        var bestSize = dataSizes.OrderBy(d => d.Value).First();

                        Output.WriteLine("----------------------------------------------------------------");
                        Output.WriteLine($"{id} sizes");

                        foreach (var size in dataSizes.OrderBy(d => d.Value))
                        {
                            // Factor
                            double sizeIncrease = (((double) size.Value / (double) bestSize.Value) - 1.0) * 100.0;

                            Output.WriteLine($"{size.Key}: {size.Value} bytes, +{sizeIncrease:F1} %");
                        }

                        return result;
                    }
                }
            }
        }*/

        protected int Size(string value)
        {
            return Encoding.UTF8.GetByteCount(value);
        }

        protected int Size(byte[] value)
        {
            return value.Length;
        }

        protected int CompressedSize(string value)
        {
            return Compress(value).Length;
        }

        protected int CompressedSize(byte[] value)
        {
            return Compress(value).Length;
        }

        protected byte[] Compress(string value)
        {
            return Compress(Encoding.UTF8.GetBytes(value));
        }

        protected byte[] Compress(byte[] value)
        {
            using (var sourceStream = new MemoryStream(value))
            {
                using (var targetStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(zipStream);
                    }

                    return targetStream.ToArray();
                }
            }
        }
    }
}