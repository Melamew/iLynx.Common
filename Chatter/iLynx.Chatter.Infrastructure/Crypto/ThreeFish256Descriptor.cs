using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using iLynx.Networking.Cryptography;
using SkeinFish;

namespace iLynx.Chatter.Infrastructure.Crypto
{
    public abstract class ThreefishDescriptor : ISymmetricAlgorithmDescriptor
    {
        private readonly int keySize;
        private readonly string algorithmIdentifier;
        private readonly int strength;
        private readonly int blockSize;

        protected ThreefishDescriptor(int blockSize, int keySize)
        {
            this.blockSize = blockSize;
            this.keySize = keySize;
            algorithmIdentifier = string.Format("Threefish.{0}", keySize);
            strength = keySize*101;
        }

        #region Implementation of IAlgorithmDescriptor

        public int KeySize
        {
            get { return keySize; }
        }

        public string AlgorithmIdentifier
        {
            get { return algorithmIdentifier; }
        }

        public int Strength
        {
            get { return strength; }
        }

        #endregion

        #region Implementation of ISymmetricAlgorithmDescriptor

        public SymmetricAlgorithm Build()
        {
            return new Threefish
                   {
                       KeySize = keySize,
                       BlockSize = blockSize,
                       Mode = CipherMode.CBC,
                       Padding = PaddingMode.None,
                   };
        }

        public int BlockSize
        {
            get { return blockSize; }
        }

        #endregion
    }

    public class Threefish256Descriptor : ThreefishDescriptor
    {
        public Threefish256Descriptor() : base(256, 256) { }
    }

    public class Threefish512Descriptor : ThreefishDescriptor
    {
        public Threefish512Descriptor() : base(512, 512) { }
    }

    public class Threefish1024Descriptor : ThreefishDescriptor
    {
        public Threefish1024Descriptor() : base(1024, 1024) { }
    }
}
