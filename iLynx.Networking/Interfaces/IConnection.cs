using System;
using System.Net;
using System.Threading.Tasks;
using iLynx.PubSub;

namespace iLynx.Networking.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConnection<TMessage, TMessageKey> : IKeyedSubscriptionManager<TMessageKey, MessageReceivedHandler<TMessage, TMessageKey>> where TMessage : IKeyedMessage<TMessageKey>
    {
        /// <summary>
        /// Attempts to connect to the specified endpoint
        /// </summary>
        /// <param name="endPoint"></param>
        /// <exception cref="System.NotSupportedException">Thrown if this connection does not support the specified endpoint type</exception>
        void Connect(EndPoint endPoint);

        /// <summary>
        /// Attempts to disconnect from the current endpoint.
        /// <para/>
        /// For a connection that is already connected, this method should not do anything.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets a value indicating whether or not this connection is currently connected to an endpoint.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Sends the specified keyedMessage and returns the total number of bytes written to the underlying transport layer.
        /// </summary>
        /// <param name="keyedMessage"></param>
        /// <returns></returns>
        int Send(TMessage keyedMessage);

        /// <summary>
        /// Asynchronously sends the specified keyedMessage
        /// </summary>
        /// <param name="keyedMessage"></param>
        /// <returns></returns>
        Task<int> SendAsync(TMessage keyedMessage);

        /// <summary>
        /// Raised when the connection is first established
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Raised when the connection is closed
        /// </summary>
        event EventHandler Disconnected;
    }
}