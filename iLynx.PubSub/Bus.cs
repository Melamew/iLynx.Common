using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iLynx.Common;

namespace iLynx.PubSub
{
    public class Bus<T> : KeyedSubscriptionManager<Type, dynamic>, IBus<T>
    {
        public void Subscribe<TMessage>(SubscriberCallback<TMessage> subscriber) where TMessage : T
        {
            Subscribe(typeof (TMessage), subscriber);
        }

        public virtual void Unsubscribe<TMessage>(SubscriberCallback<TMessage> subscriber) where TMessage : T
        {
            Unsubscribe(typeof (TMessage), subscriber);
        }

        public virtual void Publish<TMessage>(TMessage message) where TMessage : T
        {
            var type = typeof(TMessage);
            Publish(type, message);
            this.LogDebug("Publish: {0}", message);
        }

        protected virtual void Publish(Type messageType, dynamic message)
        {
            var subscribers = GetSubscribers(messageType);
            if (null == subscribers) return;
            foreach (var subscriber in subscribers.Where(x => null != x))
                try { subscriber.Invoke(message); }
                catch (Exception e) { RuntimeCommon.DefaultLogger.Log(LogLevel.Error, this, string.Format("Unable to send {0} to {1}, got Exception {2}", message, subscriber, e)); }
        }

        private void Publish<TMessage>(TMessage message, IEnumerable<dynamic> subscribers)
        {
            foreach (var subscriber in subscribers)
                try { subscriber.Invoke(message); }
                catch (Exception e) { RuntimeCommon.DefaultLogger.Log(LogLevel.Error, this, string.Format("Unable to send {0} to {1}, got Exception {2}", message, subscriber, e)); }
        }

        public virtual async Task PublishAsync<TMessage>(TMessage message) where TMessage : T
        {
            var type = typeof(TMessage);
            var subscribers = await GetSubscribersAsync(type);
            await Task.Run(() => Publish(message, subscribers));
        }
    }
}
