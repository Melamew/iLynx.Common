using System;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public interface ITimestampedKeyedMessage<out TKey> : IKeyedMessage<TKey>
    {
        DateTime TimeStampUtc { get; }
    }
}