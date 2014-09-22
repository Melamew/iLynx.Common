using System;
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

        /// <summary>
        /// Gets a value that can be used to identify this authenticator globally
        /// <para/>
        /// Note that this should be the same for both client and server authenticators of the same authentication type.
        /// </summary>
        Guid AuthenticatorId { get; }
    }
}