using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using iLynx.Common.Serialization;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public abstract class CompositeAuthenticationHandler : IMultiAuthenticationHandler<ChatMessage, int>
    {
        private readonly Dictionary<Guid, IAuthenticationHandler<ChatMessage, int>> handlers = new Dictionary<Guid, IAuthenticationHandler<ChatMessage, int>>();
        private readonly ReaderWriterLockSlim handlerLock = new ReaderWriterLockSlim();

        public int Strength
        {
            get
            {
                handlerLock.EnterReadLock();
                int result;
                try
                {
                    result = handlers.Values.Max(x => x.Strength);
                }
                finally { handlerLock.ExitReadLock(); }
                return result;
            }
        }

        /// <summary>
        /// This authenticator is a composite and thus, will return an empty Id
        /// </summary>
        public Guid AuthenticatorId { get { return Guid.Empty; } }

        public void AddHandler(IAuthenticationHandler<ChatMessage, int> handler)
        {
            handlerLock.EnterWriteLock();
            try
            {
                var id = handler.AuthenticatorId;
                if (handlers.ContainsKey(id)) return;
                handlers.Add(id, handler);
            }
            finally { handlerLock.ExitWriteLock(); }
        }

        public void RemoveHandler(IAuthenticationHandler<ChatMessage, int> handler)
        {
            handlerLock.EnterWriteLock();
            try
            {
                handlers.Remove(handler.AuthenticatorId);
            }
            finally { handlerLock.ExitWriteLock(); }
        }

        protected IEnumerable<KeyValuePair<Guid, IAuthenticationHandler<ChatMessage, int>>> GetAllHandlers()
        {
            handlerLock.EnterReadLock();
            try
            {
                // Enumerate directly to array to avoid lazy evaluation here.
                return handlers.ToArray();
            }
            finally { handlerLock.ExitReadLock(); }
        }

        public abstract bool Authenticate(IConnectionStub<ChatMessage, int> connection);

        protected static void SendAuthenticatorList(IConnectionStub<ChatMessage, int> connection, IEnumerable<KeyValuePair<Guid, IAuthenticationHandler<ChatMessage, int>>> handlers)
        {
            var identifiers = handlers.Select(x => new AuthenticationHandlerIdentifier { AuthenticatorId = x.Key });
            var message = new ChatMessage
            {
                Key = MessageKeys.Authentication,
                ClientId = Guid.Empty,
            };
            using (var outputStream = new MemoryStream())
            {
                foreach (var identifier in identifiers)
                    Serializer.Serialize(identifier, outputStream);
                message.Data = outputStream.ToArray();
            }
            connection.Write(message);
        }

        protected static IEnumerable<AuthenticationHandlerIdentifier> ReceiveAuthenticatorList(IConnectionStub<ChatMessage, int> connection)
        {
            int size;
            var message = connection.ReadNext(out size);
            if (null == message) yield break;

            using (var dataStream = new MemoryStream(message.Data))
            {
                while (dataStream.Position < dataStream.Length)
                    yield return Serializer.Deserialize<AuthenticationHandlerIdentifier>(dataStream);
            }
        }

        protected IEnumerable<KeyValuePair<Guid, IAuthenticationHandler<ChatMessage, int>>> FindCommonHandlers(
            IEnumerable<AuthenticationHandlerIdentifier> identifiers)
        {
            handlerLock.EnterReadLock();
            try
            {
                return handlers.Join(identifiers, pair => pair.Key, identifier => identifier.AuthenticatorId,
                    (pair, identifier) => pair);
            }
            finally { handlerLock.ExitReadLock(); }
        }

        protected IAuthenticationHandler<ChatMessage, int> GetHandler(Guid id)
        {
            handlerLock.EnterReadLock();
            try
            {
                IAuthenticationHandler<ChatMessage, int> handler;
                return !handlers.TryGetValue(id, out handler) ? null : handler;
            }
            finally { handlerLock.ExitReadLock(); }
        }

        protected class AuthenticationHandlerIdentifierComparer : IEqualityComparer<AuthenticationHandlerIdentifier>
        {
            public bool Equals(AuthenticationHandlerIdentifier x, AuthenticationHandlerIdentifier y)
            {
                return x.AuthenticatorId == y.AuthenticatorId;
            }

            public int GetHashCode(AuthenticationHandlerIdentifier obj)
            {
                return obj.AuthenticatorId.GetHashCode();
            }
        }

        protected class AuthenticationHandlerIdentifier
        {
            public Guid AuthenticatorId { get; set; }
        }
    }
}
