using System.Threading.Tasks;

namespace iLynx.PubSub
{
    public interface IBus<in T>
    {
        void Subscribe<TMessage>(SubscriberCallback<TMessage> subscriber) where TMessage : T;
        void Unsubscribe<TMessage>(SubscriberCallback<TMessage> subscriber) where TMessage : T;
        void Publish<TMessage>(TMessage message) where TMessage : T;
        Task PublishAsync<TMessage>(TMessage message) where TMessage : T;
    }
}