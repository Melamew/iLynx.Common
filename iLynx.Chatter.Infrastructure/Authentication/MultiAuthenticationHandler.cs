using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using iLynx.Common.Serialization;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public interface IMultiAuthenticationHandler<TMessage, TKey> : IAuthenticationHandler<TMessage, TKey>
        where TMessage : IKeyedMessage<TKey>
    {
        void AddHandler(IAuthenticationHandler<TMessage, TKey> handler);
        void RemoveHandler(IAuthenticationHandler<TMessage, TKey> handler);
    }

    public abstract class MultiAuthenticationHandler : IMultiAuthenticationHandler<ChatMessage, int>
    {
        private readonly Dictionary<string, IAuthenticationHandler<ChatMessage, int>> handlers = new Dictionary<string, IAuthenticationHandler<ChatMessage, int>>();
        private readonly ReaderWriterLockSlim handlerLock = new ReaderWriterLockSlim();
        private readonly IBitConverter bitConverter = Serializer.SingletonBitConverter;

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

        public void AddHandler(IAuthenticationHandler<ChatMessage, int> handler)
        {
            handlerLock.EnterWriteLock();
            try
            {
                var fullName = handler.GetType().FullName;
                if (handlers.ContainsKey(fullName)) return;
                handlers.Add(fullName, handler);
            }
            finally { handlerLock.ExitWriteLock(); }
        }

        public void RemoveHandler(IAuthenticationHandler<ChatMessage, int> handler)
        {
            handlerLock.EnterWriteLock();
            try
            {
                handlers.Remove(handler.GetType().FullName);
            }
            finally { handlerLock.ExitWriteLock(); }
        }

        protected IEnumerable<KeyValuePair<string, IAuthenticationHandler<ChatMessage, int>>> GetAllHandlers()
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

        protected static void SendAuthenticatorList(IConnectionStub<ChatMessage, int> connection, IEnumerable<KeyValuePair<string, IAuthenticationHandler<ChatMessage, int>>> handlers)
        {
            var identifiers = handlers.Select(x => new AuthenticationHandlerIdentifier { Name = x.Key });
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

        protected IEnumerable<KeyValuePair<string, IAuthenticationHandler<ChatMessage, int>>> FindCommonHandlers(
            IEnumerable<AuthenticationHandlerIdentifier> identifiers)
        {
            handlerLock.EnterReadLock();
            try
            {
                return handlers.Join(identifiers, pair => pair.Key, identifier => identifier.Name,
                    (pair, identifier) => pair);
            }
            finally { handlerLock.ExitReadLock(); }
        }

        protected IAuthenticationHandler<ChatMessage, int> GetHandler(string id)
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
                return x.Name == y.Name;
            }

            public int GetHashCode(AuthenticationHandlerIdentifier obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected class AuthenticationHandlerIdentifier
        {
            public string Name { get; set; }
        }
    }

    public class ServerMultiAuthenticationHandler : MultiAuthenticationHandler
    {
        public override bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            SendAuthenticatorList(connection, GetAllHandlers());
            var remoteAuthMethods = ReceiveAuthenticatorList(connection);
            var strongest = FindCommonHandlers(remoteAuthMethods).OrderByDescending(x => x.Value.Strength).FirstOrDefault();
            if (null == strongest.Value)
                return false;
            var message = new ChatMessage
            {
                Key = MessageKeys.Authentication,
                ClientId = Guid.Empty,
                Data = Encoding.Unicode.GetBytes(strongest.Key)
            };
            connection.Write(message);
            return strongest.Value.Authenticate(connection);
        }
    }

    public class ClientMultiAuthenticationHandler : MultiAuthenticationHandler
    {
        public override bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            var list = ReceiveAuthenticatorList(connection);
            var matches = FindCommonHandlers(list);
            SendAuthenticatorList(connection, matches);
            int size;
            var message = connection.ReadNext(out size);
            if (null == message) return false;
            var handlerId = Encoding.Unicode.GetString(message.Data);
            var handler = GetHandler(handlerId);
            return null != handler && handler.Authenticate(connection);
        }
    }
}
