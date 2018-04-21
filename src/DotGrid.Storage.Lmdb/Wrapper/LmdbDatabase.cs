using System;
using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    using MdbDbi = System.UInt32;
    
    public sealed unsafe class LmdbDatabase : IDisposable
    {
        private readonly LmdbEnvironment environment;
        private readonly MdbDbi dbi;

        public MdbDbi Dbi => dbi;
        
        internal bool IsDisposed { get; set; }

        internal LmdbDatabase(LmdbEnvironment environment, MdbDbi dbi)
        {
            this.environment = environment;
            this.dbi = dbi;
        }

        public LmdbStatistics Statistics
        {
            get
            {
                using (var transaction = environment.CreateTransaction(MdbTransactionFlags.RdOnly))
                {
                    MdbStat stat;
                    LmdbException.ThrowOnError(LmdbFunctions.Stat(transaction.Txn,dbi,&stat));
                    
                    return new LmdbStatistics(stat);
                }
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                LmdbFunctions.DbiClose(environment.Env, dbi);
            }

            IsDisposed = true;
        }
    }
}