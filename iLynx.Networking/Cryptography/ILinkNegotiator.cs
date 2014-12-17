using System;
using System.IO;
using System.Security.Cryptography;

namespace iLynx.Networking.Cryptography
{
    public interface ILinkNegotiator : IDisposable
    {
        bool SetupConnection(Stream baseStream, out Stream reader, out Stream writer, out int blockSize);

        bool SetupConnection(Stream baseStream, out ICryptoTransform decryptor, out ICryptoTransform encryptor,
            out int blockSize);
    }
}