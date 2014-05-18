using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Iveely.General.Communication
{
    public class ServerConnection
    {
        private Thread Receiver;
        private Thread Sender;
        private volatile bool Shutdown = false;

        public BlockingCollection<Packet> PendingPackets;

        public readonly TcpServer TcpServer;
        public readonly TcpClient TcpClient;

        public ServerConnection(TcpServer tcpServer, TcpClient tcpClient)
        {
            if (tcpServer == null)
                throw new ArgumentNullException("tcpServer == null");
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient == null");

            TcpServer = tcpServer;
            TcpClient = tcpClient;
        }

        public void Connect()
        {
            Disconnect();

            TcpServer.ServerConnections.TryAdd(this, this);
            PendingPackets = new BlockingCollection<Packet>();

            Shutdown = false;

            Receiver = new Thread(DoReceive);
            Receiver.Start();

            Sender = new Thread(DoSend);
            Sender.Start();
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            Shutdown = true;

            if (TcpClient != null)
                TcpClient.Close();

            Thread thread = Sender;
            if (thread != null && thread.ThreadState == ThreadState.Running)
            {
                if (!thread.Join(5000))
                    thread.Abort();
            }

            Sender = null;

            thread = Receiver;
            if (thread != null && thread.ThreadState == ThreadState.Running)
            {
                if (!thread.Join(5000))
                    thread.Abort();
            }

            Receiver = null;

            PendingPackets.Dispose();

            ServerConnection reference;
            TcpServer.ServerConnections.TryRemove(this, out reference);
        }

        public bool IsConnected
        {
            get { return Receiver != null || Sender != null; }
        }

        private void DoReceive()
        {
            try
            {
                while (!TcpServer.ShutdownTokenSource.Token.IsCancellationRequested && !Shutdown && TcpClient.Connected)
                    ReceivePacket();
            }
            catch (Exception exc)
            {
                TcpServer.LogError(exc);
            }
            finally
            {
                Disconnect();
            }
        }

        private void ReceivePacket()
        {
            BinaryReader reader = new BinaryReader(TcpClient.GetStream());

            long id = reader.ReadInt64();
            int size = reader.ReadInt32();
            TcpServer.BytesReceive += size;

            Packet packet = new Packet(new MemoryStream(reader.ReadBytes(size)));
            packet.ID = id;

            TcpServer.RecievedPackets.Add(new KeyValuePair<ServerConnection, Packet>(this, packet));
        }

        private void DoSend()
        {
            try
            {
                while (!TcpServer.ShutdownTokenSource.Token.IsCancellationRequested && !Shutdown && TcpClient.Connected)
                    SendPacket();
            }
            catch (OperationCanceledException exc)
            {
            }
            catch (Exception exc)
            {
                TcpServer.LogError(exc);
            }
            finally
            {
            }
        }

        private void SendPacket()
        {
            CancellationToken token = TcpServer.ShutdownTokenSource.Token;
            Packet packet = PendingPackets.Take(token);
            TcpServer.BytesSent += packet.Response.Length;

            BinaryWriter writer = new BinaryWriter(TcpClient.GetStream());
            packet.Write(writer, packet.Response);
        }
    }
}