using System;
using System.Collections.Generic;
using System.IO;
using DotGrid.Binary;
using DotGrid.TestHelpers.Io;
using DotGrid.TestHelpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotGrid.Benchmarks.Binary
{
    public abstract class JsonDataBenchmarkBase
    {
        private readonly JsonTestDocumentCollection testDocuments = 
            JsonTestDocumentCollection.FromDirectory(PathResolver.ResolveUpward(PathResolver.ApplicationRoot,"data/binary/benchmarks"),
                (reader,size) => reader.ToByteArray(size * 4));
        
        private readonly BinaryFormatReadingContext readingContext = new BinaryFormatReadingContext();
        private readonly BinaryFormatWritingContext writingContext = new BinaryFormatWritingContext();
        private readonly byte[] dataBuffer = new byte[1024 * 1024 * 10];

        protected JsonTestDocumentCollection TestDocuments => testDocuments;
        
        protected unsafe void UsingBinaryReader(string id,Action<BinaryFormatReader> action)
        {
            var bytes = TestDocuments[id].BinaryBytes;

            fixed (byte* ptr = &bytes[0])
            {
                var memoryReader = new BinaryFormatMemoryReader(ptr, 0, bytes.Length);
                
                var reader = new BinaryFormatReader(readingContext,memoryReader);
                
                action.Invoke(reader);
            }
        }

        protected void UsingJsonTextReader(string id, Action<JsonReader> action)
        {
            using (var jsonTextReader = new JsonTextReader(new StringReader(TestDocuments[id].JsonText)))
            {
                action.Invoke(jsonTextReader);
            }
        }

        protected void UsingJsonTokenReader(string id, Action<JsonReader> action)
        {
            using (var jtokenReader = new JTokenReader(TestDocuments[id].JsonToken))
            {
                action.Invoke(jtokenReader);
            }
        }

        protected void FullJsonTextEncode(string id)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(new MemoryStream())))
            {
                UsingJsonTokenReader(id, reader =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    reader.WriteTo(writer);
                });
            }
        }

        protected void FullJTokenEncode(string id)
        {
            using (var writer = new JTokenWriter())
            {
                UsingJsonTokenReader(id, reader =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    reader.WriteTo(writer);
                });
            }
        }

        protected unsafe void FullBinaryEncode(string id)
        {
            fixed (byte* ptr = &dataBuffer[0])
            {
                var memoryWriter = new BinaryFormatMemoryWriter(ptr,0,dataBuffer.Length);
                var writer = new BinaryFormatWriter(writingContext,memoryWriter);
                
                UsingJsonTokenReader(id, reader =>
                {
                    writer.WriteStartDocument();
                    reader.WriteTo(writer);
                    writer.WriteEndDocument();
                });
            }
        }

        protected void FullJsonTextDecode(string id)
        {
            UsingJsonTextReader(id,WalkJsonReader);
        }

        protected void FullJTokenDecode(string id)
        {
            UsingJsonTokenReader(id,WalkJsonReader);
        }

        protected void FullBinaryDecode(string id)
        {
            UsingBinaryReader(id,WalkContainer);
        }

        private void WalkJsonReader(JsonReader reader)
        {
            while (reader.Read())
            {
                var type = reader.TokenType;
                var value = reader.Value;

                var x = (type, value);
            }
        }
        
        private void WalkContainer(BinaryFormatReader reader)
        {
            for (int i = 0; i < reader.ElementCount; ++i)
            {
                int propertyIdOrIndex = i;
                
                if (reader.ContainerType == BinaryFormatValueType.Object)
                {
                    propertyIdOrIndex = reader.GetPropertyId(propertyIdOrIndex);
                }

                var propertyType = reader.GetValueType(propertyIdOrIndex);

                switch (propertyType)
                {
                    case BinaryFormatValueType.Array:
                        reader.Down(propertyIdOrIndex);
                        WalkContainer(reader);
                        reader.Up();
                        break;
                    case BinaryFormatValueType.Blob:
                        break;
                    case BinaryFormatValueType.Boolean:
                        reader.GetBooleanValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Byte:
                        reader.GetByteValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Document:
                        break;
                    case BinaryFormatValueType.Double:
                        reader.GetDoubleValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Float:
                        reader.GetFloatValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Int:
                        reader.GetIntValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Long:
                        reader.GetLongValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Null:
                        break;
                    case BinaryFormatValueType.Object:
                        reader.Down(propertyIdOrIndex);
                        WalkContainer(reader);
                        reader.Up();
                        break;
                    case BinaryFormatValueType.Short:
                        reader.GetShortValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.String:
                        reader.GetStringValue(propertyIdOrIndex);
                        break;
                    case BinaryFormatValueType.Undefined:
                        break;
                }
            }
        }
    }
}
