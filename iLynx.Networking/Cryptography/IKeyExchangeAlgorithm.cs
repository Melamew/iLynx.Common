namespace iLynx.Networking.Cryptography
{
    public interface IKeyExchangeAlgorithm
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
        byte[] GetPublicKey();
        void Reset();
        void GenerateKeys();
        void SetPublicKey(byte[] data);
    }
}