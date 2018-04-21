using System;
using System.Globalization;
using System.Text.Utf8;
using Newtonsoft.Json;

namespace DotGrid.Binary2
{
    public static class JsonExtensions
    {
        public static byte[] ToByteArray(this JsonReader reader,int maxSize)
        {
            var buffer = new byte[maxSize];

            var size = ToByteArray(reader, buffer);

            var finalBuffer = new byte[size];
            Buffer.BlockCopy(buffer,0,finalBuffer,0,size);

            return finalBuffer;
        }

        public static unsafe int ToByteArray(this JsonReader reader,byte[] buffer)
        {
            fixed (byte* bufferPtr = &buffer[0])
            {
                var memoryWriter = new MemoryWriter(bufferPtr,buffer.Length);
                var writer = new BinaryFormatWriter();
                
                writer.WriteStartDocument(memoryWriter);
                
                WriteTo(reader,writer);
                
                writer.WriteEndDocument();

                return memoryWriter.Position;
            }
        }
        
        public static void WriteTo(this JsonReader reader, JsonWriter writer)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                     case JsonToken.Boolean:
                         writer.WriteValue((bool)reader.Value);
                         break;
                     case JsonToken.Bytes:
                         var value = (byte[]) reader.Value;
                         writer.WriteValue(value);
                         break;
                     case JsonToken.Comment:
                         writer.WriteComment((string)reader.Value);
                         break;
                     case JsonToken.Date:
                         writer.WriteValue((DateTime)reader.Value);
                         break;
                     case JsonToken.EndArray:
                         writer.WriteEndArray();
                         break;
                     case JsonToken.EndConstructor:
                         writer.WriteEndConstructor();
                         break;
                     case JsonToken.EndObject:
                         writer.WriteEndObject();
                         break;
                     case JsonToken.Float:
                         writer.WriteValue((double)reader.Value);
                         break;
                     case JsonToken.Integer:
                         writer.WriteValue((long)reader.Value);
                         break;
                     case JsonToken.None:
                         break;
                     case JsonToken.Null:
                         writer.WriteNull();
                         break;
                     case JsonToken.PropertyName:
                         writer.WritePropertyName((string)reader.Value);
                         break;
                     case JsonToken.Raw:
                         writer.WriteRaw((string)reader.Value);
                         break;
                     case JsonToken.StartArray:
                         writer.WriteStartArray();
                         break;
                     case JsonToken.StartConstructor:
                         writer.WriteStartConstructor((string)reader.Value);
                          break;
                     case JsonToken.StartObject:
                         writer.WriteStartObject();
                         break;
                     case JsonToken.String:
                         writer.WriteValue((string)reader.Value);
                         break;
                     case JsonToken.Undefined:
                         break;
                }
            }
        }
        
        public static void WriteTo(this JsonReader reader, BinaryFormatWriter writer)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                     case JsonToken.Boolean:
                         writer.WriteValue((bool)reader.Value);
                         break;
                     case JsonToken.Bytes:
                         var value = (byte[]) reader.Value;
                         var span = new Span<byte>(value);
                         writer.WriteValue(span.AsReadOnlySpan());
                         break;
                     case JsonToken.Comment:
                         break;
                     case JsonToken.Date:
                         var utf8SpanDate = new Utf8Span(ToIso8601String((DateTime)reader.Value));
                         writer.WriteValue(in utf8SpanDate);
                         break;
                     case JsonToken.EndArray:
                         writer.WriteEndArray();
                         break;
                     case JsonToken.EndConstructor:
                         break;
                     case JsonToken.EndObject:
                         writer.WriteEndObject();
                         break;
                     case JsonToken.Float:
                         writer.WriteValue((double)reader.Value);
                         break;
                     case JsonToken.Integer:
                         writer.WriteValue((long)reader.Value);
                         break;
                     case JsonToken.None:
                         break;
                     case JsonToken.Null:
                         writer.WriteNull();
                         break;
                     case JsonToken.PropertyName:
                         writer.WritePropertyName((string)reader.Value);
                         break;
                     case JsonToken.Raw:
                         break;
                     case JsonToken.StartArray:
                         writer.WriteStartArray();
                         break;
                     case JsonToken.StartConstructor:
                          break;
                     case JsonToken.StartObject:
                         writer.WriteStartObject();
                         break;
                     case JsonToken.String:
                         var utf8Span = new Utf8Span((string)reader.Value);
                         writer.WriteValue(in utf8Span);
                         break;
                     case JsonToken.Undefined:
                         break;
                }
            }
        }

        private static string ToIso8601String(DateTime dateTime)
        {
            var utc = dateTime.ToUniversalTime();
            var utcString = utc.ToString("s", CultureInfo.InvariantCulture);

            return utcString;
        }
    }
}