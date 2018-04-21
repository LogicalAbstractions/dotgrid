 
     
using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Security;
using System.Transactions;
using DotGrid.Core.Memory;

namespace DotGrid.Storage.Lmdb.Interop
{
    using MdbDbi = System.UInt32;
    using MdbSize = System.UInt64;
    using MdbMode = System.Int32;

    [Flags]
    public enum MdbEnvFlags : uint 
    {
        None = 0x00,
        FixedMap = 0x01,
        NoSubDir = 0x4000,
        NoSync = 0x10000,
        RdOnly = 0x20000,
        NoMetAsync = 0x40000,
        WriteMap = 0x80000,
        MapAsync = 0x100000,
        NoTls = 0x200000,
        NoLock = 0x400000,
        NoRdAhead = 0x800000,
        NoMemInit = 0x1000000
    }   

    [Flags]
    public enum MdbDbiFlags : uint 
    {
        None = 0x00,
        ReverseKey = 0x02,
        DupSort = 0x04,
        IntegerKey = 0x8,
        DupFixed = 0x10,
        IntegerDup = 0x20,
        ReverseDup = 0x40,
        Create = 0x40000
    }

    [Flags]
    public enum MdbCursorPutFlags : uint
    {
        None = 0x00,
        NoOverwrite = 0x10,
        NoDupData = 0x20,
        Current = 0x40,
        Reserve = 0x10000,
        Append = 0x20000,
        AppendDup = 0x40000,
        Multiple = 0x80000
    }

    [Flags]
    public enum MdbPutFlags : uint
    {
        None = 0x00,
        NoOverwrite = 0x10,
        NoDupData = 0x20,
       
        Reserve = 0x10000,
        Append = 0x20000,
        AppendDup = 0x40000,
    }

    [Flags]
    public enum MdbTransactionFlags : uint
    {
        None = 0x00,
        NoSync = 0x10000,
        RdOnly = 0x20000,
        NoMetAsync = 0x40000,
    }
    
    public enum MdbCursorOp
    {
        /// <summary>Position at first key/data item</summary>
        First = 0,
        /// <summary>
        /// <para>Position at first data item of current key.</para>
        /// <para>Only for #MDB_DUPSORT</para>
        /// </summary>
        FirstDup = 1,
        /// <summary>Position at key/data pair. Only for #MDB_DUPSORT</summary>
        GetBoth = 2,
        /// <summary>position at key, nearest data. Only for #MDB_DUPSORT</summary>
        GetBothRange = 3,
        /// <summary>Return key/data at current cursor position</summary>
        GetCurrent = 4,
        /// <summary>
        /// <para>Return key and up to a page of duplicate data items</para>
        /// <para>from current cursor position. Move cursor to prepare</para>
        /// <para>for #MDB_NEXT_MULTIPLE. Only for #MDB_DUPFIXED</para>
        /// </summary>
        GetMultiple = 5,
        /// <summary>Position at last key/data item</summary>
        Last = 6,
        /// <summary>
        /// <para>Position at last data item of current key.</para>
        /// <para>Only for #MDB_DUPSORT</para>
        /// </summary>
        LastDup = 7,
        /// <summary>Position at next data item</summary>
        Next = 8,
        /// <summary>
        /// <para>Position at next data item of current key.</para>
        /// <para>Only for #MDB_DUPSORT</para>
        /// </summary>
        NextDup = 9,
        /// <summary>
        /// <para>Return key and up to a page of duplicate data items</para>
        /// <para>from next cursor position. Move cursor to prepare</para>
        /// <para>for #MDB_NEXT_MULTIPLE. Only for #MDB_DUPFIXED</para>
        /// </summary>
        NextMultiple = 10,
        /// <summary>Position at first data item of next key</summary>
        NextNodup = 11,
        /// <summary>Position at previous data item</summary>
        Prev = 12,
        /// <summary>
        /// <para>Position at previous data item of current key.</para>
        /// <para>Only for #MDB_DUPSORT</para>
        /// </summary>
        PrevDup = 13,
        /// <summary>Position at last data item of previous key</summary>
        PrevNodup = 14,
        /// <summary>Position at specified key</summary>
        Set = 15,
        /// <summary>Position at specified key, return key + data</summary>
        SetKey = 16,
        /// <summary>Position at first key greater than or equal to specified key.</summary>
        SetRange = 17,
        /// <summary>
        /// <para>Position at previous page and return key and up to</para>
        /// <para>a page of duplicate data items. Only for #MDB_DUPFIXED</para>
        /// </summary>
        PrevMultiple = 18
    }
    
    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
    internal unsafe delegate int MdbCmpFunc(MdbVal * a, MdbVal* b);
    
    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
    internal unsafe delegate void MdbRelFunc(MdbVal * item, void * oldptr, void * newptr, void * relctx);
    
    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
    internal unsafe delegate void MdbAssertFunc(MdbEnv * env, [MarshalAs(UnmanagedType.LPStr)] string msg);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
    internal unsafe delegate int MdbMsgFunc([MarshalAs(UnmanagedType.LPStr)] string msg, void * ctx);

    
    internal unsafe struct MdbEnv
    {
        
    }

    internal unsafe struct MdbTxn
    {
        
    }

    internal unsafe struct MdbCursor
    {
        
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly unsafe struct MdbVal
    {
        internal MdbVal(MemorySegment segment)
        {
            Size = segment.Size;
            Data = segment.Pointer;
        }

        [FieldOffset(0)] internal readonly ulong Size;

        [FieldOffset(8)] internal readonly void* Data;

        public static MdbVal* ToPointer(ref MdbVal value)
        {
            if (value.Data == null)
            {
                return null;
            }

            fixed (MdbVal* ptr = &value)
            {
                return ptr;
            }
        }

        public static implicit operator MemorySegment(MdbVal value)
        {
            return value.ToSegment();
        }

        public static implicit operator MdbVal(MemorySegment value)
        {
            return new MdbVal(value);
        }

        internal MemorySegment ToSegment()
        {
            return new MemorySegment((byte*)Data,Size);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal struct MdbStat
    {
        [FieldOffset(0)]
        internal uint PSize;

        [FieldOffset(4)]
        internal uint Depth;

        [FieldOffset(8)]
        internal MdbSize BranchPages;

        [FieldOffset(16)]
        internal MdbSize LeafPages;

        [FieldOffset(24)]
        internal MdbSize OverflowPages;

        [FieldOffset(32)]
        internal MdbSize Entries;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal unsafe struct MdbEnvInfo
    {
        [FieldOffset(0)]
        internal void * MapAddr;

        [FieldOffset(8)]
        internal MdbSize MapSize;

        [FieldOffset(16)]
        internal MdbSize LastPgno;

        [FieldOffset(24)]
        internal MdbSize LastTxnId;

        [FieldOffset(32)]
        internal uint MaxReaders;

        [FieldOffset(36)]
        internal uint NumReaders;
    }
}