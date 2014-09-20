using System.Net;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public interface ICryptoConnectionStubBuilder<TMessage, TMessageKey> : IConnectionStubBuilder<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        new ICryptoConnectionStub<TMessage, TMessageKey> Build(EndPoint remoteEndpoint);
    }
}
