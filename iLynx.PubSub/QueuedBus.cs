using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iLynx.Common;
using iLynx.Threading;

namespace iLynx.PubSub
{
    public class QueuedBus<T> : Bus<T>, IDisposable
    {
        private readonly IWorker worker;
        private bool isRunning = true;
        private readonly Queue<Tuple<Type, dynamic>> messageQueue = new Queue<Tuple<Type, dynamic>>();

        public QueuedBus(IThreadManager threadManager)
        {
            var manager = Guard.IsNull(() => threadManager);
            worker = manager.StartNew(MessagePump);
        }

        private void MessagePump()
        {
            while (isRunning)
            {
                if (messageQueue.Count < 1)
                {
                    Thread.CurrentThread.Join(5);
                    continue;
                }
                var item = messageQueue.Dequeue();
                Publish(item.Item1, item.Item2);
            }
            PopLast();
        }

        private void PopLast()
        {
            while (messageQueue.Count > 1)
            {
                var item = messageQueue.Dequeue();
                Publish(item.Item1, item.Item2);
            }
        }

        protected override void Publish(Type messageType, dynamic message)
        {
            var subscribers = GetSubscribers(messageType);
            if (null == subscribers) return;
            foreach (var subscriber in subscribers.Where(x => null != x))
                try { subscriber.Invoke(message); }
                catch (Exception e) { RuntimeCommon.DefaultLogger.Log(LogLevel.Error, this, string.Format("Unable to send {0} to {1}, got Exception {2}", message, subscriber, e)); }
        }

        public override void Publish<TMessage>(TMessage message)
        {
            messageQueue.Enqueue(new Tuple<Type, dynamic>(typeof (TMessage), message));
        }

        public override async Task PublishAsync<TMessage>(TMessage message)
        {
            await Task.Run(() => Publish(message));
        }

        public void Dispose()
        {
            isRunning = false;
            if (null != worker)
                worker.Wait();
        }
    }
}