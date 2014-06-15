using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Iveely.Framework.Log;

namespace Iveely.SearchEngine
{
    public enum ServerStatusLevel { Off, WaitingConnection, ConnectionEstablished };

    public delegate void DataReceivedEventHandler(Object sender, string message, EventArgs e);

    public class DataFrame
    {
        DataFrameHeader _header;
        private byte[] _extend = new byte[0];
        private byte[] _mask = new byte[0];
        private byte[] _content = new byte[0];

        public DataFrame(byte[] buffer)
        {
            //帧头
            _header = new DataFrameHeader(buffer);

            //扩展长度
            if (_header.Length == 126)
            {
                _extend = new byte[2];
                Buffer.BlockCopy(buffer, 2, _extend, 0, 2);
            }
            else if (_header.Length == 127)
            {
                _extend = new byte[8];
                Buffer.BlockCopy(buffer, 2, _extend, 0, 8);
            }

            //是否有掩码
            if (_header.HasMask)
            {
                _mask = new byte[4];
                Buffer.BlockCopy(buffer, _extend.Length + 2, _mask, 0, 4);
            }

            //消息体
            if (_extend.Length == 0)
            {
                _content = new byte[_header.Length];
                Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, _content.Length);
            }
            else if (_extend.Length == 2)
            {
                int contentLength = (int)_extend[0] * 256 + (int)_extend[1];
                _content = new byte[contentLength];
                Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, contentLength > 1024 * 100 ? 1024 * 100 : contentLength);
            }
            else
            {
                long len = 0;
                int n = 1;
                for (int i = 7; i >= 0; i--)
                {
                    len += (int)_extend[i] * n;
                    n *= 256;
                }
                _content = new byte[len];
                Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, _content.Length);
            }

            if (_header.HasMask) _content = Mask(_content, _mask);

        }

        public DataFrame(string content)
        {
            _content = Encoding.UTF8.GetBytes(content);
            int length = _content.Length;

            if (length < 126)
            {
                _extend = new byte[0];
                _header = new DataFrameHeader(true, false, false, false, 1, false, length);
            }
            else if (length < 65536)
            {
                _extend = new byte[2];
                _header = new DataFrameHeader(true, false, false, false, 1, false, 126);
                _extend[0] = (byte)(length / 256);
                _extend[1] = (byte)(length % 256);
            }
            else
            {
                _extend = new byte[8];
                _header = new DataFrameHeader(true, false, false, false, 1, false, 127);

                int left = length;
                int unit = 256;

                for (int i = 7; i > 1; i--)
                {
                    _extend[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[2 + _extend.Length + _mask.Length + _content.Length];
            Buffer.BlockCopy(_header.GetBytes(), 0, buffer, 0, 2);
            Buffer.BlockCopy(_extend, 0, buffer, 2, _extend.Length);
            Buffer.BlockCopy(_mask, 0, buffer, 2 + _extend.Length, _mask.Length);
            Buffer.BlockCopy(_content, 0, buffer, 2 + _extend.Length + _mask.Length, _content.Length);
            return buffer;
        }

        public string Text
        {
            get
            {
                if (_header.OpCode != 1)
                    return string.Empty;

                return Encoding.UTF8.GetString(_content);
            }
        }

        private byte[] Mask(byte[] data, byte[] mask)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }

            return data;
        }

    }

    public class DataFrameHeader
    {
        private bool _fin;
        private bool _rsv1;
        private bool _rsv2;
        private bool _rsv3;
        private sbyte _opcode;
        private bool _maskcode;
        private sbyte _payloadlength;

        public bool FIN { get { return _fin; } }

        public bool RSV1 { get { return _rsv1; } }

        public bool RSV2 { get { return _rsv2; } }

        public bool RSV3 { get { return _rsv3; } }

        public sbyte OpCode { get { return _opcode; } }

        public bool HasMask { get { return _maskcode; } }

        public sbyte Length { get { return _payloadlength; } }

        public DataFrameHeader(byte[] buffer)
        {
            if (buffer.Length < 2)
                throw new Exception("无效的数据头.");

            //第一个字节
            _fin = (buffer[0] & 0x80) == 0x80;
            _rsv1 = (buffer[0] & 0x40) == 0x40;
            _rsv2 = (buffer[0] & 0x20) == 0x20;
            _rsv3 = (buffer[0] & 0x10) == 0x10;
            _opcode = (sbyte)(buffer[0] & 0x0f);

            //第二个字节
            _maskcode = (buffer[1] & 0x80) == 0x80;
            _payloadlength = (sbyte)(buffer[1] & 0x7f);

        }

        //发送封装数据
        public DataFrameHeader(bool fin, bool rsv1, bool rsv2, bool rsv3, sbyte opcode, bool hasmask, int length)
        {
            _fin = fin;
            _rsv1 = rsv1;
            _rsv2 = rsv2;
            _rsv3 = rsv3;
            _opcode = opcode;
            //第二个字节
            _maskcode = hasmask;
            _payloadlength = (sbyte)length;
        }

        //返回帧头字节
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[2] { 0, 0 };

            if (_fin) buffer[0] ^= 0x80;
            if (_rsv1) buffer[0] ^= 0x40;
            if (_rsv2) buffer[0] ^= 0x20;
            if (_rsv3) buffer[0] ^= 0x10;

            buffer[0] ^= (byte)_opcode;

            if (_maskcode) buffer[1] ^= 0x80;

            buffer[1] ^= (byte)_payloadlength;

            return buffer;
        }
    }

    public class SocketConnection
    {

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Boolean isDataMasked;
        public Boolean IsDataMasked
        {
            get { return isDataMasked; }
            set { isDataMasked = value; }
        }

        public Socket ConnectionSocket;

        private int MaxBufferSize;
        private string Handshake;
        private string New_Handshake;

        public byte[] receivedDataBuffer;
        private byte[] FirstByte;
        private byte[] LastByte;
        private byte[] ServerKey1;
        private byte[] ServerKey2;

        public event DataReceivedEventHandler DataReceived;

        public SocketConnection()
        {
            MaxBufferSize = 1024 * 100;
            receivedDataBuffer = new byte[MaxBufferSize];
            FirstByte = new byte[MaxBufferSize];
            LastByte = new byte[MaxBufferSize];
            FirstByte[0] = 0x00;
            LastByte[0] = 0xFF;

            Handshake = "HTTP/1.1 101 Web Socket Protocol Handshake" + Environment.NewLine;
            Handshake += "Upgrade: WebSocket" + Environment.NewLine;
            Handshake += "Connection: Upgrade" + Environment.NewLine;
            Handshake += "Sec-WebSocket-Origin: " + "{0}" + Environment.NewLine;
            Handshake += string.Format("Sec-WebSocket-Location: " + "ws://{0}:4141/chat" + Environment.NewLine, WebSocketServer.getLocalmachineIPAddress());
            Handshake += Environment.NewLine;

            New_Handshake = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine;
            New_Handshake += "Upgrade: WebSocket" + Environment.NewLine;
            New_Handshake += "Connection: Upgrade" + Environment.NewLine;
            New_Handshake += "Sec-WebSocket-Accept: {0}" + Environment.NewLine;
            New_Handshake += Environment.NewLine;
        }

        private void Read(IAsyncResult status)
        {
            if (!ConnectionSocket.Connected) return;
            string messageReceived = string.Empty;
            DataFrame dr = new DataFrame(receivedDataBuffer);

            try
            {
                if (!this.isDataMasked)
                {
                    // Web Socket protocol: messages are sent with 0x00 and 0xFF as padding bytes
                    System.Text.UTF8Encoding decoder = new System.Text.UTF8Encoding();
                    int startIndex = 0;
                    int endIndex = 0;

                    // Search for the start byte
                    while (receivedDataBuffer[startIndex] == FirstByte[0]) startIndex++;
                    // Search for the end byte
                    endIndex = startIndex + 1;
                    while (receivedDataBuffer[endIndex] != LastByte[0] && endIndex != MaxBufferSize - 1) endIndex++;
                    if (endIndex == MaxBufferSize - 1) endIndex = MaxBufferSize;

                    // Get the message
                    messageReceived = decoder.GetString(receivedDataBuffer, startIndex, endIndex - startIndex);
                }
                else
                {
                    messageReceived = dr.Text;
                }

                if ((messageReceived.Length == MaxBufferSize && messageReceived[0] == Convert.ToChar(65533)) ||
                    messageReceived.Length == 0)
                {

                }
                else
                {
                    if (DataReceived != null)
                    {
                        DataReceived(this, messageReceived, EventArgs.Empty);
                    }
                    Array.Clear(receivedDataBuffer, 0, receivedDataBuffer.Length);
                    ConnectionSocket.BeginReceive(receivedDataBuffer, 0, receivedDataBuffer.Length, 0, new AsyncCallback(Read), null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void BuildServerPartialKey(int keyNum, string clientKey)
        {
            string partialServerKey = "";
            byte[] currentKey;
            int spacesNum = 0;
            char[] keyChars = clientKey.ToCharArray();
            foreach (char currentChar in keyChars)
            {
                if (char.IsDigit(currentChar)) partialServerKey += currentChar;
                if (char.IsWhiteSpace(currentChar)) spacesNum++;
            }
            try
            {
                currentKey = BitConverter.GetBytes((int)(Int64.Parse(partialServerKey) / spacesNum));
                if (BitConverter.IsLittleEndian) Array.Reverse(currentKey);

                if (keyNum == 1) ServerKey1 = currentKey;
                else ServerKey2 = currentKey;
            }
            catch
            {
                if (ServerKey1 != null) Array.Clear(ServerKey1, 0, ServerKey1.Length);
                if (ServerKey2 != null) Array.Clear(ServerKey2, 0, ServerKey2.Length);
            }
        }

        private byte[] BuildServerFullKey(byte[] last8Bytes)
        {
            byte[] concatenatedKeys = new byte[16];
            Array.Copy(ServerKey1, 0, concatenatedKeys, 0, 4);
            Array.Copy(ServerKey2, 0, concatenatedKeys, 4, 4);
            Array.Copy(last8Bytes, 0, concatenatedKeys, 8, 8);

            // MD5 Hash
            System.Security.Cryptography.MD5 MD5Service = System.Security.Cryptography.MD5.Create();
            return MD5Service.ComputeHash(concatenatedKeys);
        }

        public void ManageHandshake(IAsyncResult status)
        {
            try
            {


                string header = "Sec-WebSocket-Version:";
                int HandshakeLength = (int)status.AsyncState;
                byte[] last8Bytes = new byte[8];

                System.Text.UTF8Encoding decoder = new System.Text.UTF8Encoding();
                String rawClientHandshake = decoder.GetString(receivedDataBuffer, 0, HandshakeLength);

                Array.Copy(receivedDataBuffer, HandshakeLength - 8, last8Bytes, 0, 8);

                //现在使用的是比较新的Websocket协议
                if (rawClientHandshake.IndexOf(header) != -1)
                {
                    this.isDataMasked = true;
                    string[] rawClientHandshakeLines = rawClientHandshake.Split(new string[] { Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
                    string acceptKey = "";
                    foreach (string Line in rawClientHandshakeLines)
                    {
                        //Console.WriteLine(Line);
                        if (Line.Contains("Sec-WebSocket-Key:"))
                        {
                            acceptKey = ComputeWebSocketHandshakeSecurityHash09(Line.Substring(Line.IndexOf(":") + 2));
                        }
                    }

                    New_Handshake = string.Format(New_Handshake, acceptKey);
                    byte[] newHandshakeText = Encoding.UTF8.GetBytes(New_Handshake);
                    ConnectionSocket.BeginSend(newHandshakeText, 0, newHandshakeText.Length, 0, HandshakeFinished, null);
                    return;
                }

                string ClientHandshake = decoder.GetString(receivedDataBuffer, 0, HandshakeLength - 8);

                string[] ClientHandshakeLines = ClientHandshake.Split(new string[] { Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);


                // Welcome the new client
                foreach (string Line in ClientHandshakeLines)
                {
                    if (Line.Contains("Sec-WebSocket-Key1:"))
                        BuildServerPartialKey(1, Line.Substring(Line.IndexOf(":") + 2));
                    if (Line.Contains("Sec-WebSocket-Key2:"))
                        BuildServerPartialKey(2, Line.Substring(Line.IndexOf(":") + 2));
                    if (Line.Contains("Origin:"))
                        try
                        {
                            Handshake = string.Format(Handshake, Line.Substring(Line.IndexOf(":") + 2));
                        }
                        catch
                        {
                            Handshake = string.Format(Handshake, "null");
                        }
                }
                // Build the response for the client
                byte[] HandshakeText = Encoding.UTF8.GetBytes(Handshake);
                byte[] serverHandshakeResponse = new byte[HandshakeText.Length + 16];
                byte[] serverKey = BuildServerFullKey(last8Bytes);
                Array.Copy(HandshakeText, serverHandshakeResponse, HandshakeText.Length);
                Array.Copy(serverKey, 0, serverHandshakeResponse, HandshakeText.Length, 16);
                ConnectionSocket.BeginSend(serverHandshakeResponse, 0, HandshakeText.Length + 16, 0, HandshakeFinished, null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
        {
            const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            String secWebSocketAccept = String.Empty;
            // 1. Combine the request Sec-WebSocket-Key with magic key.
            String ret = secWebSocketKey + MagicKEY;
            // 2. Compute the SHA1 hash
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
            // 3. Base64 encode the hash
            secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }

        private void HandshakeFinished(IAsyncResult status)
        {
            ConnectionSocket.EndSend(status);
            ConnectionSocket.BeginReceive(receivedDataBuffer, 0, receivedDataBuffer.Length, 0, new AsyncCallback(Read), null);
        }
    }

    public class WebSocketServer : IDisposable
    {
        private bool AlreadyDisposed;
        private Socket Listener;
        private int ConnectionsQueueLength;
        private int MaxBufferSize;
        private string Handshake;
        private StreamReader ConnectionReader;
        private StreamWriter ConnectionWriter;
        private byte[] FirstByte;
        private byte[] LastByte;
        private byte[] ServerKey1;
        private byte[] ServerKey2;

        List<SocketConnection> connectionSocketList = new List<SocketConnection>();

        public ServerStatusLevel Status { get; private set; }
        public int ServerPort { get; set; }
        public string ServerLocation { get; set; }
        public string ConnectionOrigin { get; set; }
        public event DataReceivedEventHandler DataReceived;
        public delegate string ProcessMessage(string msg);

        private ProcessMessage processMessage;
        private void Initialize()
        {
            AlreadyDisposed = false;

            Status = ServerStatusLevel.Off;
            ConnectionsQueueLength = 500;
            MaxBufferSize = 1024 * 100;
            FirstByte = new byte[MaxBufferSize];
            LastByte = new byte[MaxBufferSize];
            FirstByte[0] = 0x00;
            LastByte[0] = 0xFF;
        }

        public WebSocketServer(ProcessMessage pm)
        {
            this.processMessage = pm;
            ServerPort = 4141;
            ServerLocation = string.Format("ws://{0}:4141/chat", getLocalmachineIPAddress());
            Initialize();
        }

        ~WebSocketServer()
        {
            Close();
        }


        public void Dispose()
        {
            Close();
        }

        private void Close()
        {
            if (!AlreadyDisposed)
            {
                AlreadyDisposed = true;
                if (Listener != null) Listener.Close();
                foreach (SocketConnection item in connectionSocketList)
                {
                    item.ConnectionSocket.Close();
                }
                connectionSocketList.Clear();
                GC.SuppressFinalize(this);
            }
        }

        public static IPAddress getLocalmachineIPAddress()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);

            foreach (IPAddress ip in ipEntry.AddressList)
            {
                //IPV4
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }

            return ipEntry.AddressList[0];
        }

        public void StartServer()
        {
            Char char1 = Convert.ToChar(65533);

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            Listener.Bind(new IPEndPoint(getLocalmachineIPAddress(), ServerPort));
            Listener.Listen(ConnectionsQueueLength);
            Console.WriteLine(string.Format("[6/6]启动完毕，WebSocket: ws://{0}:{1}/chat", getLocalmachineIPAddress(), ServerPort));

            while (true)
            {
                Socket sc = Listener.Accept();

                if (sc != null)
                {
                    System.Threading.Thread.Sleep(100);
                    SocketConnection socketConn = new SocketConnection();
                    socketConn.ConnectionSocket = sc;
                    socketConn.DataReceived += new DataReceivedEventHandler(DataRecivedCallBack);
                    socketConn.ConnectionSocket.BeginReceive(socketConn.receivedDataBuffer,
                                                             0, socketConn.receivedDataBuffer.Length,
                                                             0, new AsyncCallback(socketConn.ManageHandshake),
                                                             socketConn.ConnectionSocket.Available);
                    connectionSocketList.Add(socketConn);
                }
            }
        }

        /// <summary>
        /// 单点发送
        /// </summary>
        /// <param name="item"></param>
        /// <param name="message"></param>
        public void DataRecivedCallBack(Object sender, string message, EventArgs e)
        {
            Logger.Info("Get Query:" + message);
            SocketConnection item = sender as SocketConnection;
            if (!item.ConnectionSocket.Connected) return;
            string msg = processMessage(message);
            try
            {
                if (item.IsDataMasked)
                {
                    DataFrame dr = new DataFrame(msg);
                    item.ConnectionSocket.Send(dr.GetBytes());
                }
                else
                {
                    item.ConnectionSocket.Send(FirstByte);
                    item.ConnectionSocket.Send(Encoding.UTF8.GetBytes(msg));
                    item.ConnectionSocket.Send(LastByte);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
