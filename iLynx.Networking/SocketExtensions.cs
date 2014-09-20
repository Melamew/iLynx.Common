using System;
using System.Net;
using System.Net.Sockets;
using iLynx.Common;

namespace iLynx.Networking
{
    public static class SocketExtensions
    {
        public static int FillReceive(this Socket socket, byte[] target)
        {
            Guard.IsNull(() => target);
            var length = target.Length;
            var totalReceived = 0;
            while (totalReceived < length)
            {
                var buffer = new byte[length - totalReceived];
                var received = socket.Receive(buffer, SocketFlags.None);
                Buffer.BlockCopy(buffer, 0, target, totalReceived, received);
                totalReceived += received;
            }
            return totalReceived;
        }

        public static int FillReceiveFrom(this Socket socket, byte[] target, ref EndPoint remoteEndpoint)
        {
            Guard.IsNull(() => target);
            var length = target.Length;
            var totalReceived = 0;
            while (totalReceived < length)
            {
                var buffer = new byte[length - totalReceived];
                var received = socket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEndpoint);
                Buffer.BlockCopy(buffer, 0, target, totalReceived, received);
                totalReceived += received;
            }
            return totalReceived;
        }

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }
}