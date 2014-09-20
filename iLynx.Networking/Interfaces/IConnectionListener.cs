namespace iLynx.Networking.Interfaces
{
    public interface IConnectionListener<TMessage, TMessageKey> : IListener where TMessage : IKeyedMessage<TMessageKey>
    {
        IConnection<TMessage, TMessageKey> AcceptNext();
    }
}