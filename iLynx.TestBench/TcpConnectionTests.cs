using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;
using iLynx.Networking.Serialization;
using iLynx.Networking.TCP;
using iLynx.Serialization;

namespace iLynx.TestBench
{
    public class TcpConnectionTests
    {
        private readonly ConsoleMenu menu = new ConsoleMenu();

        public void Run()
        {
            menu.AddMenuItem('1', "Run a simple Server/Client test", RunClientServerTest);
            menu.AddMenuItem('2', "Run a test on the subscriber functionality of StubConnection", RunSubscriberTest);
            menu.AddMenuItem('3', "Run encrypted transport tests", RunCryptographyTest);
            menu.ShowMenu();
        }

        private void RunCryptographyTest()
        {
            var keyExchangeContainer = new AlgorithmContainer<IKeyExchangeAlgorithmDescriptor>();
            keyExchangeContainer.AddAlgorithm(new RsaDescriptor(512));

            var symmetricContainer = new AlgorithmContainer<ISymmetricAlgorithmDescriptor>();
            symmetricContainer.AddAlgorithm(new AesDescriptor(256));
            var timerService = new SingleThreadedTimerService();
            var serializer = new SimpleMessageSerializer<int>(new BinaryPrimitives.Int32Serializer());
            var keyExchangeNegotiator = new KeyExchangeLinkNegotiator(keyExchangeContainer, symmetricContainer);
            var listener =
                new CryptoConnectionStubListener<SimpleMessage<int>, int>(
                    serializer,
                    keyExchangeNegotiator,
                    timerService);
            RunClientSubscriber(
                new StubConnection<SimpleMessage<int>, int>(new ThreadManager(),
                    new CryptoConnectionStubBuilder<SimpleMessage<int>, int>(serializer, keyExchangeNegotiator, timerService)),
                listener, 5392);
        }

        private void RunSubscriberTest()
        {
            var listener = new TcpStubListener<SimpleMessage<int>, int>(new SimpleMessageSerializer<int>(), new SingleThreadedTimerService());
            const ushort port = 50124;
            var connection = new StubConnection<SimpleMessage<int>, int>(
                new ThreadManager(RuntimeCommon.DefaultLogger),
                new TcpStubBuilder<SimpleMessage<int>, int>(new SimpleMessageSerializer<int>(), new SingleThreadedTimerService()));
            RunClientSubscriber(connection, listener, port);
        }

        private static void TextMessageHandler(string side, SimpleMessage<int> message, int size)
        {
            Console.WriteLine(@"{0}: Received Message (Size: {3} Bytes), Type {1}, Contents: {2}", side, message.Key,
                Encoding.ASCII.GetString(message.Data), size);
        }

        private void RunClientSubscriber(IConnection<SimpleMessage<int>, int> connection, IConnectionStubListener<SimpleMessage<int>, int> listener, ushort port)
        {
            listener.BindTo(new IPEndPoint(IPAddress.Any, port));
            var listenerTask = Task.Run(() => RunListenerSubscriber(listener));
            var ready = false;
            connection.Subscribe(0, (message, size) => ready = true);
            connection.Subscribe(1, (message, size) => TextMessageHandler("Client", message, size));
            connection.Connect(new IPEndPoint(IPAddress.Loopback, port));
            while (!ready)
                Thread.CurrentThread.Join(10);

            const int messages = 500;
            Console.WriteLine(@"Sending {0} messages with random data", 5);
            var rnd = new Random();
            for (var i = 0; i < messages; ++i)
            {
                var data = new byte[rnd.Next(16, 256)];
                rnd.NextBytes(data);
                connection.Send(new SimpleMessage<int>(1, Encoding.ASCII.GetBytes(Convert.ToBase64String(data))));
            }
            Console.WriteLine(@"Client: Done, sending exit message");
            connection.Send(new SimpleMessage<int>(2));
            connection.Disconnect();
            Console.WriteLine(@"Waiting for listener thread to exit");
            listenerTask.Wait();
            Console.WriteLine(@"Listener thread has exited");
            Console.WriteLine();
            Console.WriteLine(@"Press any key to exit");
            Console.ReadKey();
        }

        private void RunListenerSubscriber(IConnectionStubListener<SimpleMessage<int>, int> listener)
        {
            var stub = listener.AcceptNext();
            Console.WriteLine(@"Server: Got Connection");
            var connection = new StubConnection<SimpleMessage<int>, int>(
                new ThreadManager(), stub);
            var messages = 0;
            connection.Subscribe(1, (message, size) =>
            {
                TextMessageHandler("Server", message, size);
                messages++;
            });
            var exit = false;
            connection.Subscribe(2, (message, size) =>
            {
                Console.WriteLine(@"Received exit Message");
                Console.WriteLine(@"Total messages received: {0}", messages);
                exit = true;
            });
            connection.Send(new SimpleMessage<int>(0));
            while (!exit)
            {
                Thread.CurrentThread.Join(10);
            }
            connection.Disconnect();
            listener.Close();
        }

        private void RunClientServerTest()
        {
            const ushort port = 5321;
            var listener = new TcpStubListener<SimpleMessage<int>, int>(new SimpleMessageSerializer<int>(), new SingleThreadedTimerService());
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            listener.BindTo(endPoint);
            var serverTask = Task.Run(() => AcceptSocket(listener));
            var connectionBuilder = new TcpStubBuilder<SimpleMessage<int>, int>(new SimpleMessageSerializer<int>(), new SingleThreadedTimerService());
            var ep = new IPEndPoint(IPAddress.Loopback, port);
            Console.WriteLine(@"Client: Building connection to {0}", ep);
            var connection = connectionBuilder.Build(ep);
            Console.WriteLine(@"Client: Sending Hello");
            connection.Write(new SimpleMessage<int>(0, Encoding.ASCII.GetBytes("Hello")));
            Console.WriteLine(@"Client: Reading response");
            int size;
            var response = connection.ReadNext(out size);
            if (null == response)
            {
                Console.WriteLine(@"Received null from connection");
                connection.Dispose();
                return;
            }
            Console.WriteLine(@"Client: Read {0} bytes", size);
            Console.WriteLine(@"Client: Got Response; contents: {0}", Encoding.ASCII.GetString(response.Data));
            Console.WriteLine(@"Client: Done, exiting");
            connection.Dispose();
            serverTask.Wait();
            Console.WriteLine();
            Console.WriteLine(@"Press any key to exit");
            Console.ReadKey();
        }

        private void AcceptSocket(IConnectionStubListener<SimpleMessage<int>, int> listener)
        {
            Console.WriteLine(@"Server: Listening for connection");
            var remoteConnection = listener.AcceptNext();
            Console.WriteLine(@"Server: Got connection, closing listener");
            int read;
            Console.WriteLine(@"Server: Waiting for message");
            var msg = remoteConnection.ReadNext(out read);
            if (null == msg)
            {
                Console.WriteLine(@"Received null from connection");
                remoteConnection.Dispose();
                return;
            }
            Console.WriteLine(@"Server: Read {0} bytes", read);
            Console.WriteLine(@"Server: Message received; contents: {0}", Encoding.ASCII.GetString(msg.Data));
            Console.WriteLine(@"Server: Sending response");
            remoteConnection.Write(new SimpleMessage<int>(0, Encoding.ASCII.GetBytes("Hello to you too")));
            Console.WriteLine(@"Server: Done, exiting");
            remoteConnection.Dispose();
            listener.Close();
        }
    }
}