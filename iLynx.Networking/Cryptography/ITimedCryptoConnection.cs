using System;

namespace iLynx.Networking.Cryptography
{
    public interface ITimedCryptoConnection<TMessage, TMessageKey> : ICryptoConnection<TMessage, TMessageKey> where TMessage : ITimestampedKeyedMessage<TMessageKey>
    {
        /// <summary>
        /// Gets or Sets a value that indicates the maximum age for the transport key; after this period is passed, the connection will re-negotiate the transport key.
        /// </summary>
        TimeSpan MaxKeyAge { get; set; }
    }
}