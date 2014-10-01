using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public class CryptoConnectionStub<TMessage, TMessageKey> : ICryptoConnectionStub<TMessage, TMessageKey>
        where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly int pollingInterval;
        private readonly ITimerService timerService;
        private readonly Socket socket;
        private readonly ILinkNegotiator linkNegotiator;
        private readonly ISerializer<TMessage> serializer;
        private readonly Stream baseStream;
        private bool isDisposed;
        private readonly int connectionTimer;
        private readonly RandomNumberGenerator reasonablySecurePrng = RandomNumberGenerator.Create();
        private int blockSize = -1;
        private Stream outputStream;
        private Stream inputStream;

        public CryptoConnectionStub(ISerializer<TMessage> serializer, Socket socket, ILinkNegotiator linkNegotiator, ITimerService timerService, int pollingInterval = 1000)
        {
            this.pollingInterval = pollingInterval;
            this.timerService = Guard.IsNull(() => timerService);
            this.socket = Guard.IsNull(() => socket);
            this.linkNegotiator = Guard.IsNull(() => linkNegotiator);
            this.serializer = Guard.IsNull(() => serializer);
            baseStream = new NetworkStream(socket);
            connectionTimer = this.timerService.StartNew(OnCheckConnectionStatus, pollingInterval, Timeout.Infinite);
        }

        private void OnCheckConnectionStatus()
        {
            if (socket.IsConnected())
                timerService.Change(connectionTimer, pollingInterval, Timeout.Infinite);
            else
                Dispose();
        }

        public bool NegotiateTransportKeys()
        {
            if (!IsOpen) throw new InvalidOperationException("This connection is not open");
            return linkNegotiator.SetupConnection(baseStream, out inputStream, out outputStream, out blockSize);
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            try { outputStream.Close(); }
            catch (IOException) { }
            outputStream.Dispose();
            try { inputStream.Close(); }
            catch (IOException) { }
            inputStream.Dispose();
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Dispose();
            }
            catch (NullReferenceException) { }
            baseStream.Dispose();
            linkNegotiator.Dispose();
            outputStream = null;
            inputStream = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public int Write(TMessage message)
        {
            Guard.IsNull(() => message);
            int size;
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(message, memoryStream);
                var data = memoryStream.ToArray();
                size = WriteBlocks(data, blockSize, reasonablySecurePrng, outputStream);
                //outputStream.Flush();
            }
            return size;
        }

        private static int WriteBlocks(byte[] data, int blockSize, RandomNumberGenerator reasonablySecurePrng, Stream target)
        {
            var paddingSize = (blockSize - ((data.Length + sizeof(int)) % blockSize));
            var rnd = new byte[paddingSize]; // Padding
            reasonablySecurePrng.GetBytes(rnd);
            var lengthField = Serializer.SingletonBitConverter.GetBytes(data.Length);
            var final = new byte[lengthField.Length + data.Length + rnd.Length];
            Buffer.BlockCopy(lengthField, 0, final, 0, lengthField.Length);
            Buffer.BlockCopy(data, 0, final, lengthField.Length, data.Length);
            Buffer.BlockCopy(rnd, 0, final, lengthField.Length + data.Length, rnd.Length);
            if (final.Length % blockSize != 0)
                Trace.WriteLine("Wat?");
            target.Write(final, 0, final.Length);
            return lengthField.Length + data.Length + rnd.Length;
        }

        private static byte[] ReadBlocks(Stream source, int blockSize, out int finalSize)
        {
            // Read an entire block first
            var buffer = new byte[blockSize];
            source.Read(buffer, 0, buffer.Length);
            // Decode the length field
            var length = Serializer.SingletonBitConverter.ToInt32(buffer);
            if (0 > length) throw new InvalidDataException();
            var result = new byte[length];
            //var padding = (blockSize - (read%blockSize)) - sizeof (int);
            var gotData = 0;
            var block = 0;
            var dataLength = length < blockSize - sizeof(int) ? length : blockSize - sizeof(int);
            Buffer.BlockCopy(buffer, sizeof(int), result, block * blockSize, dataLength);
            gotData += dataLength;
            ++block;
            while (gotData < length)
            {
                source.Read(buffer, 0, buffer.Length);
                var toCopy = length - gotData < blockSize ? length - gotData : blockSize;
                Buffer.BlockCopy(buffer, 0, result, gotData, toCopy);
                gotData += toCopy;
                ++block;
            }
            finalSize = block * blockSize;
            return result;
        }

        public TMessage ReadNext(out int totalSize)
        {
            try
            {
                var buffer = ReadBlocks(inputStream, blockSize, out totalSize);
                TMessage result;
                using (var memoryStream = new MemoryStream(buffer))
                {
                    result = serializer.Deserialize(memoryStream);
                    totalSize = buffer.Length;
                }
                return result;
            }
            catch (Exception e)
            {
                RuntimeCommon.DefaultLogger.Log(LogLevel.Error, this, string.Format("Caught exception {0} when trying to read data", e));
                totalSize = -1;
                return default(TMessage);
            }
        }

        public bool IsOpen { get { return baseStream.CanRead && baseStream.CanWrite; } }
        public bool CanRead { get { return inputStream.CanRead; } }
        public bool CanWrite { get { return outputStream.CanWrite; } }
    }
}