using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iLynx.PubSub
{
    public class KeyedSubscriptionManager<TKey, TSubscriber> : IKeyedSubscriptionManager<TKey, TSubscriber>
    {
        private readonly Dictionary<TKey, List<TSubscriber>> subscribers = new Dictionary<TKey, List<TSubscriber>>();
        private readonly ReaderWriterLockSlim subscriberLock = new ReaderWriterLockSlim();

        public virtual IEnumerable<TSubscriber> GetSubscribers(TKey messageType)
        {
            subscriberLock.EnterReadLock();
            try
            {
                List<TSubscriber> result;
                if (!subscribers.TryGetValue(messageType, out result))
                    return null;

                // Copy to avoid enumeration failures due to modifications
                var copy = new TSubscriber[result.Count];
                result.CopyTo(copy);
                return copy;
            }
            finally { subscriberLock.ExitReadLock(); }
        }

        public virtual async Task<IEnumerable<TSubscriber>> GetSubscribersAsync(TKey messageType)
        {
            return await Task.Run(() => GetSubscribers(messageType));
        }

        public virtual void Subscribe(TKey key, TSubscriber subscriber)
        {
            subscriberLock.EnterWriteLock();
            try
            {
                List<TSubscriber> subscriberList;
                if (!subscribers.TryGetValue(key, out subscriberList))
                {
                    subscriberList = new List<TSubscriber>();
                    subscribers.Add(key, subscriberList);
                }
                if (subscriberList.Contains(subscriber)) return;
                subscriberList.Add(subscriber);
            }
            finally
            {
                subscriberLock.ExitWriteLock();
            }
        }

        public virtual void Unsubscribe(TKey key, TSubscriber subscriber)
        {
            subscriberLock.EnterWriteLock();
            try
            {
                List<TSubscriber> subscriberList;
                if (!subscribers.TryGetValue(key, out subscriberList)) return;
                subscriberList.Remove(subscriber);
            }
            finally
            {
                subscriberLock.ExitWriteLock();
            }
        }
    }
}