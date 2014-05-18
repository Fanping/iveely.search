using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Iveely.General.Communication
{
    public class TcpServer
    {
        private Thread Worker;

        public BlockingCollection<KeyValuePair<ServerConnection, Packet>> RecievedPackets;
        public CancellationTokenSource ShutdownTokenSource { get; private set; }

        public readonly ConcurrentDictionary<ServerConnection, ServerConnection> ServerConnections = new ConcurrentDictionary<ServerConnection, ServerConnection>();
        public readonly ConcurrentQueue<KeyValuePair<DateTime, Exception>> Errors = new ConcurrentQueue<KeyValuePair<DateTime, Exception>>();

        public int Port { get; private set; }

        public long BytesReceive { get; internal set; }
        public long BytesSent { get; internal set; }

        public TcpServer(int port = 7182)
        {
            Port = port;
        }

        public void Start(int boundedCapacity = 64)
        {
            Stop();

            RecievedPackets = new BlockingCollection<KeyValuePair<ServerConnection, Packet>>(boundedCapacity);
            ServerConnections.Clear();

            ShutdownTokenSource = new CancellationTokenSource();

            Worker = new Thread(DoWork);
            Worker.Start();
        }

        public void Stop()
        {
            if (!IsWorking)
                return;

            if (ShutdownTokenSource != null)
                ShutdownTokenSource.Cancel(false);

            DisconnectConnections();

            Thread thread = Worker;
            if (thread != null)
            {
                if (!thread.Join(5000))
                    thread.Abort();
            }
        }

        public bool IsWorking
        {
            get { return Worker != null; }
        }

        private void DoWork()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Start();

                while (!ShutdownTokenSource.Token.IsCancellationRequested)
                {
                    if (listener.Pending())
                    {
                        try
                        {
                            TcpClient client = listener.AcceptTcpClient();
                            ServerConnection serverConnection = new ServerConnection(this, client);
                            serverConnection.Connect();
                        }
                        catch (Exception exc)
                        {
                            LogError(exc);
                        }
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();

                Worker = null;
            }
        }

        internal void LogError(Exception exc)
        {
            while (Errors.Count > 100)
            {
                KeyValuePair<DateTime, Exception> err;
                Errors.TryDequeue(out err);
            }

            Errors.Enqueue(new KeyValuePair<DateTime, Exception>(DateTime.Now, exc));
        }

        private void DisconnectConnections()
        {
            foreach (var connection in ServerConnections)
                connection.Key.Disconnect();
        }
    }
}
