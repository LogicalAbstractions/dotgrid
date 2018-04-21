using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using DotGrid.Core.Memory;
using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    using MdbSize = System.UInt64;
    using MdbDbi = System.UInt32;

    public enum LmdbCursorEnumerationDirection
    {
        Ascending,
        Descending,
    }

    public readonly unsafe struct LmdbCursorEnumerable
    {
        internal readonly LmdbCursorEnumerationDirection Direction;

        internal readonly MemorySegment Min;

        internal readonly bool MinInclusive;

        internal readonly MemorySegment Max;

        internal readonly bool MaxInclusive;

        internal readonly MdbCursor* Cursor;

        internal readonly MdbTxn* Txn;

        internal readonly MdbDbi Dbi;

        internal LmdbCursorEnumerable(LmdbCursorEnumerationDirection direction, MemorySegment min, bool minInclusive, MemorySegment max, bool maxInclusive, MdbCursor* cursor, MdbTxn* txn, uint dbi)
        {
            Direction = direction;
            Min = min;
            MinInclusive = minInclusive;
            Max = max;
            MaxInclusive = maxInclusive;
            Cursor = cursor;
            Txn = txn;
            Dbi = dbi;
        }

        public LmdbCursorEnumerator GetEnumerator()
        {
            return new LmdbCursorEnumerator(this);
        }
    }
    
    public unsafe struct LmdbCursorEnumerator
    {
        private readonly LmdbCursorEnumerable source;  
        private KeyValuePair<MemorySegment, MemorySegment> current;
        private bool isFirst;

        internal LmdbCursorEnumerator(LmdbCursorEnumerable source)
        {
            this.source = source;
            this.isFirst = true;
        }

        public KeyValuePair<MemorySegment, MemorySegment> Current => current;

        public bool MoveNext()
        {
            if (source.Direction == LmdbCursorEnumerationDirection.Ascending)
            {
                if (MoveNextAscending(out current))
                {
                    isFirst = false;
                    return true;
                }
            }
            else
            {
                if (MoveNextDescending(out current))
                {
                    isFirst = false;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            isFirst = true;
        }
        
        private bool MoveNextDescending(out KeyValuePair<MemorySegment,MemorySegment> entry)
        {
            if (isFirst)
            {
                if (source.Max.IsNotNull)
                {
                    // We have a lower bound:
                    if (TryMoveToKey(in source.Max) && TryGetValue(MdbCursorOp.GetCurrent,out entry))
                    {
                        if (!IsBelowMax(in entry))
                        {
                            // Move one further
                            if (TryGetValue(MdbCursorOp.Prev, out entry))
                            {
                                return IsAboveMin(in entry);
                            }
                        }

                        return IsAboveMin(in entry);
                    }

                    return false;
                }

                if (TryGetValue(MdbCursorOp.Last, out entry))
                {
                    return IsAboveMin(in entry);
                }

                return false;
            }


            if (TryGetValue(MdbCursorOp.Prev, out entry))
            {
                return IsAboveMin(in entry);
            }

            return false;
        }

        private bool MoveNextAscending(out KeyValuePair<MemorySegment,MemorySegment> entry)
        {
            if (isFirst)
            {
                if (source.Min.IsNotNull)
                {
                    // We have a lower bound:
                    if (TryMoveToKey(in source.Min) && TryGetValue(MdbCursorOp.GetCurrent,out entry))
                    {
                        if (!IsAboveMin(in entry))
                        {
                            // Move one further
                            if (TryGetValue(MdbCursorOp.Next, out entry))
                            {
                                return IsBelowMax(in entry);
                            }
                        }

                        return IsBelowMax(in entry);
                    }

                    return false;
                }

                if (TryGetValue(MdbCursorOp.First, out entry))
                {
                    return IsBelowMax(in entry);
                }

                return false;
            }


            if (TryGetValue(MdbCursorOp.Next, out entry))
            {
                return IsBelowMax(in entry);
            }

            return false;
        }

        private bool IsAboveMin(in KeyValuePair<MemorySegment, MemorySegment> entry)
        {
            if (source.Min.IsNotNull)
            {
                MdbVal foundKey = entry.Key;
                MdbVal compKey = source.Min;

                var comparisonValue = source.MinInclusive ? 0 : 1;

                return LmdbFunctions.Cmp(source.Txn, source.Dbi, &foundKey, &compKey) >= comparisonValue;
            }

            return true;
        }

        private bool IsBelowMax(in KeyValuePair<MemorySegment, MemorySegment> entry)
        {
            if (source.Max.IsNotNull)
            {
                MdbVal foundKey = entry.Key;
                MdbVal compKey = source.Max;

                var comparisonValue = source.MaxInclusive ? 0 : -1;

                return LmdbFunctions.Cmp(source.Txn, source.Dbi, &foundKey, &compKey) <= comparisonValue;
            }

            return true;
        }

        private bool TryMoveToKey(in MemorySegment key)
        {
            MdbVal mdbKey = key;

            var errorCode = LmdbFunctions.CursorGet(source.Cursor, &mdbKey, null, MdbCursorOp.SetRange);

            if (errorCode == (int) LmdbErrorCode.NotFound)
            {
                return false;
            }
            
            LmdbException.ThrowOnError(errorCode);

            return true;
        }

        private bool TryGetValue(MdbCursorOp operation,out KeyValuePair<MemorySegment,MemorySegment> entry)
        {
            MdbVal mdbKey = new MdbVal();
            MdbVal mdbValue = new MdbVal();

            var errorCode = LmdbFunctions.CursorGet(source.Cursor, &mdbKey,&mdbValue,operation);

            if (errorCode == (int) LmdbErrorCode.NotFound)
            {
                entry = new KeyValuePair<MemorySegment, MemorySegment>(MemorySegment.Null,MemorySegment.Null);        
                return false;
            }

            LmdbException.ThrowOnError(errorCode);

            entry = new KeyValuePair<MemorySegment, MemorySegment>(mdbKey,mdbValue);
            return true;
        }
    }
    
    public sealed unsafe class LmdbCursor : IDisposable
    {
        private MdbCursor* cursor;
        private MdbTxn* txn;
        private MdbDbi dbi;  
        
        internal LmdbCursor(MdbCursor* cursor, MdbTxn* txn, MdbDbi dbi)
        {
            this.cursor = cursor;
            this.txn = txn;
            this.dbi = dbi;
        }

        public LmdbCursorEnumerable GetEnumerable(in MemorySegment min, in MemorySegment max, bool minInclusive = true,
            bool maxInclusive = false,
            LmdbCursorEnumerationDirection direction = LmdbCursorEnumerationDirection.Ascending)
        {
            return new LmdbCursorEnumerable(direction,min,minInclusive,max,maxInclusive,cursor,txn,dbi);
        }
        
        public LmdbCursorEnumerator GetEnumerator()
        {
            return new LmdbCursorEnumerator(
                new LmdbCursorEnumerable(
                    LmdbCursorEnumerationDirection.Ascending,
                    MemorySegment.Null,false,
                    MemorySegment.Null,false,
                    cursor,txn,dbi));
        }

        public void Dispose()
        {
            LmdbFunctions.CursorClose(cursor);
        }
    }
}