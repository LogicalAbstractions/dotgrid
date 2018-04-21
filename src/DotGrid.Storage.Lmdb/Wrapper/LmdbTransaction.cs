using System;
using DotGrid.Core.Memory;
using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    using MdbSize = System.UInt64;
    using MdbDbi = System.UInt32;
    
    public sealed unsafe class LmdbTransaction : IDisposable
    {
        private readonly LmdbEnvironment environment;
        private readonly MdbTxn* txn;
        private readonly MdbTransactionFlags flags;

        private MdbSize id = UInt64.MaxValue;
        private bool wasCommitted;
        
        internal LmdbTransaction(LmdbEnvironment environment, MdbTxn* txn, MdbTransactionFlags flags)
        {
            this.environment = environment;
            this.txn = txn;
            this.flags = flags;
        }

        internal MdbTxn* Txn => txn;

        public ulong Id
        {
            get
            {
                if (id == UInt64.MaxValue)
                {
                    id = LmdbFunctions.TxnId(txn);
                }

                return id;
            }
        }

        public void Commit()
        {
            if (wasCommitted)
            {
                throw new LmdbException("Transaction already committed");
            }
            
            LmdbException.ThrowOnError(LmdbFunctions.TxnCommit(txn));
            wasCommitted = true;
        }

        public void ResetRead(bool startNew = true)
        {
            if (flags.HasFlag(MdbTransactionFlags.RdOnly))
            {
                LmdbException.ThrowOnError(LmdbFunctions.TxnReset(txn));

                if (startNew)
                {
                    LmdbException.ThrowOnError(LmdbFunctions.TxnRenew(txn));
                }
            }
            else
            {
                throw new LmdbException("Only read transactions can be reset");
            }
        }

        public LmdbTransaction CreateChildTransaction(MdbTransactionFlags flags = MdbTransactionFlags.None)
        {
            uint flagsValue = (uint) flags;

            MdbTxn* childTxn = null;
            
            LmdbException.ThrowOnError(LmdbFunctions.TxnBegin(environment.Env,txn,flagsValue,&childTxn));
            
            return new LmdbTransaction(environment,childTxn,flags);
        }

        public bool DatabaseExists(string databaseName)
        {
            MdbDbi dbi;
            var errorCode = LmdbFunctions.DbiOpen(txn, databaseName, (uint) MdbDbiFlags.None, &dbi);

            if (errorCode == (int) LmdbErrorCode.NotFound)
            {
                return false;
            }
            else
            {
                LmdbFunctions.DbiClose(environment.Env,dbi);
            }
            
            LmdbException.ThrowOnError(errorCode);

            return true;
        }

        public LmdbDatabase CreateDatabase(string databaseName,MdbDbiFlags flags = MdbDbiFlags.None)
        {
            var finalFlags = flags | MdbDbiFlags.Create;
            MdbDbi dbi;
            LmdbException.ThrowOnError(LmdbFunctions.DbiOpen(txn,databaseName,(uint)finalFlags,&dbi));

            return new LmdbDatabase(environment, dbi);
        }

        public LmdbDatabase GetDatabase(string databaseName,MdbDbiFlags flags = MdbDbiFlags.None)
        {
            var finalFlags = flags ^ MdbDbiFlags.Create;
            MdbDbi dbi;
            LmdbException.ThrowOnError(LmdbFunctions.DbiOpen(txn,databaseName,(uint)finalFlags,&dbi));

            return new LmdbDatabase(environment, dbi);
        }

        public LmdbDatabase GetOrCreateDatabase(string databaseName, MdbDbiFlags flags = MdbDbiFlags.None)
        {
            return DatabaseExists(databaseName) ? GetDatabase(databaseName) : CreateDatabase(databaseName);
        }

        public void ClearDatabase(LmdbDatabase database)
        {
            LmdbException.ThrowOnError(LmdbFunctions.Drop(txn,database.Dbi,0));
        }
        
        public void DestroyDatabase(LmdbDatabase database)
        {
            LmdbException.ThrowOnError(LmdbFunctions.Drop(txn,database.Dbi,1));
            database.IsDisposed = true;
        }

        public LmdbCursor CreateCursor(LmdbDatabase database)
        {
            MdbCursor* cursor = null;
            LmdbException.ThrowOnError(LmdbFunctions.CursorOpen(txn,database.Dbi,&cursor));
            
            return new LmdbCursor(cursor,txn,database.Dbi);
        }

        public bool TryGetValue(LmdbDatabase database, in MemorySegment key, out MemorySegment value)
        {
            MdbVal mdbKey = key;
            MdbVal mdbValue = new MdbVal();

            var errorCode = LmdbFunctions.Get(txn, database.Dbi, &mdbKey, &mdbValue);

            if (errorCode == (int) LmdbErrorCode.NotFound)
            {
                value = MemorySegment.Null;
                return false;
            }

            LmdbException.ThrowOnError(errorCode);

            value = mdbValue;
            return true;
        }

        public void PutValue(LmdbDatabase database, in MemorySegment key, in MemorySegment value,
            MdbPutFlags flags = MdbPutFlags.None)
        {
            uint finalFlags = (uint) flags;

            MdbVal mdbKey = key;
            MdbVal mdbValue = value;
         
            LmdbException.ThrowOnError(LmdbFunctions.Put(txn,database.Dbi,&mdbKey,&mdbValue,finalFlags));
        }

        public void DeleteValue(LmdbDatabase database, in MemorySegment key)
        {
            var mdbKey = new MdbVal(key);
            
            LmdbException.ThrowOnError(LmdbFunctions.Del(txn,database.Dbi,&mdbKey,null));
        }

        public void DeleteValue(LmdbDatabase database, in MemorySegment key, in MemorySegment value)
        {
            var mdbKey = new MdbVal(key);
            var mdbValue = new MdbVal(value);
            
            LmdbException.ThrowOnError(LmdbFunctions.Del(txn,database.Dbi,&mdbKey,&mdbValue));
        }
        
        public void Dispose()
        {
            if (!wasCommitted)
            {
                if (flags.HasFlag(MdbTransactionFlags.RdOnly))
                {
                    LmdbException.ThrowOnError(LmdbFunctions.TxnCommit(txn));
                }
                else
                {
                    LmdbFunctions.TxnAbort(txn);
                }
            }
        }
    }
}