using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using DotGrid.Core.Memory;
using DotGrid.Storage.Lmdb.Interop;
using DotGrid.Storage.Lmdb.Wrapper;

namespace DotGrid.Playground
{
    public static unsafe class EntryPoint
    {
        public static int Main(string[] arguments)
        {
            using (var env = new LmdbEnvironment(new LmdbEnvironmentOptions("database")))
            {
                //env.CopyTo("database2",true);
                env.Sync(true);                
                
                Console.WriteLine(env.Statistics);
                Console.WriteLine(env.EnvironmentInfo);
                Console.WriteLine(env.Flags);
                Console.WriteLine(env.MaxKeySize);

                using (var transaction = env.CreateTransaction())
                {
                    using (var database = transaction.GetOrCreateDatabase("System"))
                    {
                        transaction.ClearDatabase(database);

                        foreach (var number in Enumerable.Range(0, 10).Select(i => i.ToString("00")))
                        {
                            number.AsMemory( s =>
                            {
                                Console.WriteLine("Writing: " + number);
                                transaction.PutValue(database,in s,in s);
                            });
                        }

                        transaction.Commit();
                    }
                }

                using (var transaction = env.CreateTransaction())
                {
                    using (var database = transaction.GetOrCreateDatabase("System"))
                    {
                        using (var cursor = transaction.CreateCursor(database))
                        {
                            "04".AsMemory( s =>
                            {
                                foreach (var entry in cursor.GetEnumerable(in MemorySegment.Null,in s,direction:LmdbCursorEnumerationDirection.Descending))
                                {
                                    Console.WriteLine($"{DecodeString(entry.Key)}: {DecodeString(entry.Value)}");
                                }
                            });
                        }
                    }
                }
            }
          
            return 0;
        }

        private static string DecodeString(in MemorySegment segment)
        {
            return Encoding.UTF8.GetString(segment.Pointer, (int) segment.Size);
        }
    }
}