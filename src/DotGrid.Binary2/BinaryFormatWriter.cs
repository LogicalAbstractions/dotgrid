using System;
using System.Collections.Generic;
using System.Text.Utf8;
using DotGrid.Core.Pooling;

namespace DotGrid.Binary2
{
    public sealed class BinaryFormatWriter
    {
        private readonly ContainerPool       containerPool = new ContainerPool(16);
        private readonly Stack<Container>    containerStack = new Stack<Container>();
        private readonly PropertyNamesWriter propertyNamesWriter;

        private Container                    currentContainer;
        private int                          lastPropertyId;
        private MemoryWriter                 writer;

        public BinaryFormatWriter(IReadOnlyDictionary<string,int> propertyIds = null)
        {
            this.propertyNamesWriter =
                propertyIds != null ? new PropertyNamesWriter(propertyIds) : new PropertyNamesWriter();
        }

        public void WriteStartDocument(MemoryWriter writer)
        {
            this.writer = writer;
            
            // Reset all data structures
            containerStack.Clear();
            propertyNamesWriter.Reset();
            lastPropertyId = -1;

            currentContainer = PushContainer(ValueType.Document);
        }

        public void WriteEndDocument()
        {
            var isSelfContained = propertyNamesWriter.WritePropertyNames(writer);
        }
        
        public void WriteStartObject()
        {
            currentContainer = PushContainer(ValueType.Object);
            lastPropertyId = -1;
        }

        public void WriteEndObject()
        {
            WriteEndContainer(ValueType.Object);
        }

        public void WriteStartArray()
        {
            currentContainer = PushContainer(ValueType.Array);
            lastPropertyId = -1;
        }

        public void WriteEndArray()
        {
           WriteEndContainer(ValueType.Array);
        }
        
        public void WritePropertyName(string propertyName)
        {
            lastPropertyId = propertyNamesWriter.GetOrAdd(propertyName);
        }

        public void WriteNull()
        {
            PushContainerEntry(ValueType.Null);
        }
        
        public void WriteValue(bool value)
        {
            PushContainerEntry(ValueType.Boolean);
            writer.WriteBoolean(value);
        }

        public void WriteValue(byte value)
        {
            PushContainerEntry(ValueType.Byte);
            writer.WriteByte(value);
        }

        public void WriteValue(short value)
        {
            PushContainerEntry(ValueType.Short);
            writer.WriteShort(value);
        }

        public void WriteValue(int value)
        {
            PushContainerEntry(ValueType.Int);
            writer.WriteSignedVarInt(value);
        }

        public void WriteValue(long value)
        {
            PushContainerEntry(ValueType.Long);
            writer.WriteSignedVarLong(value);
        }

        public void WriteValue(float value)
        {
            PushContainerEntry(ValueType.Float);
            writer.WriteFloat(value);
        }

        public void WriteValue(double value)
        {
            PushContainerEntry(ValueType.Double);
            writer.WriteDouble(value);
        }

        public void WriteValue(in Utf8Span value)
        {
            PushContainerEntry(ValueType.String);
            writer.WriteUtf8Span(in value);
        }

        public void WriteValue(in ReadOnlySpan<byte> value)
        {
            PushContainerEntry(ValueType.Blob);
            
            writer.WriteUnsignedVarInt((uint)value.Length);
            writer.WriteBytes(in value);
        }

        private void WriteEndContainer(ValueType valueType)
        {
            var metadataStartPosition = writer.Position;
            
            currentContainer.WriteFooter(writer);

            if (currentContainer.LastPropertyId > -1)
            {
                lastPropertyId = currentContainer.LastPropertyId;
            }

            if (TryPopContainer(out currentContainer))
            {
                PushContainerEntry(valueType, metadataStartPosition);
            }
        }

        private void PushContainerEntry(ValueType valueType,int position = -1)
        {
            var realPosition = position < 0 ? writer.Position : position;
            
            switch (currentContainer.ContainerType)
            {
               case ValueType.Array:
                   currentContainer.PushArrayElement(realPosition,valueType,lastPropertyId);
                   return;
               case ValueType.Object:
                   currentContainer.PushObjectProperty(realPosition,lastPropertyId,valueType,lastPropertyId);
                   return;
               case ValueType.Document:
                   currentContainer.PushArrayElement(realPosition,valueType,lastPropertyId);
                   return;
            }
        }

        private Container PushContainer(ValueType valueType)
        {
            var container = containerPool.Aquire(valueType,writer.Position,lastPropertyId);
            containerStack.Push(container);
            return container;
        }

        private bool TryPopContainer(out Container container)
        {
            if (containerStack.Count > 0)
            {
                var lastContainer = containerStack.Pop();
                
                containerPool.Release(lastContainer);

                if (containerStack.Count > 0)
                {
                    container = containerStack.Peek();
                    return true;
                }
            }

            container = null;
            return false;
        }
    }
}