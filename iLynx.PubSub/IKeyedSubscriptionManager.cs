using System.Collections.Generic;
using System.Threading.Tasks;

namespace iLynx.PubSub
{
    public interface IKeyedSubscriptionManager<in TKey, TDelegate>
    {
        IEnumerable<TDelegate> GetSubscribers(TKey key);
        Task<IEnumerable<TDelegate>> GetSubscribersAsync(TKey key); 
        void Subscribe(TKey key, TDelegate subscriber);
        void Unsubscribe(TKey key, TDelegate subscriber);
    }
}
