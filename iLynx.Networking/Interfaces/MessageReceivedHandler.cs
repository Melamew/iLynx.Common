namespace iLynx.Networking.Interfaces
{
    /// <summary>
    /// Used when a keyedMessage is received on an <see cref="IConnection{TMessage,TKey}"/>
    /// </summary>
    /// <param name="keyedMessage">The keyedMessage that was received</param>
    /// <param name="totalSize">The total size of the keyedMessage as read from the transport layer</param>
    public delegate void MessageReceivedHandler<in TMessage, in TKey>(TMessage keyedMessage, int totalSize)
        where TMessage : IKeyedMessage<TKey>;
}