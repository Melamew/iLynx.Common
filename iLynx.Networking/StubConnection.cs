using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.Threading;

namespace iLynx.Networking
{
    public class StubConnection<TMessage, TMessageKey> : ConnectionBase<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly IConnectionStubBuilder<TMessage, TMessageKey> stubBuilder;
        private readonly IThreadManager threadManager;
        private IConnectionStub<TMessage, TMessageKey> stub;
        private IWorker messageWorker;
        private readonly object syncRoot = new object();
        private bool doProcess;

        public StubConnection(IThreadManager threadManager, IConnectionStubBuilder<TMessage, TMessageKey> stubBuilder)
        {
            this.stubBuilder = Guard.IsNull(() => stubBuilder);
            this.threadManager = Guard.IsNull(() => threadManager);
        }

        public StubConnection(IThreadManager threadManager, IConnectionStub<TMessage, TMessageKey> establishedConnection)
        {
            this.threadManager = Guard.IsNull(() => threadManager);
            stub = establishedConnection;
            RunReader();
        } 

        public override void Connect(EndPoint endPoint)
        {
            if (null == stubBuilder) throw new InvalidOperationException("This StubConnection does not support new connections");
            if (IsConnected) throw new InvalidOperationException("This connection is already connected");
            stub = stubBuilder.Build(endPoint);
            RunReader();
        }

        private void RunReader()
        {
            if (!stub.CanRead) return;
            doProcess = true;
            messageWorker = threadManager.StartNew(ProcessMessages);
        }

        private void ProcessMessages()
        {
            while (doProcess)
            {
                int size;
                TMessage message = stub.ReadNext(out size);
                if (0 > size)
                {
                    Close();
                    return;
                }
                PublishMessage(message, size);
            }
        }

        private void Close()
        {
            if (null == stub) return;
            stub.Dispose();
            doProcess = false;
            stub = null;
        }

        public override void Disconnect()
        {
            Close();
            if (null == messageWorker)
                return;
            doProcess = false;
            messageWorker.Wait();
        }

        public override bool IsConnected
        {
            get { return null != stub && stub.IsOpen; }
        }

        public override int Send(TMessage keyedMessage)
        {
            if (null == stub)
                throw new InvalidOperationException("Not connected");
            int size;
            lock (syncRoot)
            {
                if (!stub.CanWrite)
                {
                    if (!stub.IsOpen) throw new InvalidOperationException("Not connected");
                    throw new InvalidOperationException("The connection stub does not support writing");
                }
                size = stub.Write(keyedMessage);
            }
            return size;
        }

        public TMessage Send(TMessage message, TMessageKey responseKey, out int txSize, out int rxSize, TimeSpan? timeout = null)
        {
            var result = default(TMessage);
            var s = 0;
            var received = false;
            var subscriber = new MessageReceivedHandler<TMessage, TMessageKey>((keyedMessage, size) =>
            {
                result = message;
                s = size;
                received = true;
            });
            Subscribe(responseKey, subscriber);
            try
            {
                txSize = Send(message);
                var rxBegin = DateTime.UtcNow;
// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (received) // This variable is changed when the messagehandler above receives a message
                {
                    if (null != timeout && (DateTime.UtcNow - rxBegin) >= timeout)
                        throw new TimeoutException();
                    Thread.CurrentThread.Join(5);
                }
            }
            finally
            {
                Unsubscribe(responseKey, subscriber);
            }
            rxSize = s;
            return result;
        }

        public override async Task<int> SendAsync(TMessage keyedMessage)
        {
            return await Task.Run(() => Send(keyedMessage));
        }
    }
}