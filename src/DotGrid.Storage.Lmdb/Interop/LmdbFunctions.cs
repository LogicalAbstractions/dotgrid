using System.Runtime.InteropServices;

namespace DotGrid.Storage.Lmdb.Interop
{
    using MdbDbi = System.UInt32;
    using MdbSize = System.UInt64;
    using MdbMode = System.Int32;
    
    internal static unsafe class LmdbFunctions
    {
        private const string DllName = "lmdb";
        
        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_version")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string Version(int* major, int* minor, int* patch);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_strerror")]
        internal static extern byte * StrError(int err);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_create")]
        internal static extern int EnvCreate(MdbEnv** env);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_open")]
        internal static extern int EnvOpen(MdbEnv* env, [MarshalAs(UnmanagedType.LPStr)]string path, uint flags, MdbMode mode);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_copy")]
        internal static extern int EnvCopy(MdbEnv* env, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_copyfd")]
        internal static extern int EnvCopyFd(MdbEnv* env, void* fd);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_copy2")]
        internal static extern int EnvCopy2(MdbEnv* env, [MarshalAs(UnmanagedType.LPStr)] string path, uint flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_copyfd2")]
        internal static extern int EnvCopyFd2(MdbEnv* env, void* fd, uint flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_stat")]
        internal static extern int EnvStat(MdbEnv* env, MdbStat* stat);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_info")]
        internal static extern int EnvInfo(MdbEnv* env, MdbEnvInfo* info);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_sync")]
        internal static extern int EnvSync(MdbEnv* env, int force);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_close")]
        internal static extern void EnvClose(MdbEnv* env);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_flags")]
        internal static extern int EnvSetFlags(MdbEnv* env, uint flags, int onoff);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_flags")]
        internal static extern int EnvGetFlags(MdbEnv* env, uint* flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_path")]
        internal static extern int EnvGetPath(MdbEnv* env, sbyte** path);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_fd")]
        internal static extern int EnvGetFd(MdbEnv* env, void* fd);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_mapsize")]
        internal static extern int EnvSetMapSize(MdbEnv* env, MdbSize size);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_maxreaders")]
        internal static extern int EnvSetMaxReaders(MdbEnv* env, uint readers);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_maxreaders")]
        internal static extern int EnvGetMaxReaders(MdbEnv* env, uint* readers);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_maxdbs")]
        internal static extern int EnvSetMaxDbs(MdbEnv* env, MdbDbi dbs);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_maxkeysize")]
        internal static extern int EnvGetMaxKeySize(MdbEnv* env);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_userctx")]
        internal static extern int EnvSetUserCtx(MdbEnv* env, void* ctx);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_get_userctx")]
        internal static extern void* EnvGetUserCtx(MdbEnv* env);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_env_set_assert")]
        internal static extern int EnvSetAssert(MdbEnv* env, [MarshalAs(UnmanagedType.FunctionPtr)]MdbAssertFunc func);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_begin")]
        internal static extern int TxnBegin(MdbEnv* env, MdbTxn* parent, uint flags, MdbTxn** txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_env")]
        internal static extern MdbEnv* TxnEnv(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_id")]
        internal static extern MdbSize TxnId(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_commit")]
        internal static extern int TxnCommit(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_abort")]
        internal static extern void TxnAbort(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_reset")]
        internal static extern int TxnReset(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_txn_renew")]
        internal static extern int TxnRenew(MdbTxn* txn);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_dbi_open")]
        internal static extern int DbiOpen(MdbTxn* txn, [MarshalAs(UnmanagedType.LPStr)]string name, uint flags, MdbDbi* dbi);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_stat")]
        internal static extern int Stat(MdbTxn* txn, MdbDbi dbi, MdbStat* stat);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_dbi_flags")]
        internal static extern int DbiFlags(MdbTxn* txn, MdbDbi dbi, uint* flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_dbi_close")]
        internal static extern void DbiClose(MdbEnv* env, MdbDbi dbi);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_drop")]
        internal static extern int Drop(MdbTxn* txn, MdbDbi dbi, int del);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_set_compare")]
        internal static extern int SetCompare(MdbTxn* txn, MdbDbi dbi,
            [MarshalAs(UnmanagedType.FunctionPtr)] MdbCmpFunc cmpFunc);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_set_dupsort")]
        internal static extern int SetDupSort(MdbTxn* txn, MdbDbi dbi,
            [MarshalAs(UnmanagedType.FunctionPtr)] MdbCmpFunc cmpFunc);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_set_relfunc")]
        internal static extern int SetRelFunc(MdbTxn* txn, MdbDbi dbi,
            [MarshalAs(UnmanagedType.FunctionPtr)] MdbRelFunc relFunc);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_set_relctx")]
        internal static extern int SetRelCtx(MdbTxn* txn, MdbDbi dbi, void* ctx);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_get")]
        internal static extern int Get(MdbTxn* txn, MdbDbi dbi, MdbVal* key, MdbVal* data);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_put")]
        internal static extern int Put(MdbTxn* txn, MdbDbi dbi, MdbVal* key, MdbVal* data, uint flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_del")]
        internal static extern int Del(MdbTxn* txn, MdbDbi dbi, MdbVal* key, MdbVal* data);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_open")]
        internal static extern int CursorOpen(MdbTxn* txn, MdbDbi dbi, MdbCursor** cursor);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_close")]
        internal static extern void CursorClose(MdbCursor* cursor);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_renew")]
        internal static extern int CursorRenew(MdbTxn* txn, MdbCursor* cursor);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_txn")]
        internal static extern MdbTxn CursorTxn(MdbCursor* cursor);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_dbi")]
        internal static extern MdbDbi CursorDbi(MdbCursor* cursor);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_get")]
        internal static extern int CursorGet(MdbCursor* cursor, MdbVal* key, MdbVal* data, [MarshalAs(UnmanagedType.I4)]MdbCursorOp cursorOp);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_put")]
        internal static extern int CursorPut(MdbCursor* cursor, MdbVal* key, MdbVal* data, uint flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_del")]
        internal static extern int CursorDel(MdbCursor* cursor, uint flags);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cursor_count")]
        internal static extern int CursorCount(MdbCursor* cursor, MdbSize* countp);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_cmp")]
        internal static extern int Cmp(MdbTxn* txn, MdbDbi dbi, MdbVal* mdbVal, MdbVal* b);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_dcmp")]
        internal static extern int Dcmp(MdbTxn* txn, MdbDbi dbi, MdbVal* mdbVal, MdbVal* b);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_reader_list")]
        internal static extern int ReaderList(MdbEnv* env, [MarshalAs(UnmanagedType.FunctionPtr)]MdbMsgFunc msgFunc, void* ctx);

        [DllImport(DllName,CallingConvention = CallingConvention.Cdecl,EntryPoint = "mdb_reader_check")]
        internal static extern int ReaderCheck(MdbEnv* env, int* dead);
    }
}