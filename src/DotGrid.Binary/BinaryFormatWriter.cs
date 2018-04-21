using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Utf8;

namespace DotGrid.Binary
{
    public sealed class BinaryFormatWriter
    {
        private readonly BinaryFormatWritingContext context;
        private readonly BinaryFormatMemoryWriter writer;
   
        private BinaryFormatWritingContext.OperationStackEntry currentOperation;
        private string currentPropertyName;

        public BinaryFormatWriter(BinaryFormatWritingContext context, BinaryFormatMemoryWriter writer)
        {
            this.context = context;
            this.writer = writer;
        }

        public void WriteStartDocument()
        {
            // Reset the context
            context.Reset();
            currentOperation = context.PushOperation(BinaryFormatValueType.Document, writer.Position, null);
        }

        public void WriteEndDocument()
        {
            WriteOperationFooter();
        }

        public void WriteStartObject()
        {
            currentOperation = context.PushOperation(BinaryFormatValueType.Object, writer.Position, currentPropertyName);
            currentPropertyName = null;
        }

        public void WriteEndObject()
        {
            var metadataStartPosition = writer.Position;

            // Write out all the value footer information
            WriteOperationFooter();

            if (currentOperation.PropertyName != null)
            {
                currentPropertyName = currentOperation.PropertyName;
            }

            if (context.TryPopOperation(out currentOperation))
            {
                PushValueOffset(BinaryFormatValueType.Object, metadataStartPosition);
            }
        }

        public void WriteStartArray()
        {
            currentOperation = context.PushOperation(BinaryFormatValueType.Array, writer.Position, currentPropertyName);
            currentPropertyName = null;
        }

        public void WriteEndArray()
        {
            var metadataStartPosition = writer.Position;

            WriteOperationFooter();

            if (currentOperation.PropertyName != null)
            {
                currentPropertyName = currentOperation.PropertyName;
            }

            if (context.TryPopOperation(out currentOperation))
            {
                PushValueOffset(BinaryFormatValueType.Array, metadataStartPosition);
            }
        }
        
        public void WritePropertyName(string propertyName)
        {
            this.currentPropertyName = propertyName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            PushValueOffset(BinaryFormatValueType.Null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool value)
        {
            PushValueOffset(BinaryFormatValueType.Boolean);

            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            PushValueOffset(BinaryFormatValueType.Byte);

            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            PushValueOffset(BinaryFormatValueType.Short);

            //WriteLongInternal(value);

            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value)
        {
            PushValueOffset(BinaryFormatValueType.Int);
            //WriteLongInternal(value);

            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value)
        {
            PushValueOffset(BinaryFormatValueType.Long);
            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value)
        {
            PushValueOffset(BinaryFormatValueType.Float);

            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value)
        {
            PushValueOffset(BinaryFormatValueType.Double);
            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value)
        {
            WriteValue(new Utf8String(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Utf8String value)
        {
            var span = value.Span;
            WriteValue(in span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(in Utf8Span value)
        {
            PushValueOffset(BinaryFormatValueType.String);
            WriteStringInternal(in value);          
        }

        public unsafe void WriteValue(byte[] sourceArray, int offset, int count)
        {
            if (sourceArray == null)
            {
                WriteNull();
            }
            else
            {
                fixed (byte* sourcePtr = &sourceArray[0])
                {
                    WriteValue(sourcePtr, offset, count);
                }
            }
        }

        public unsafe void WriteValue(byte* sourcePtr, int offset, int count)
        {
            if (sourcePtr == null)
            {
                WriteNull();
            }
            else
            {
                PushValueOffset(BinaryFormatValueType.Blob);

                writer.Write(count);
                writer.Write(sourcePtr, offset, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushValueOffset(BinaryFormatValueType valueType, int position = -1)
        {
            currentOperation.Entries.Add(new BinaryFormatWritingContext.ValueEntry()
            {
                Id = currentOperation.ValueType == BinaryFormatValueType.Object
                    ? context.GetPropertyId(currentPropertyName)
                    : 0,
                Position = position < 0 ? writer.Position : position,
                ValueType = valueType,
                ReservedForAlignment = 0
            });
        }

        private void WriteOperationFooter()
        {
            var currentPosition = writer.Position;

            var entries = currentOperation.Entries;

            switch (currentOperation.ValueType)
            {
                case BinaryFormatValueType.Array:
                    WriteArrayFooter(entries,currentPosition);
                    break;
                case BinaryFormatValueType.Object:
                    WriteObjectFooter(entries,currentPosition);
                    break;
                case BinaryFormatValueType.Document:
                    WriteDocumentFooter();
                    break; 
            }
        }

        private void WriteDocumentFooter()
        {
            var propertyNames = context.PropertyNames;
            var propertyOffsets = context.PropertyOffsets;

            for (var i = 0; i < propertyNames.Count; ++i)
            {
                var propertyName = propertyNames[i];

                propertyOffsets[i] = writer.Position;
                var propertyNameSpan = new Utf8Span(propertyName);
                WriteStringInternal(in propertyNameSpan);
            }

            var propertyOffsetStart = writer.Position;

            for (var i = 0; i < propertyNames.Count; ++i)
            {
                var offset = ((propertyOffsetStart - propertyOffsets[i]));
                writer.Write(offset);
            }

            var rootEntry = currentOperation.Entries[0];

            writer.Write(((writer.Position - rootEntry.Position)));
            writer.Write(((writer.Position - propertyOffsetStart)));

            writer.Write((int) rootEntry.ValueType);
            writer.Write((propertyNames.Count));
        }

        private void WriteArrayFooter(List<BinaryFormatWritingContext.ValueEntry> entries, int currentPosition)
        {
            writer.Write((currentOperation.Entries.Count));

            for (var i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];

                var offset = ((currentPosition - entry.Position));

                writer.Write(offset);

                writer.Write((int) entry.ValueType);
            }
        }

        private void WriteObjectFooter(List<BinaryFormatWritingContext.ValueEntry> entries, int currentPosition)
        {
            writer.Write(currentOperation.Entries.Count);

            currentOperation.SortEntries();
            // Write offsets:
            for (var i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];

                var offset = (currentPosition - entry.Position);

                writer.Write(offset);

                writer.Write(entry.Id);

                writer.Write((int) entry.ValueType);
            }
        }

        private void WriteStringInternal(in Utf8Span value)
        {
            var bytes = value.Bytes;
            
            writer.Write(bytes.Length);
            writer.Write(in bytes);
        }
    }
}