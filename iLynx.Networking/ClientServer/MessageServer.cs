using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Common.Threading.Unmanaged;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Networking.ClientServer
{
    public class MessageServer<TMessage, TKey> : IMessageServer<TMessage, TKey>, IDisposable where TMessage : IClientMessage<TKey>
    {
        private readonly IBus<MessageEnvelope<TMessage, TKey>> messageBus;
        private readonly IClientBuilder<TMessage, TKey> clientBuilder;
        private readonly IAuthenticationHandler<TMessage, TKey> authenticationHandler;
        private readonly IThreadManager threadManager;
        private readonly IConnectionStubListener<TMessage, TKey> listener;
        private IWorker connectionWorker;
        private bool isStarted;
        private readonly ReaderWriterLockSlim clientLock = new ReaderWriterLockSlim();
        private readonly Dictionary<Guid, IClient<TMessage, TKey>> connectedClients = new Dictionary<Guid, IClient<TMessage, TKey>>();

        public MessageServer(IConnectionStubListener<TMessage, TKey> listener,
            IClientBuilder<TMessage, TKey> clientBuilder,
            IAuthenticationHandler<TMessage, TKey> authenticationHandler,
            IThreadManager threadManager,
            IBus<MessageEnvelope<TMessage, TKey>> messageBus)
        {
            this.messageBus = Guard.IsNull(() => messageBus);
            this.clientBuilder = Guard.IsNull(() => clientBuilder);
            this.authenticationHandler = Guard.IsNull(() => authenticationHandler);
            this.threadManager = Guard.IsNull(() => threadManager);
            this.listener = Guard.IsNull(() => listener);
            this.messageBus.Subscribe<MessageEnvelope<TMessage, TKey>>(HandleMessage);
        }

        private void HandleMessage(MessageEnvelope<TMessage, TKey> envelope)
        {
            if (null == envelope.TargetClients || 0 == envelope.TargetClients.Length)
                SendMessage(envelope.Message);
            else
                SendMessage(envelope.Message, envelope.TargetClients);
        }

        public void Start(EndPoint localEndpoint)
        {
            if (isStarted) return;
            isStarted = true;
            listener.BindTo(localEndpoint);
            connectionWorker = threadManager.StartNew(AcceptConnections);
        }

        public void Stop()
        {
            if (!isStarted) return;
            isStarted = false;
            listener.Close();
            connectionWorker.Wait();
            connectionWorker = null;
        }

        private void AcceptConnections()
        {
            while (isStarted)
            {
                var connection = listener.AcceptNext();
                if (null == connection) continue;
                if (!authenticationHandler.Authenticate(connection))
                {
                    connection.Dispose();
                    continue;
                }
                var client = clientBuilder.Build(connection);
                if (null == client) continue;
                clientLock.EnterWriteLock();
                try
                {
                    if (connectedClients.ContainsKey(client.ClientId))
                    {
                        client.Close();
                        continue;
                    }
                    connectedClients.Add(client.ClientId, client);
                    client.Disconnected += ClientOnDisconnected;
                    OnClientConnected(client.ClientId);
                }
                finally { clientLock.ExitWriteLock(); }
            }
        }

        private void ClientOnDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            clientLock.EnterUpgradeableReadLock();
            try
            {
                var clientId = clientDisconnectedEventArgs.ClientId;
                if (!connectedClients.ContainsKey(clientId)) return;
                clientLock.EnterWriteLock();
                try
                {
                    connectedClients.Remove(clientId);
                }
                finally { clientLock.ExitWriteLock(); }
                OnClientDisconnected(clientId);
            }
            finally { clientLock.ExitUpgradeableReadLock(); }
        }

        protected virtual void OnClientConnected(Guid clientId)
        {
            var handler = ClientConnected;
            if (null == handler) return;
            handler(this, new ClientConnectedEventArgs(clientId));
        }

        protected virtual void OnClientDisconnected(Guid clientId)
        {
            var handler = ClientDisconnected;
            if (null == handler) return;
            handler(this, new ClientDisconnectedEventArgs(clientId));
        }

        public void SendMessage(TMessage message)
        {
            Task.Run(() =>
            {
                clientLock.EnterReadLock();
                try
                {
                    foreach (var client in connectedClients.Values)
                        client.Send(message);
                }
                finally { clientLock.ExitReadLock(); }
            });
        }

        public void SendMessage(TMessage message, IEnumerable<Guid> clients)
        {
            Task.Run(() =>
            {
                clientLock.EnterReadLock();
                try
                {
                    foreach (var clientId in clients)
                    {
                        IClient<TMessage, TKey> client;
                        if (!connectedClients.TryGetValue(clientId, out client)) continue;
                        client.Send(message);
                    }
                }
                finally { clientLock.ExitReadLock(); }
            });
        }

        public IEnumerable<Guid> ConnectedClients
        {
            get
            {
                clientLock.EnterReadLock();
                try
                {
                    var copy = new Guid[connectedClients.Count];
                    connectedClients.Keys.CopyTo(copy, 0);
                    return copy;
                }
                finally { clientLock.ExitReadLock(); }
            }
        }

        public bool IsRunning { get { return null != listener && listener.IsBound; } }

        public void Disconnect(Guid clientId)
        {
            clientLock.EnterReadLock();
            try
            {
                IClient<TMessage, TKey> client;
                if (!connectedClients.TryGetValue(clientId, out client)) return;
                client.Close();
            }
            finally { clientLock.ExitReadLock(); }
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public void Dispose()
        {
            messageBus.Unsubscribe<MessageEnvelope<TMessage, TKey>>(HandleMessage);
        }
    }
}