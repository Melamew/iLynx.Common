using System.Security.Cryptography;

namespace iLynx.Networking.Cryptography
{
    public interface ISymmetricAlgorithmDescriptor : IAlgorithmDescriptor
    {
        SymmetricAlgorithm Build();
        int BlockSize { get; }
    }
}