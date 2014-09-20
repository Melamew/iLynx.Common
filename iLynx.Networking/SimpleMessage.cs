using iLynx.Networking.Interfaces;

namespace iLynx.Networking
{
    public class SimpleMessage<TKey> : IKeyedMessage<TKey>
    {
        public SimpleMessage(TKey key)
        {
            Key = key;
        }

        public SimpleMessage(TKey key, byte[] data)
        {
            Key = key;
            Data = data;
        }

        public TKey Key { get; private set; }
        public byte[] Data { get; private set; }
    }
}