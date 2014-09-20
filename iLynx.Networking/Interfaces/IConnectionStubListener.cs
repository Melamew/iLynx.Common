namespace iLynx.Networking.Interfaces
{
    public interface IConnectionStubListener<TMessage, TMessageKey> : IListener where TMessage : IKeyedMessage<TMessageKey>
    {
        IConnectionStub<TMessage, TMessageKey> AcceptNext();
    }
}