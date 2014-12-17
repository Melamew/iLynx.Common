using System;
using System.Net.Sockets;
using System.Threading;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

namespace iLynx.Networking
{
    public class StreamedSocketConnectionStub<TMessage, TKey> : StreamConnectionStub<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        private readonly int pollingInterval;
        private readonly ITimerService timerService;
        private readonly int connectionTimer;
        private readonly Socket socket;

        public StreamedSocketConnectionStub(ISerializer<TMessage> serializer, Socket socket, ITimerService timerService, int pollingInterval = 10000)
            : base(serializer, new NetworkStream(socket))
        {
            this.pollingInterval = pollingInterval;
            this.timerService = Guard.IsNull(() => timerService);
            this.socket = Guard.IsNull(() => socket);
            connectionTimer = this.timerService.StartNew(OnCheckConnectionStatus, pollingInterval, Timeout.Infinite);
        }

        private void OnCheckConnectionStatus()
        {
            if (socket.IsConnected())
                timerService.Change(connectionTimer, pollingInterval, Timeout.Infinite);
            else
                Dispose();
        }

        public override int Write(TMessage message)
        {
            var result = base.Write(message);
            return result;
        }

        public override bool IsOpen
        {
            get { return socket.Connected; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Dispose();
            }
            catch (NullReferenceException)
            {
                // Apparently there's a bug in the .NET framework which may cause socket.Close to throw a NullReferenceException if called after socket.Shutdown
            }
            catch (ObjectDisposedException) { }
        }
    }
}