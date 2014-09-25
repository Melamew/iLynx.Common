using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace iLynx.Networking.Cryptography
{
    public class RsaKeyExchangeAlgorithm : IKeyExchangeAlgorithm
    {
        private RSACryptoServiceProvider privateProvider;
        private RSACryptoServiceProvider publicProvider;
        private RSAParameters rsaParameters;
        private int maxInputBytes;
        private readonly int keySize = 3072;


        private static void GenerateRsaKeys(CspParameters parameters, int keySize, out RSACryptoServiceProvider provider)
        {
            parameters.Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseMachineKeyStore;
            provider = new RSACryptoServiceProvider(keySize, parameters);
        }

        private static CspParameters GenKeys(int keySize)
        {
            var parameters = new CspParameters();
            var provider = new RSACryptoServiceProvider();
            parameters.Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey;
            parameters.KeyNumber = (int)KeyNumber.Exchange;
            parameters.KeyContainerName = System.Reflection.Assembly.GetCallingAssembly().FullName;
            try
            {
                //RSAHelper: Attempting to open existing key container
                provider = new RSACryptoServiceProvider(parameters);
                var pa = provider.ExportParameters(false);
                if (pa.Modulus.Length * 8 == keySize) return parameters;
                //Found existing key, but not of the correct size
                provider.PersistKeyInCsp = false;
                provider.Clear();
                GenerateRsaKeys(parameters, keySize, out provider);
            }
            catch
            {
                //No existing Key Container was found in the machine keystore
                GenerateRsaKeys(parameters, keySize, out provider);
            }
            finally
            {
                provider.Clear();
                provider.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return parameters;
        }

        public RsaKeyExchangeAlgorithm(int keySize)
        {
            this.keySize = keySize;
            GenerateKeys();
        }

        public byte[] Encrypt(byte[] data)
        {
            var outputStream = new MemoryStream(rsaParameters.Modulus.Length * ((int)Math.Ceiling((double)data.Length / maxInputBytes)));
            Encrypt(data, outputStream);
            var ret = outputStream.ToArray();
            outputStream.Dispose();
            return ret;
        }

        public void Encrypt(byte[] data, Stream target)
        {
            if (null == publicProvider) throw new InvalidOperationException("No public key");
            var dataStream = new MemoryStream(data);
            var dataSlice = new byte[maxInputBytes];
            int read;
            while ((read = dataStream.Read(dataSlice, 0, maxInputBytes)) > 0)
            {
                if (read < maxInputBytes)
                {
                    var temp = new byte[read];
                    Array.Copy(dataSlice, temp, read);
                    dataSlice = temp;
                }
                var encryptedSlice = publicProvider.Encrypt(dataSlice, true);
                target.Write(encryptedSlice, 0, encryptedSlice.Length);
                dataSlice = new byte[maxInputBytes];
            }
            dataStream.Dispose();
        }

        public byte[] Decrypt(byte[] data)
        {
            var dataStream = new MemoryStream(data);
            var outputStream = new MemoryStream(maxInputBytes * (int)(Math.Ceiling((double)data.Length / rsaParameters.Modulus.Length)));
            var buffer = new byte[rsaParameters.Modulus.Length];
            var total = 0;
            while (dataStream.Read(buffer, 0, rsaParameters.Modulus.Length) == rsaParameters.Modulus.Length)
            {
                var decryptedSlice = privateProvider.Decrypt(buffer, true);
                if (decryptedSlice.Length != maxInputBytes)
                {
                    data = outputStream.GetBuffer();
                    var final = new byte[total + decryptedSlice.Length];
                    Array.Copy(data, final, total);
                    Array.Copy(decryptedSlice, 0, final, total, decryptedSlice.Length);
                    return final;
                }
                outputStream.Write(decryptedSlice, 0, decryptedSlice.Length);
                buffer = new byte[rsaParameters.Modulus.Length];
                total += decryptedSlice.Length;
            }
            data = outputStream.GetBuffer();
            return data;
        }

        public byte[] GetPublicKey()
        {
            return Encoding.ASCII.GetBytes(privateProvider.ToXmlString(false));
        }

        public void Reset()
        {
            GenerateKeys();
        }

        public void GenerateKeys()
        {
            var parameters = GenKeys(keySize);
            privateProvider = new RSACryptoServiceProvider(parameters);
            rsaParameters = privateProvider.ExportParameters(false);
        }

        private void SetMaxInputBytes()
        {
            maxInputBytes = ((publicProvider.KeySize - 384) / 8) + 6;
        }

        public void SetPublicKey(byte[] data)
        {
            var str = Encoding.ASCII.GetString(data);
            publicProvider = new RSACryptoServiceProvider();
            publicProvider.FromXmlString(str);
            SetMaxInputBytes();
        }
    }
}