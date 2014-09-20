namespace iLynx.Networking.Cryptography
{
    public interface IAlgorithmDescriptor
    {
        int KeySize { get; }
        string AlgorithmIdentifier { get; }
        int Strength { get; }
    }
}