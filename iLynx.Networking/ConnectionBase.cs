using System;
using System.Net;
using System.Threading.Tasks;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Networking
{
    public abstract class ConnectionBase<TMessage, TMessageKey> : KeyedSubscriptionManager<TMessageKey, MessageReceivedHandler<TMessage, TMessageKey>>, IConnection<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        public abstract int Send(TMessage keyedMessage);
        public abstract Task<int> SendAsync(TMessage keyedMessage);

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public abstract void Connect(EndPoint endPoint);
        public abstract void Disconnect();
        public abstract bool IsConnected { get; }

        protected virtual void OnConnected()
        {
            var handler = Connected;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {
            var handler = Disconnected;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        protected virtual void PublishMessage(TMessage keyedMessage, int totalSize)
        {
            if (null == keyedMessage) return;
            var subscribers = GetSubscribers(keyedMessage.Key);
            if (null == subscribers) return;
            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber.Invoke(keyedMessage, totalSize);
                }
                catch (Exception e)
                {
                    RuntimeCommon.DefaultLogger.Log(LogLevel.Error, this,
                        string.Format("Unable to notify {0} of keyedMessage received, key was {1}, exception was {2}",
                            subscriber, keyedMessage.Key, e));
                }
            }
        }

        protected virtual async Task PublishMessageAsync(TMessage keyedMessage, int totalSize)
        {
            await Task.Run(() => PublishMessage(keyedMessage, totalSize));
        }
    }
}
