using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using iLynx.Common;
using iLynx.Common.Serialization;
using ProtoBuf;
using Serializer = ProtoBuf.Serializer;

namespace iLynx.Networking.Cryptography
{
    public class KeyExchangeLinkNegotiator : ILinkNegotiator
    {
        private readonly IAlgorithmContainer<IKeyExchangeAlgorithmDescriptor> keyExchangeBuilder;
        private readonly IAlgorithmContainer<ISymmetricAlgorithmDescriptor> transportAlgorithmBuilder;
        private readonly IBitConverter bitConverter = new BigEndianBitConverter();

        public KeyExchangeLinkNegotiator(IAlgorithmContainer<IKeyExchangeAlgorithmDescriptor> keyExchangeBuilder,
                                         IAlgorithmContainer<ISymmetricAlgorithmDescriptor> transportAlgorithmBuilder)
        {
            this.keyExchangeBuilder = Guard.IsNull(() => keyExchangeBuilder);
            this.transportAlgorithmBuilder = Guard.IsNull(() => transportAlgorithmBuilder);
        }

        public bool SetupConnection(Stream baseStream, out Stream reader, out Stream writer, out int blockSize)
        {
            var exchangeAlgorithm = GetKeyExchangeAlgorithm(baseStream);
            var transportAlgorithm = GetStrongestDescriptor(baseStream, transportAlgorithmBuilder, exchangeAlgorithm);
            if (null == transportAlgorithm) throw new InvalidOperationException();
            ExchangeKeys(baseStream, transportAlgorithm, exchangeAlgorithm, out reader, out writer);
            blockSize = transportAlgorithm.BlockSize / 8;
            return true;
        }

        private void ExchangeKeys(Stream baseStream, ISymmetricAlgorithmDescriptor transportAlgorithmDescriptor, IKeyExchangeAlgorithm keyExchangeAlgorithm, out Stream inputStream, out Stream outputStream)
        {
            var encryptor = transportAlgorithmDescriptor.Build();
            encryptor.GenerateKey();
            encryptor.GenerateIV();
            var keyPackage = new KeyPackage
            {
                Key = encryptor.Key,
                InitializationVector = encryptor.IV,
            };
            byte[] keyBuffer;
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, keyPackage);
                keyBuffer = keyExchangeAlgorithm.Encrypt(memoryStream.ToArray());
            }
            var buffer = bitConverter.GetBytes(keyBuffer.Length);
            baseStream.Write(buffer, 0, buffer.Length);
            baseStream.Write(keyBuffer, 0, keyBuffer.Length);
            buffer = new byte[sizeof(int)];
            baseStream.Read(buffer, 0, buffer.Length);
            var length = bitConverter.ToInt32(buffer);
            if (0 > length) throw new InvalidDataException();
            buffer = new byte[length];
            baseStream.Read(buffer, 0, buffer.Length);
            buffer = keyExchangeAlgorithm.Decrypt(buffer);
            var decryptor = transportAlgorithmDescriptor.Build();
            KeyPackage remoteKeyPackage;
            using (var memoryStream = new MemoryStream(buffer))
                remoteKeyPackage = Serializer.Deserialize<KeyPackage>(memoryStream);
            outputStream = new CryptoStream(baseStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            decryptor.Key = remoteKeyPackage.Key;
            decryptor.IV = remoteKeyPackage.InitializationVector;
            inputStream = new CryptoStream(baseStream, decryptor.CreateDecryptor(), CryptoStreamMode.Read);
        }

        private IKeyExchangeAlgorithm GetKeyExchangeAlgorithm(Stream baseStream)
        {
            var strongestCommon = GetStrongestDescriptor(baseStream, keyExchangeBuilder, new PreNegotiationAlgorithm());
            var algorithm = strongestCommon.Build();
            var pubKey = algorithm.GetPublicKey();
            var buffer = bitConverter.GetBytes(pubKey.Length);
            baseStream.Write(buffer, 0, buffer.Length);
            baseStream.Write(pubKey, 0, pubKey.Length);
            buffer = new byte[sizeof (int)];
            baseStream.Read(buffer, 0, buffer.Length);
            var length = bitConverter.ToInt32(buffer);
            if (0 > length) throw new InvalidDataException();
            buffer = new byte[length];
            baseStream.Read(buffer, 0, buffer.Length);
            algorithm.SetPublicKey(buffer);
            return algorithm;
        }

