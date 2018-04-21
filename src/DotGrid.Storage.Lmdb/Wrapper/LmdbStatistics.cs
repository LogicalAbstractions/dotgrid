using DotGrid.Storage.Lmdb.Interop;

namespace DotGrid.Storage.Lmdb.Wrapper
{
    using MdbSize = System.UInt64;
    
    public sealed class LmdbStatistics
    {
        private readonly MdbStat statistics;

        internal LmdbStatistics(MdbStat statistics)
        {
            this.statistics = statistics;
        }

        public uint PageSize => statistics.PSize;

        public uint BTreeDepth => statistics.Depth;

        public MdbSize BranchPages => statistics.BranchPages;

        public MdbSize LeafPages => statistics.LeafPages;

        public MdbSize OverflowPages => statistics.OverflowPages;

        public MdbSize DataItemCount => statistics.Entries;

        public override string ToString()
        {
            return $"{nameof(PageSize)}: {PageSize}, {nameof(BTreeDepth)}: {BTreeDepth}, {nameof(BranchPages)}: {BranchPages}, {nameof(LeafPages)}: {LeafPages}, {nameof(OverflowPages)}: {OverflowPages}, {nameof(DataItemCount)}: {DataItemCount}";
        }
    }
}