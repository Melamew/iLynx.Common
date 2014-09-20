using System.Collections.Generic;

namespace iLynx.Networking.Cryptography
{
    public class AlgorithmComparer : IEqualityComparer<IAlgorithmDescriptor>
    {
        public bool Equals(IAlgorithmDescriptor x, IAlgorithmDescriptor y)
        {
            return x.KeySize == y.KeySize && x.AlgorithmIdentifier == y.AlgorithmIdentifier
                   && x.Strength == y.Strength;
        }

        public int GetHashCode(IAlgorithmDescriptor obj)
        {
            unchecked
            {
                var hashCode = obj.KeySize;
                hashCode = (hashCode * 397) ^ (obj.AlgorithmIdentifier != null ? obj.AlgorithmIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Strength;
                return hashCode;
            }
        }
    }
}