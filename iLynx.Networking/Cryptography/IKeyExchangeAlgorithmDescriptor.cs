namespace iLynx.Networking.Cryptography
{
    public interface IKeyExchangeAlgorithmDescriptor : IAlgorithmDescriptor
    {
        IKeyExchangeAlgorithm Build();
    }
}