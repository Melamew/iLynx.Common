using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IMessageHandler<in TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        void Handle(TMessage message, int messageSize);
    }
}