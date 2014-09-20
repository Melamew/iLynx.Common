using System.Net;

namespace iLynx.Networking.Interfaces
{
    public interface IListener
    {
        void BindTo(EndPoint localEndpoint);
        bool IsBound { get; }
        void Close();
    }
    public interface IConnectionStubListener<TMessage, TMessageKey> : IListener where TMessage : IKeyedMessage<TMessageKey>
    {
        IConnectionStub<TMessage, TMessageKey> AcceptNext();
    }

    public interface IConnectionListener<TMessage, TMessageKey> : IListener where TMessage : IKeyedMessage<TMessageKey>
    {
        IConnection<TMessage, TMessageKey> AcceptNext();
    }
}