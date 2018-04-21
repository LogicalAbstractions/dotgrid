using System;
using BenchmarkDotNet.Running;
using DotGrid.Benchmarks.Binary;
using DotGrid.Benchmarks.Memory;

namespace DotGrid.Benchmarks
{
    public static class EntryPoint
    {
        public static int Main(string[] arguments)
        {
            //RunUnderProfiler<BinaryFormatBenchmarks>(b => b.FullBinaryEncode() );
            
            
            var benchmark = new BinaryFormatBenchmarks();
            benchmark.RandomJTokenAccess();
            benchmark.RandomBinaryAccess();
            benchmark.RandomJsonTextAccess();
            
            var switcher = new BenchmarkSwitcher(new[] {
                typeof(AccessStrategies),
                typeof(BinaryFormatBenchmarks),
               
            });
           
            switcher.Run(arguments);
           
            /*var benchmark = new DecodingBenchmarks();

            while (true)
            {
                benchmark.BinaryFormatDecoding();
            }*/
            
            return 0;
        }

        private static void RunUnderProfiler<T>(Action<T> action)
            where T : new()
        {
            var codeToProfile = new T();
            
            while (true)
            {
                action.Invoke(codeToProfile);
            }
        }
    }
}