using System.Net;

namespace iLynx.Networking.Interfaces
{
    public interface IConnectionStubBuilder<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        /// <summary>
        /// Builds a connection up using this builder
        /// <para/>
        /// For TCP connections the specified endpoint is the REMOTE endpoint.
        /// <para/>
        /// For UDP connections the specified endpoint is the LOCAL endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        IConnectionStub<TMessage, TMessageKey> Build(EndPoint endpoint);
    }
}