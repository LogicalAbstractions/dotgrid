using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    using MdbSize = System.UInt64;
    
    public sealed class LmdbEnvironmentInfo
    {
        private readonly MdbEnvInfo envInfo;

        internal LmdbEnvironmentInfo(MdbEnvInfo envInfo)
        {
            this.envInfo = envInfo;
        }

        public unsafe ulong MapAddress => (ulong)envInfo.MapAddr;

        public MdbSize MapSize => envInfo.MapSize;

        public MdbSize LastPageNumber => envInfo.LastPgno;

        public MdbSize LastTransactionId => envInfo.LastTxnId;

        public uint MaxReaders => envInfo.MaxReaders;

        public uint NumReaders => envInfo.NumReaders;

        public override string ToString()
        {
            return $"{nameof(MapAddress)}: {MapAddress}, {nameof(MapSize)}: {MapSize}, {nameof(LastPageNumber)}: {LastPageNumber}, {nameof(LastTransactionId)}: {LastTransactionId}, {nameof(MaxReaders)}: {MaxReaders}, {nameof(NumReaders)}: {NumReaders}";
        }
    }
}