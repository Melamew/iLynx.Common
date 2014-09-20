using System;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IClient<in TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        void Send(TMessage message);
        /// <summary>
        /// Closes this client without sending a disconnect command
        /// </summary>
        void Close();
        void Disconnect(TMessage exitMessage);
        event EventHandler<ClientDisconnectedEventArgs> Disconnected;
        long SentBytes { get; }
        long ReceivedBytes { get; }
        Guid ClientId { get; }
        bool IsConnected { get; }
    }
}