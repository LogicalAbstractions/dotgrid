using System.Collections.Generic;
using Lucene.Net.Store;

namespace DotGrid.Storage.Lmdb.Lucene
{
    public class LuceneLmdbDirectory : BaseDirectory
    {
        public override string[] ListAll()
        {
            throw new System.NotImplementedException();
        }

        public override bool FileExists(string name)
        {
            throw new System.NotImplementedException();
        }

        public override void DeleteFile(string name)
        {
            throw new System.NotImplementedException();
        }

        public override long FileLength(string name)
        {
            throw new System.NotImplementedException();
        }

        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            
            throw new System.NotImplementedException();
        }

        public override void Sync(ICollection<string> names)
        {
            throw new System.NotImplementedException();
        }

        public override IndexInput OpenInput(string name, IOContext context)
        {
            throw new System.NotImplementedException();
        }

        public override Lock MakeLock(string name)
        {
            throw new System.NotImplementedException();
        }

        public override void ClearLock(string name)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLockFactory(LockFactory lockFactory)
        {
            throw new System.NotImplementedException();
        }

        public override LockFactory LockFactory { get; }
    }
}