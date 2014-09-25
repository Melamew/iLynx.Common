namespace iLynx.Networking.Interfaces
{
    public interface IKeyedPayloadMessage<out TKey> : IKeyedMessage<TKey>
    {
        byte[] Payload { get; }
    }
}