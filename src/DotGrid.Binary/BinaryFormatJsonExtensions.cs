using System;
using System.Globalization;
using System.Text.Utf8;
using Newtonsoft.Json;

namespace DotGrid.Binary
{
    public static class BinaryFormatJsonExtensions
    {
         public static byte[] ToByteArray(this JsonReader reader,int maxSize,BinaryFormatWritingContext context = null)
        {
            var buffer = new byte[maxSize];

            var size = ToByteArray(reader, buffer, context);

            var finalBuffer = new byte[size];
            Buffer.BlockCopy(buffer,0,finalBuffer,0,size);

            return finalBuffer;
        }

        public static unsafe int ToByteArray(this JsonReader reader,byte[] buffer,BinaryFormatWritingContext context = null)
        {
            var finalContext = context ?? new BinaryFormatWritingContext();
             
            fixed (byte* bufferPtr = &buffer[0])
            {
                var memoryWriter = new BinaryFormatMemoryWriter(bufferPtr, 0, buffer.Length);
                var writer = new BinaryFormatWriter(finalContext,memoryWriter);
                
                writer.WriteStartDocument();
                
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
                         writer.WriteValue(value,0,value.Length);
                         break;
                     case JsonToken.Comment:
                         break;
                     case JsonToken.Date:
                         writer.WriteValue(new Utf8String(ToIso8601String((DateTime)reader.Value)));
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

                         if (reader.Value == null)
                         {
                             writer.WriteValue((Utf8String)null);
                         }
                         else
                         {
                             writer.WriteValue(new Utf8String((string) reader.Value));
                         }

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
        
        public static void WriteTo(this BinaryFormatReader reader, JsonWriter writer)
        {
            WriteContainer(reader,writer);
        }

        private static void WriteContainer(this BinaryFormatReader reader, JsonWriter writer)
        {
            if (reader.ContainerType == BinaryFormatValueType.Array)
            {
                writer.WriteStartArray();
            }

            if (reader.ContainerType == BinaryFormatValueType.Object)
            {
                writer.WriteStartObject();
            }
            
            for (var i = 0; i < reader.ElementCount; ++i)
            {
                var idOrIndex = reader.ContainerType == BinaryFormatValueType.Object
                    ? reader.GetPropertyId(i)
                    : i;

                var valueType = reader.GetValueType(idOrIndex);
                
                WriteValue(reader,writer,valueType,idOrIndex);
            }
            
            if (reader.ContainerType == BinaryFormatValueType.Array)
            {
                writer.WriteEndArray();
            }

            if (reader.ContainerType == BinaryFormatValueType.Object)
            {
                writer.WriteEndObject();
            }
        }

        private static void WriteValue(this BinaryFormatReader reader, JsonWriter writer,BinaryFormatValueType valueType,int idOrIndex)
        {
            if (reader.ContainerType == BinaryFormatValueType.Object)
            {        
                if (reader.TryGetPropertyName(idOrIndex, out var propertyName))
                {
                    writer.WritePropertyName(propertyName.ToString());
                }
                else
                {
                    throw new BinaryFormatException($"Unable to find property name for property id: {idOrIndex}");
                }
            }
            
            switch (valueType)
            {
               case BinaryFormatValueType.Null:
                    writer.WriteNull();
                    break;
               case BinaryFormatValueType.Boolean:
                   writer.WriteValue(reader.GetBooleanValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Byte:
                   writer.WriteValue(reader.GetByteValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Short:
                   writer.WriteValue(reader.GetShortValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Int:
                   writer.WriteValue(reader.GetIntValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Long:
                   writer.WriteValue(reader.GetLongValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Float:
                   writer.WriteValue(reader.GetFloatValue(idOrIndex));
                   break;
               case BinaryFormatValueType.Double:
                   writer.WriteValue(reader.GetDoubleValue(idOrIndex));
                   break;
               case BinaryFormatValueType.String:
                   writer.WriteValue(reader.GetStringValue(idOrIndex).ToString());
                   break;
               case BinaryFormatValueType.Blob:
                   throw new BinaryFormatException("Blob type not supported by json");
               case BinaryFormatValueType.Array:

                   reader.Down(idOrIndex);
                   
                   WriteContainer(reader,writer);
                   
                   reader.Up();
                  
                   break;
               case BinaryFormatValueType.Object:

                   reader.Down(idOrIndex);
                   
                   WriteContainer(reader,writer);
                   
                   reader.Up();
                   
                   break;
            }
        }
    }
}