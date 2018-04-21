using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

namespace DotGrid.Benchmarks.Memory
{
    [CoreJob()]
    [RPlotExporter,RankColumn]
    public class AccessStrategies
    {
        private long[] data;


        public int Size { get; } = 1000000 * 100;

        [GlobalSetup]
        public void Setup()
        {
            data = new long[Size];

            var random = new Random();

            for (int i = 0; i < Size; ++i)
            {
                data[i] = random.Next(100);
            }
        }
        
        [Benchmark(Baseline = true)]
        public unsafe void Pointer()
        {
            long sum = 0L;
            
            fixed (long* ptr = &data[0])
            {
                for (int i = 0; i < Size; ++i)
                {
                    ptr[i] = ptr[i] + 4;
                }

                for (int i = 0; i < Size; ++i)
                {
                    sum += ptr[i];
                }
            }
        }

        [Benchmark]
        public unsafe void Span()
        {
            long sum = 0L;
            
            var span = new Span<long>(data);
            
            for (int i = 0; i < Size; ++i)
            {
                span[i] = span[i] + 4;
            }

            for (int i = 0; i < Size; ++i)
            {
                sum += span[i];
            }          
        }

        [Benchmark]
        public unsafe void Array()
        {
            long sum = 0L;
                      
            for (int i = 0; i < Size; ++i)
            {
                data[i] = data[i] + 4;
            }

            for (int i = 0; i < Size; ++i)
            {
                sum += data[i];
            }          
        }
    }
}