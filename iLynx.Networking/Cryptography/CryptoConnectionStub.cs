using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

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
            Trace.WriteLine(string.Format("TX: Computed Padding: {0}, Final Length: {1}", paddingSize, final.Length));
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
            Trace.WriteLine(string.Format("RX: Padding was: {0}, Final Length: {1}", finalSize - dataLength, finalSize));
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

    public class ManualCryptoConnectionStub<TMessage, TMessageKey> : ICryptoConnectionStub<TMessage, TMessageKey>
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
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public ManualCryptoConnectionStub(ISerializer<TMessage> serializer, Socket socket, ILinkNegotiator linkNegotiator, ITimerService timerService, int pollingInterval = 1000)
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

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            decryptor.Dispose();
            encryptor.Dispose();
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Dispose();
            }
            catch (NullReferenceException) { }
            baseStream.Dispose();
            linkNegotiator.Dispose();
            decryptor = null;
            encryptor = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public bool NegotiateTransportKeys()
        {
            if (!IsOpen) throw new InvalidOperationException("This connection is not open");
            return linkNegotiator.SetupConnection(baseStream, out decryptor, out encryptor, out blockSize);
        }

        public int Write(TMessage message)
        {
            int totalSize;
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(message, memoryStream);
                var data = memoryStream.ToArray();
                WriteBytes(data, blockSize, reasonablySecurePrng, baseStream, encryptor, out totalSize);
            }
            return totalSize;
        }

        private static void WriteBytes(byte[] data, int blockSize, RandomNumberGenerator reasonablySecurePrng, Stream baseStream, ICryptoTransform encryptor, out int totalLength)
        {
            var dataLength = data.Length;
            Trace.WriteLine("TX: " + data.ToString(":"));
            var inputLength = dataLength + 4;
            var blocks = inputLength / blockSize;
            totalLength = blocks * blockSize;
            if (0 != inputLength % blockSize)
            {
                if (inputLength > blockSize)
                {
                    ++blocks;
                    totalLength += blockSize;
                }
                var dataPlusPadding = totalLength - 4;
                //Array.Resize(ref data, dataPlusPadding);
                var temp = data;
                data = new byte[dataPlusPadding];
                Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
                var paddingSize = dataPlusPadding - dataLength;
                var paddingOffset = dataLength;
                var paddingBytes = new byte[paddingSize];
                reasonablySecurePrng.GetBytes(paddingBytes);
                Buffer.BlockCopy(paddingBytes, 0, data, paddingOffset, paddingSize);
            }

            var finalData = new byte[totalLength];
            var firstBlock = new byte[blockSize];
            var length = Serializer.SingletonBitConverter.GetBytes(dataLength);
            Buffer.BlockCopy(length, 0, firstBlock, 0, 4);
            Buffer.BlockCopy(data, 0, firstBlock, 4, blockSize - 4);
            encryptor.TransformBlock(firstBlock, 0, blockSize, finalData, 0);
            for (var block = 1; block < blocks; ++block)
            {
                var offset = block * blockSize;
                encryptor.TransformBlock(data, offset - 4, blockSize, finalData, offset);
            }
            baseStream.Write(finalData, 0, finalData.Length);
            Trace.WriteLine(string.Format("Wrote {0} blocks", blocks));
        }

        public TMessage ReadNext(out int totalSize)
        {
            var bytes = ReadBytes(baseStream, blockSize, decryptor, out totalSize);
            using (var memoryStream = new MemoryStream(bytes))
                return serializer.Deserialize(memoryStream);
        }

        private static byte[] ReadBytes(Stream sourceStream, int blockSize, ICryptoTransform decryptor, out int totalSize)
        {
            var block = new byte[blockSize];
            var read = sourceStream.FillRead(block);
            totalSize = read;
            if (read != blockSize) throw new InvalidDataException();
            var decrypted = new byte[blockSize];
            decryptor.TransformBlock(block, 0, block.Length, decrypted, 0);
            var dataLength = Serializer.SingletonBitConverter.ToInt32(decrypted);
            var final = new byte[dataLength];
            Trace.WriteLine(string.Format("First Block: {0}", decrypted.ToString(":")));
            Buffer.BlockCopy(decrypted, 4, final, 0, blockSize - 4);
            var dataRead = blockSize - 4;
            var blocksRead = 1;
            var chunkSize = blockSize;
            while (dataRead < dataLength)
            {
                read = sourceStream.FillRead(block);
                totalSize += read;
                if (dataRead + blockSize > dataLength)
                    chunkSize = dataLength - dataRead;
                try
                {
                    decrypted = new byte[blockSize];
                    decryptor.TransformBlock(block, 0, blockSize, decrypted, 0);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
                Buffer.BlockCopy(decrypted, 0, final, dataRead, chunkSize);
                ++blocksRead;
                dataRead += chunkSize;
            }
            Trace.WriteLine(string.Format("Read {0} blocks", blocksRead));
            Trace.WriteLine("RX: " + final.ToString(":"));
            return final;
            //var read = sourceStream.FillRead(readBlock);
            //if (read != blockSize) throw new InvalidDataException();
            //var inputData = new byte[blockSize];
            //decryptor.TransformBlock(readBlock, 0, blockSize, inputData, 0);
            //var dataLength = Serializer.SingletonBitConverter.ToInt32(inputData);
            //if (0 > dataLength) throw new InvalidDataException();
            //var remainingData = dataLength - (blockSize - 4);
            //var blocks = dataLength / blockSize;
            //if (0 != remainingData % blockSize)
            //    ++blocks;
            //var result = new byte[(blockSize * blocks) - 4];
            //Buffer.BlockCopy(inputData, 4, result, 0, blockSize - 4);
            //for (var block = 1; block < blocks; ++block)
            //{
            //    read += sourceStream.FillRead(readBlock);
            //    decryptor.TransformBlock(readBlock, 0, blockSize, result, block * blockSize);
            //}
            //var data = new byte[dataLength];
            //Buffer.BlockCopy(result, 0, data, 0, dataLength);
            //totalSize = blocks * blockSize;
            //return result;
        }

        public bool IsOpen { get { return baseStream.CanRead && baseStream.CanWrite; } }
        public bool CanRead { get { return baseStream.CanRead; } }
        public bool CanWrite { get { return baseStream.CanWrite; } }
    }
}