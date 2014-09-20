using System;

namespace iLynx.Networking.Interfaces
{
    public interface IConnectionStub<T, TMessageKey> : IDisposable where T : IKeyedMessage<TMessageKey>
    {
        int Write(T message);
        T ReadNext(out int totalSize);
        bool IsOpen { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
    }
}
