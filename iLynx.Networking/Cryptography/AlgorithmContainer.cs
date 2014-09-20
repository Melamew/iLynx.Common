using System.Collections.Generic;
using System.Linq;
using iLynx.Common;

namespace iLynx.Networking.Cryptography
{
    public class AlgorithmContainer<TAlgorithmType> : IAlgorithmContainer<TAlgorithmType> where TAlgorithmType : IAlgorithmDescriptor
    {
        private readonly Dictionary<string, TAlgorithmType> builderTable = new Dictionary<string, TAlgorithmType>();
        public IEnumerable<TAlgorithmType> SupportedAlgorithms
        {
            get
            {
                return builderTable.Select(x => x.Value);
            }
        }

        public void AddAlgorithm(TAlgorithmType descriptor)
        {
            Guard.IsNull(() => descriptor);
            var identifier = descriptor.AlgorithmIdentifier;
            if (builderTable.ContainsKey(identifier))
                builderTable[identifier] = descriptor;
            else
                builderTable.Add(identifier, descriptor);
        }

        public void RemoveAlgorithm(TAlgorithmType descriptor)
        {
            Guard.IsNull(() => descriptor);
            builderTable.Remove(descriptor.AlgorithmIdentifier);
        }
    }
}