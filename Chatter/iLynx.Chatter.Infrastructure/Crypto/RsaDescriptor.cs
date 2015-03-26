using System.Security.Cryptography;
using iLynx.Networking.Cryptography;

namespace iLynx.Chatter.Infrastructure.Crypto
{
    public class RsaDescriptor : IKeyExchangeAlgorithmDescriptor
    {
        public RsaDescriptor(int keySize)
        {
            KeySize = keySize;
        }

        public int KeySize { get; private set; }
        public string AlgorithmIdentifier { get { return typeof(RSACryptoServiceProvider).FullName; } }
        public int Strength { get { return KeySize * 1000; } }
        public IKeyExchangeAlgorithm Build()
        {
            return new RsaKeyExchangeAlgorithm(KeySize);
        }
    }
}