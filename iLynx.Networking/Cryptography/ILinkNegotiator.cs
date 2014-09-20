using System;
using System.IO;

namespace iLynx.Networking.Cryptography
{
    public interface ILinkNegotiator : IDisposable
    {
        bool SetupConnection(Stream baseStream, out Stream reader, out Stream writer, out int blockSize);
    }
}