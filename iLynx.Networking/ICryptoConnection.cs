using System.Security.Cryptography;

namespace iLynx.Networking
{
    public interface ICryptoConnection<TMessage, TMessageKey> : IConnection<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        /// <summary>
        /// Sets the specified <see cref="AsymmetricAlgorithm"/> as the algorithm that is used for the initial key-exchange.
        /// </summary>
        /// <param name="algorithm"></param>
        void SetKeyExchangeAlgorithm(AsymmetricAlgorithm algorithm);

        /// <summary>
        /// Sets the specified <see cref="SymmetricAlgorithm"/> as the algorithm that is used after the key-exchange is completed.
        /// </summary>
        /// <param name="algorithm"></param>
        void SetTransportEncryption(SymmetricAlgorithm algorithm);
    }
}