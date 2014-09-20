using System;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IClientMessage<out TKey> : IKeyedMessage<TKey>
    {
        Guid ClientId { get; set; }
    }
}