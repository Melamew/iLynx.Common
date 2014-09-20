using System;

namespace iLynx.Common.Collections
{
    public class PassivelyTimedCacheEntry<T> : CacheEntryBase<T>
    {
        private T item;
        public DateTime Updated { get; private set; }

        public PassivelyTimedCacheEntry(T item)
        {
            this.item = item;
            Updated = DateTime.Now;
        }

        public PassivelyTimedCacheEntry(T item, DateTime lastUpdated)
        {
            this.item = item;
            Updated = lastUpdated;
        } 

        public override T Item
        {
            get { return item; }
            set
            {
                item = value;
                Updated = DateTime.Now;
            }
        }
    }
}