using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Utf8;
using DotGrid.Core.Pooling;
using Newtonsoft.Json.Schema;

namespace DotGrid.Binary
{
    public sealed class BinaryFormatWritingContext
    {
        private struct ValueEntryComparer : IComparer<ValueEntry>
        {
            public int Compare(ValueEntry x, ValueEntry y)
            {
                if (x.Id < y.Id) return -1;
                if (x.Id > y.Id) return 1;
                return 0;
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct ValueEntry : IComparable<ValueEntry>
        {
            internal BinaryFormatValueType  ValueType;
            internal int             Position;
            internal int             Id;
            internal int             ReservedForAlignment;
            
            public int CompareTo(ValueEntry other)
            {
                if (Id < other.Id) return -1;
                if (Id > other.Id) return 1;
                return 0;
            }
        }
        
        internal readonly struct OperationStackEntry
        {
            internal readonly BinaryFormatValueType        ValueType;
            internal readonly int                          Position;       
            internal readonly List<ValueEntry>             Entries;
            internal readonly string                       PropertyName;

            public OperationStackEntry(BinaryFormatValueType valueType, int position, List<ValueEntry> entries,string propertyName)
            {
                ValueType = valueType;
                Position = position;
                Entries = entries;
                PropertyName = propertyName;
            }

            internal void SortEntries()
            {
                BubbleSort();
            }

            private void BubbleSort()
            {
                var n = Entries.Count;
                do
                {
                    int newn = 0;

                    for (int i = 1; i < n; ++i)
                    {
                        var a1 = Entries[i - 1];
                        var a2 = Entries[i];

                        if (a1.Id > a2.Id)
                        {
                            // Swap
                            Entries[i] = a1;
                            Entries[i - 1] = a2;
                            newn = i;
                        }
                    }

                    n = newn;
                } while (n > 0);
            }
        }

        private readonly Stack<OperationStackEntry> operationStack = new Stack<OperationStackEntry>();
        private readonly IObjectPool<List<ValueEntry>> valueEntriesPool = ObjectPools.SingleThreaded(() => new List<ValueEntry>(),v => v.Clear());
        private readonly Dictionary<string, int> propertyIds = new Dictionary<string, int>();
        private readonly List<string> propertyNames = new List<string>();
        private readonly List<int> propertyOffsets = new List<int>();
        
        internal void Reset()
        {
            operationStack.Clear();
            propertyIds.Clear();
            propertyNames.Clear();
        }
        
        internal List<string>      PropertyNames => propertyNames;

        internal List<int>             PropertyOffsets => propertyOffsets;

        internal int GetPropertyId(string propertyName)
        {
            if (propertyIds.TryGetValue(propertyName, out var id))
            {
                return id;
            }

            id = propertyIds.Count;
            propertyIds.Add(propertyName,id);
            propertyNames.Add(propertyName);
            propertyOffsets.Add(0);

            return id;
        }

        internal bool TryPopOperation(out OperationStackEntry operation)
        {
            if (operationStack.Count > 0)
            {
                var oldOperation = operationStack.Pop();

                valueEntriesPool.Release(oldOperation.Entries);
                
                if (operationStack.Count > 0)
                {
                    operation = operationStack.Peek();
                }
                else
                {
                    operation = new OperationStackEntry();
                }
                
                return true;
            }

            operation = new OperationStackEntry();
            return false;
        }

        internal OperationStackEntry PushOperation(BinaryFormatValueType operationType, int position,string propertyName)
        {
            var entry = new OperationStackEntry(operationType, position, valueEntriesPool.Aquire(), propertyName);
       
            operationStack.Push(entry);

            return entry;
        }
    }
}