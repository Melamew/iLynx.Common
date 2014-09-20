namespace iLynx.Networking.Interfaces
{
    public interface IKeyedMessage<out TKey>
    {
        TKey Key { get; }
    }
}