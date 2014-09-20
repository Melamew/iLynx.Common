using System.Security.Cryptography.X509Certificates;

namespace iLynx.Networking
{
    public interface ITlsConnection<TMessage, TMessageKey> : IConnection<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        /// <summary>
        /// Sets the local (client side) certificate to the specified value.
        /// </summary>
        /// <param name="certificate"></param>
        void SetLocalCertificate(X509Certificate certificate);

        /// <summary>
        /// Gets or Sets a value indicating whether or not self-signed (Ie. unsecure) certificates are accepted from the server.
        /// </summary>
        bool AllowSelfSigned { get; set; }
    }
}