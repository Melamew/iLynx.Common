using System;
using System.Collections.Generic;

namespace iLynx.Common.Collections
{
    public interface ICache<T> : IDisposable, IEnumerable<T> where T : IIDentifiable
    {
        TimeSpan Timeout { get; set; }
        int Count { get; }
        void AddOrUpdate(T item);
        void Clear();
        bool Contains(T item);
        bool Remove(T item);
    }
}