using System;
using System.IO;
using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    public class LmdbEnvironmentOptions
    {
        public const ulong DefaultMaxSize = 1024ul * 1024ul* 4096ul * 10ul;
        
        public LmdbEnvironmentOptions(string path, uint maxDatabases = 128, uint maxReaders = 126, ulong maxSize = DefaultMaxSize, MdbEnvFlags flags = MdbEnvFlags.None)
        {
            Path = path;
            MaxDatabases = maxDatabases;
            MaxReaders = maxReaders;
            MaxSize = maxSize;
            Flags = flags;
        }

        public string Path { get; }
        
        public MdbEnvFlags Flags { get; }
        
        public ulong MaxSize { get; }
        
        public uint MaxReaders { get; }
        
        public uint MaxDatabases { get; }
    }

    public sealed unsafe class LmdbEnvironment : IDisposable
    {
        private readonly MdbEnv* env;
        private readonly LmdbEnvironmentOptions options;

        public LmdbEnvironment(LmdbEnvironmentOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            
            if (!Directory.Exists(options.Path))
            {
                Directory.CreateDirectory(options.Path);
            }
            
            try
            {
                MdbEnv* envPtr = null;
              
                LmdbException.ThrowOnError(LmdbFunctions.EnvCreate(&envPtr));
            
                this.env = envPtr;

                LmdbException.ThrowOnError(LmdbFunctions.EnvSetMapSize(env,options.MaxSize));
                LmdbException.ThrowOnError(LmdbFunctions.EnvSetMaxDbs(env,options.MaxDatabases));
                LmdbException.ThrowOnError(LmdbFunctions.EnvSetMaxReaders(env,options.MaxReaders));
                
                LmdbException.ThrowOnError(LmdbFunctions.EnvOpen(env, options.Path,(uint)options.Flags, 0));
            }
            catch
            {
                LmdbFunctions.EnvClose(env);
                throw;
            }
        }

        internal MdbEnv* Env => env;

        public int MaxKeySize => LmdbFunctions.EnvGetMaxKeySize(env);

        public MdbEnvFlags Flags
        {
            get
            {
                uint flagsValue = 0;
                LmdbException.ThrowOnError(LmdbFunctions.EnvGetFlags(env,&flagsValue));

                return (MdbEnvFlags) flagsValue;
            }
        }

        public LmdbStatistics Statistics
        {
            get
            {
                var stat = new MdbStat();
                LmdbException.ThrowOnError(LmdbFunctions.EnvStat(env,&stat));
                return new LmdbStatistics(stat);
            }
        }

        public LmdbEnvironmentInfo EnvironmentInfo
        {
            get
            {
                var envInfo = new MdbEnvInfo();
                LmdbException.ThrowOnError(LmdbFunctions.EnvInfo(env,&envInfo));
                return new LmdbEnvironmentInfo(envInfo);
            }
        }

        public void SetFlags(MdbEnvFlags flags, bool onOff)
        {
            uint flagsValues = (uint) flags;
            
            LmdbException.ThrowOnError(LmdbFunctions.EnvSetFlags(env,flagsValues,onOff ? 1 : 0));
        }

        public void CopyTo(string path,bool compact = false)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            LmdbException.ThrowOnError(LmdbFunctions.EnvCopy2(env, path, compact ? (uint) 0 : 0x01));
        }

        public void Sync(bool force = false)
        {
            LmdbException.ThrowOnError(LmdbFunctions.EnvSync(env,force ? 1 : 0));
        }

        public LmdbTransaction CreateTransaction(MdbTransactionFlags flags = MdbTransactionFlags.None)
        {
            uint flagsValue = (uint) flags;

            MdbTxn* txn = null;
            
            LmdbException.ThrowOnError(LmdbFunctions.TxnBegin(env,null,flagsValue,&txn));
            
            return new LmdbTransaction(this,txn,flags);
        }
        
        private void ReleaseUnmanagedResources()
        {
            LmdbFunctions.EnvClose(env);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~LmdbEnvironment()
        {
            ReleaseUnmanagedResources();
        }
    }
}