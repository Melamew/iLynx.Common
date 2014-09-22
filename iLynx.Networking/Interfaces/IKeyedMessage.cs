namespace iLynx.Networking.Interfaces
{
    public interface IKeyedMessage<out TKey>
    {
        TKey Key { get; }
    }

    public interface IKeyedPayloadMessage<out TKey> : IKeyedMessage<TKey>
    {
        byte[] Payload { get; }
    }
}