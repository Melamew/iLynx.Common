using System.Net;

namespace iLynx.Networking.Interfaces
{
    public interface IKeyedDatagramMessage<out TKey> : IKeyedMessage<TKey>
    {
        EndPoint RemoteEndPoint { get; }
    }
}