using System;
using System.IO;
using System.Security.Cryptography;

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

    ///// <summary>
    ///// 
    ///// </summary>
    //public class SymmetricEncryptionAlgorithm : IEncryptionAlgorithm
    //{
    //    private readonly SymmetricAlgorithm algorithm;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="algorithm"></param>
    //    public SymmetricEncryptionAlgorithm(SymmetricAlgorithm algorithm)
    //    {
    //        this.algorithm = Guard.IsNull(() => algorithm);
    //    }

    //    public void Destroy()
    //    {
    //        algorithm.Clear();
    //        algorithm.Dispose();
    //    }

    //    public void SetKey(byte[] key)
    //    {
            
    //    }

    //    public byte[] Encrypt(byte[] data)
    //    {
    //        var encryptor = algorithm.CreateEncryptor();
    //    }

    //    public byte[] Decrypt(byte[] data)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Stream Encrypt(Stream inputStream)
    //    {
    //        return new CryptoStream(inputStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
    //    }

    //    public Stream Decrypt(Stream outputStream)
    //    {
    //        return new CryptoStream(outputStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read);
    //    }
    //}
}
