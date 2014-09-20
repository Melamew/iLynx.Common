using System;
using System.Net;
using System.Net.Sockets;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.TCP
{
    public abstract class SocketListenerBase : IListener
    {
        private Socket localSocket;

        protected SocketListenerBase()
        {
            localSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void BindTo(EndPoint localEndpoint)
        {
            if (IsBound) throw new InvalidOperationException("This listener is already bound");
            localSocket.Bind(localEndpoint);
            localSocket.Listen(5);
        }

        protected virtual Socket AcceptSocket()
        {
            if (null == localSocket) throw new InvalidOperationException("This listener is not yet bound - call BindTo in order to bind it to a local address");
            try
            {
                var socket = localSocket.Accept();
                return socket;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsBound { get { return null != localSocket && localSocket.IsBound; } }

        public void Close()
        {
            if (null == localSocket) return;
            try
            {
                localSocket.Close();
                localSocket.Dispose();
                localSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            }
            catch (NullReferenceException) { }
        }
    }
}