        private void WriteAlgorithms<T>(Stream target, T[] algorithms, IKeyExchangeAlgorithm encryptionAlgorithm) where T : IAlgorithmDescriptor
        {
            byte[] buffer;
            using (var outputStream = new MemoryStream())
            {
                buffer = bitConverter.GetBytes(algorithms.Length);
                outputStream.Write(buffer, 0, buffer.Length);
                foreach (var algorithm in algorithms.Select(MakePackage))
                    Serializer.Serialize(outputStream, algorithm);
                buffer = encryptionAlgorithm.Encrypt(outputStream.ToArray());
            }
            var lengthBytes = bitConverter.GetBytes(buffer.Length);
            target.Write(lengthBytes, 0, lengthBytes.Length);
            target.Write(buffer, 0, buffer.Length);
        }

        private IEnumerable<IAlgorithmDescriptor> ReadRemoteAlgorithms(Stream source, IKeyExchangeAlgorithm encryptionAlgorithm)
        {
            var buffer = new byte[sizeof(int)];
            source.Read(buffer, 0, buffer.Length);
            var length = bitConverter.ToInt32(buffer);
            if (0 > length) throw new InvalidDataException();
            buffer = new byte[length];
            source.Read(buffer, 0, buffer.Length);
            buffer = encryptionAlgorithm.Decrypt(buffer);
            var result = new List<IAlgorithmDescriptor>();
            using (var outputStream = new MemoryStream(buffer))
            {
                var lengthBuffer = new byte[sizeof(int)];
                outputStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                var algorithmCount = bitConverter.ToInt32(lengthBuffer);
                for (var i = 0; i < algorithmCount; ++i)
                    result.Add(Serializer.Deserialize<AlgorithmDescriptorPackage>(outputStream));
            }
            return result;
        }

        private T GetStrongestDescriptor<T>(Stream baseStream, IAlgorithmContainer<T> container, IKeyExchangeAlgorithm exchangeAlgorithm) where T : IAlgorithmDescriptor
        {
            var localAlgorithms = container.SupportedAlgorithms.ToArray();
            WriteAlgorithms(baseStream, localAlgorithms, exchangeAlgorithm);
            var remoteAlgorithms = ReadRemoteAlgorithms(baseStream, exchangeAlgorithm);
            var strongestCommon = GetStrongestCommonDescriptor(localAlgorithms.Cast<IAlgorithmDescriptor>(), remoteAlgorithms);
            if (null == strongestCommon) throw new InvalidOperationException("The remote endpoint does not support any key exchange algorithms that the local endpoint supports");
            return (T)strongestCommon;
        }

        private static IAlgorithmDescriptor GetStrongestCommonDescriptor(IEnumerable<IAlgorithmDescriptor> left,
            IEnumerable<IAlgorithmDescriptor> right)
        {
            return left.Intersect(right, new AlgorithmComparer()).OrderByDescending(x => x.Strength).FirstOrDefault();
        }

        private static AlgorithmDescriptorPackage MakePackage<T>(T descriptor) where T : IAlgorithmDescriptor
        {
            return new AlgorithmDescriptorPackage
            {
                AlgorithmIdentifier = descriptor.AlgorithmIdentifier,
                KeySize = descriptor.KeySize,
                Strength = descriptor.Strength
            };
        }

        private class PreNegotiationAlgorithm : IKeyExchangeAlgorithm
        {
            public byte[] Encrypt(byte[] data)
            {
                return data;
            }

            public byte[] Decrypt(byte[] data)
            {
                return data;
            }

            public byte[] GetPublicKey()
            {
                throw new NotSupportedException();
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void GenerateKeys()
            {
                throw new NotSupportedException();
            }

            public void SetPublicKey(byte[] data)
            {
                throw new NotSupportedException();
            }
        }

        [ProtoContract]
        private class AlgorithmDescriptorPackage : IAlgorithmDescriptor
        {
            private bool Equals(IAlgorithmDescriptor other)
            {
                return KeySize == other.KeySize && string.Equals(AlgorithmIdentifier, other.AlgorithmIdentifier) && Strength == other.Strength;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = KeySize;
                    hashCode = (hashCode * 397) ^ (AlgorithmIdentifier != null ? AlgorithmIdentifier.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Strength;
                    return hashCode;
                }
            }

            [ProtoMember(1)]
            public int KeySize { get; set; }

            [ProtoMember(2)]
            public string AlgorithmIdentifier { get; set; }

            [ProtoMember(3)]
            public int Strength { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((AlgorithmDescriptorPackage)obj);
            }
        }

        [ProtoContract]
        private class KeyPackage
        {
            [ProtoMember(1)]
            public byte[] Key { get; set; }

            [ProtoMember(2)]
            public byte[] InitializationVector { get; set; }
        }

        public void Dispose()
        {

        }
    }
}