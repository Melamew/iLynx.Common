using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IAuthenticationHandler<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        bool Authenticate(IConnectionStub<TMessage, TKey> connection);

        /// <summary>
        /// Gets a subjective value that indicates how 'strong' this handler is.
        /// <para/>
        /// Ie. Username / Password should have a greater strength than a simple pre-shared secret.
        /// </summary>
        int Strength { get; }
    }
}