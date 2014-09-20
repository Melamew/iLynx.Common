using System.Collections.Generic;

namespace iLynx.Networking.Cryptography
{
    public interface IAlgorithmContainer<TAlgorithm> where TAlgorithm : IAlgorithmDescriptor
    {
        IEnumerable<TAlgorithm> SupportedAlgorithms { get; }
        void AddAlgorithm(TAlgorithm descriptor);
        void RemoveAlgorithm(TAlgorithm descriptor);
    }
}