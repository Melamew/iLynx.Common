using System.IO;

namespace iLynx.Common.Cryptography
{
    /// <summary>
    /// Basic specification for an encryption algorithm
    /// </summary>
    public interface IEncryptionAlgorithm
    {
        /// <summary>
        /// Destroys all sensitive information contained in this instance of the algorithm.
        /// <para/>
        /// Such as: Key, Initialization Vector (IV) and other state-relevant information.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Encrypts the specified array of data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypts the specified data with this algorithm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Creates a write-only stream that encrypts all data that is written to it and writes it to the specified underlying stream.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        Stream Encrypt(Stream inputStream);

        /// <summary>
        /// Creates a read-only stream that will decrypt all data that is read from it through the specified underlying stream.
        /// </summary>
        /// <param name="outputStream"></param>
        /// <returns></returns>
        Stream Decrypt(Stream outputStream);
    }
}
