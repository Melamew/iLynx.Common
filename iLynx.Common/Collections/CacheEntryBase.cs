using System;

namespace iLynx.Common.Collections
{
    public abstract class CacheEntryBase<T> : IDisposable, ICacheEntry<T>
    {
        public abstract T Item { get; set; }

        ~CacheEntryBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing){}

        public void Dispose()
        {
            Dispose(true);
        }
    }
}