using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.Infrastructure
{
    public abstract class NickManagerBase : INickManagerService, IDisposable
    {
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager;
        private readonly Dictionary<Guid, string> clientNicks = new Dictionary<Guid, string>();
        private readonly ReaderWriterLockSlim nickLock = new ReaderWriterLockSlim();

        protected NickManagerBase(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager)
        {
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            this.subscriptionManager.Subscribe(MessageKeys.ExitMessage, HandleExitMessage);
        }

        private void HandleExitMessage(ChatMessage keyedMessage, int totalSize)
        {
            if (null == keyedMessage) return;
            RemoveClient(keyedMessage.ClientId);
        }

        protected string GetNickName(ChatMessage message)
        {
            return Encoding.Unicode.GetString(message.Data);
        }

        protected void EncodeNickName(ChatMessage message, string nickName)
        {
            message.Data = Encoding.Unicode.GetBytes(nickName);
        }

        public virtual string GetNickName(Guid clientId)
        {
            string nick;
            nickLock.EnterReadLock();
            try
            {
                if (!clientNicks.TryGetValue(clientId, out nick))
                    nick = string.Empty;
            }
            finally { nickLock.ExitReadLock(); }
            return nick;
        }

        protected bool NickExists(string nick)
        {
            bool result;
            nickLock.EnterReadLock();
            try
            {
                result = clientNicks.Values.Any(x => string.Equals(nick, x, StringComparison.CurrentCultureIgnoreCase));
            }
            finally { nickLock.ExitReadLock(); }
            return result;
        }

        protected void RemoveClient(Guid clientId)
        {
            nickLock.EnterWriteLock();
            try
            {
                clientNicks.Remove(clientId);
            }
            finally { nickLock.ExitWriteLock(); }
        }

        protected void SetNick(Guid clientId, string nickName)
        {
            nickLock.EnterWriteLock();
            try
            {
                clientNicks.Add(clientId, nickName);
            }
            finally { nickLock.ExitWriteLock(); }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~NickManagerBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            subscriptionManager.Unsubscribe(MessageKeys.ExitMessage, HandleExitMessage);
        }
    }
}