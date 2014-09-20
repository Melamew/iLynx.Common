using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public interface ICryptoConnectionStub<TMessage, TMessageKey> : IConnectionStub<TMessage, TMessageKey>
        where TMessage : IKeyedMessage<TMessageKey>
    {
        bool NegotiateTransportKeys();
    }
}