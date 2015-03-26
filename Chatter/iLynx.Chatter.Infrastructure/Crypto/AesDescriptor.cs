using System.Security.Cryptography;
using iLynx.Networking.Cryptography;

namespace iLynx.Chatter.Infrastructure.Crypto
{
    public class AesDescriptor : ISymmetricAlgorithmDescriptor
    {
        public AesDescriptor(int keySize)
        {
            KeySize = keySize;
        }

        public int KeySize { get; private set; }
        public string AlgorithmIdentifier { get { return typeof(AesCryptoServiceProvider).FullName; } }
        public int Strength { get { return KeySize * 100; } }
        public int BlockSize { get { return 128; } }
        
        public SymmetricAlgorithm Build()
        {
            var rijndael = Rijndael.Create();
            rijndael.Mode = CipherMode.CBC;
            rijndael.BlockSize = 128;
            rijndael.KeySize = KeySize;
            rijndael.Padding = PaddingMode.None;
            return rijndael;
        }
    }
}