using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using Xunit;
using Xunit.Abstractions;

namespace DotGrid.Binary2.Tests
{
    public class MemoryRoundTripTests
    {
        private readonly ITestOutputHelper outputHelper;

        public MemoryRoundTripTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ByteShouldRoundTrip()
        {
            TestRoundTrip(new byte[]{byte.MinValue,byte.MaxValue,100},(w,v) => w.WriteByte(v),r => r.ReadByte());
        }

        [Fact]
        public void ShortShouldRoundTrip()
        {
            TestRoundTrip(new short[] {short.MinValue,short.MaxValue,100},(w,v) => w.WriteShort(v),r => r.ReadShort());
        }
        
        [Fact]
        public void IntShouldRoundTrip()
        {
            TestRoundTrip(new int[] {int.MinValue,int.MaxValue,100},(w,v) => w.WriteInt(v),r => r.ReadInt());
        }
        
        [Fact]
        public void LongShouldRoundTrip()
        {
            TestRoundTrip(new long[] {long.MinValue,long.MaxValue,100},(w,v) => w.WriteLong(v),r => r.ReadLong());
        }
        
        [Fact]
        public void FloatShouldRoundTrip()
        {
            TestRoundTrip(new float[] {float.MinValue,float.MaxValue,100.0f},(w,v) => w.WriteFloat(v),r => r.ReadFloat());
        }
        
        [Fact]
        public void DoubleShouldRoundTrip()
        {
            TestRoundTrip(new double[] {double.MinValue,double.MaxValue,100.0},(w,v) => w.WriteDouble(v),r => r.ReadDouble());
        }
        
        [Fact]
        public void BooleanShouldRoundTrip()
        {
            TestRoundTrip(new bool[] {true,false},(w,v) => w.WriteBoolean(v),r => r.ReadBoolean());
        }
        
        [Fact]
        public void VarIntShouldRoundTrip()
        {
            TestRoundTrip(new int[] {int.MinValue,int.MaxValue,100,0,10},(w,v) => w.WriteSignedVarInt(v),r => r.ReadSignedVarInt());
        }
        
        [Fact]
        public void VarUintShouldRoundTrip()
        {
            TestRoundTrip(new uint[] {uint.MinValue,uint.MaxValue,100,0,10},(w,v) => w.WriteUnsignedVarInt(v),r => r.ReadUnsignedVarInt());
        }
       
        [Fact]
        public void VarLongShouldRoundTrip()
        {
            TestRoundTrip(new long[] {long.MinValue,long.MaxValue,100L,0L,10L},(w,v) => w.WriteSignedVarLong(v),r => r.ReadSignedVarLong());
        }
        
        [Fact]
        public void VarUlongShouldRoundTrip()
        {
            TestRoundTrip(new ulong[] {ulong.MinValue,ulong.MaxValue,100L,0L,10L},(w,v) => w.WriteUnsignedVarLong(v),r => r.ReadUnsignedVarLong());
        }
        
        private unsafe void TestRoundTrip<T>(IEnumerable<T> values, Action<MemoryWriter, T> writeAction, Func<MemoryReader, T> readAction)
        {
            var buffer = new byte[1024];

            fixed (byte* bufferPtr = &buffer[0])
            {
                var reader = new MemoryReader(bufferPtr,buffer.Length);
                var writer = new MemoryWriter(bufferPtr,buffer.Length);

                foreach (var value in values)
                {
                    reader.Seek(0);
                    writer.Seek(0);
                    
                    writeAction.Invoke(writer,value);

                    outputHelper.WriteLine($"Type {typeof(T).Name}, value: {value} = {writer.Position} bytes");
                    
                    var readValue = readAction.Invoke(reader);

                    Assert.Equal(value, readValue);
                }
            }
        }
    }
}