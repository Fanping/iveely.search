using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Iveely.Framework.Log;

namespace Iveely.SearchEngine
{
    public enum ServerStatusLevel { Off, WaitingConnection, ConnectionEstablished };

    public delegate void DataReceivedEventHandler(Object sender, string message, EventArgs e);

    public class DataFrame
    {
        readonly DataFrameHeader _header;
        private readonly byte[] _extend = new byte[0];
        private readonly byte[] _mask = new byte[0];
        private readonly byte[] _content = new byte[0];

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
                int contentLength = _extend[0] * 256 + _extend[1];
                _content = new byte[contentLength];
                Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, contentLength > 1024 * 100 ? 1024 * 100 : contentLength);
            }
            else
            {
                long len = 0;
                int n = 1;
                for (int i = 7; i >= 0; i--)
                {
                    len += _extend[i] * n;
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
        private readonly bool _fin;
        private readonly bool _rsv1;
        private readonly bool _rsv2;
        private readonly bool _rsv3;
        private readonly sbyte _opcode;
        private readonly bool _maskcode;
        private readonly sbyte _payloadlength;

        public bool Fin { get { return _fin; } }

        public bool Rsv1 { get { return _rsv1; } }

        public bool Rsv2 { get { return _rsv2; } }

        public bool Rsv3 { get { return _rsv3; } }

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
            byte[] buffer = { 0, 0 };
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

        private Boolean _isDataMasked;
        public Boolean IsDataMasked
        {
            get { return _isDataMasked; }
            set { _isDataMasked = value; }
        }

        public Socket ConnectionSocket;

        private readonly int _maxBufferSize;
        private string _handshake;
        private string _newHandshake;

        public byte[] ReceivedDataBuffer;
        private readonly byte[] _firstByte;
        private readonly byte[] _lastByte;
        private byte[] _serverKey1;
        private byte[] _serverKey2;

        public event DataReceivedEventHandler DataReceived;

        public SocketConnection()
        {
            _maxBufferSize = 1024 * 100;
            ReceivedDataBuffer = new byte[_maxBufferSize];
            _firstByte = new byte[_maxBufferSize];
            _lastByte = new byte[_maxBufferSize];
            _firstByte[0] = 0x00;
            _lastByte[0] = 0xFF;

            _handshake = "HTTP/1.1 101 Web Socket Protocol Handshake" + Environment.NewLine;
            _handshake += "Upgrade: WebSocket" + Environment.NewLine;
            _handshake += "Connection: Upgrade" + Environment.NewLine;
            _handshake += "Sec-WebSocket-Origin: " + "{0}" + Environment.NewLine;
            _handshake += string.Format("Sec-WebSocket-Location: " + "ws://{0}:4141/chat" + Environment.NewLine, WebSocketServer.GetLocalmachineIPAddress());
            _handshake += Environment.NewLine;

            _newHandshake = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine;
            _newHandshake += "Upgrade: WebSocket" + Environment.NewLine;
            _newHandshake += "Connection: Upgrade" + Environment.NewLine;
            _newHandshake += "Sec-WebSocket-Accept: {0}" + Environment.NewLine;
            _newHandshake += Environment.NewLine;
        }

        private void Read(IAsyncResult status)
        {
            if (!ConnectionSocket.Connected) return;
            DataFrame dr = new DataFrame(ReceivedDataBuffer);
            try
            {
                string messageReceived;
                if (!_isDataMasked)
                {
                    UTF8Encoding decoder = new UTF8Encoding();
                    int startIndex = 0;
                    int endIndex = 0;
                    while (ReceivedDataBuffer[startIndex] == _firstByte[0]) startIndex++;
                    endIndex = startIndex + 1;
                    while (ReceivedDataBuffer[endIndex] != _lastByte[0] && endIndex != _maxBufferSize - 1) endIndex++;
                    if (endIndex == _maxBufferSize - 1) endIndex = _maxBufferSize;
                    messageReceived = decoder.GetString(ReceivedDataBuffer, startIndex, endIndex - startIndex);
                }
                else
                {
                    messageReceived = dr.Text;
                }

                if ((messageReceived.Length == _maxBufferSize && messageReceived[0] == Convert.ToChar(65533)) ||
                    messageReceived.Length == 0)
                {

                }
                else
                {
                    if (DataReceived != null)
                    {
                        DataReceived(this, messageReceived, EventArgs.Empty);
                    }
                    Array.Clear(ReceivedDataBuffer, 0, ReceivedDataBuffer.Length);
                    ConnectionSocket.BeginReceive(ReceivedDataBuffer, 0, ReceivedDataBuffer.Length, 0, new AsyncCallback(Read), null);
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

                if (keyNum == 1) _serverKey1 = currentKey;
                else _serverKey2 = currentKey;
            }
            catch
            {
                if (_serverKey1 != null) Array.Clear(_serverKey1, 0, _serverKey1.Length);
                if (_serverKey2 != null) Array.Clear(_serverKey2, 0, _serverKey2.Length);
            }
        }

        private byte[] BuildServerFullKey(byte[] last8Bytes)
        {
            byte[] concatenatedKeys = new byte[16];
            Array.Copy(_serverKey1, 0, concatenatedKeys, 0, 4);
            Array.Copy(_serverKey2, 0, concatenatedKeys, 4, 4);
            Array.Copy(last8Bytes, 0, concatenatedKeys, 8, 8);

            // MD5 Hash
            MD5 MD5Service = MD5.Create();
            return MD5Service.ComputeHash(concatenatedKeys);
        }

        public void ManageHandshake(IAsyncResult status)
        {
            try
            {
                string header = "Sec-WebSocket-Version:";
                int handshakeLength = (int)status.AsyncState;
                byte[] last8Bytes = new byte[8];

                UTF8Encoding decoder = new UTF8Encoding();
                String rawClientHandshake = decoder.GetString(ReceivedDataBuffer, 0, handshakeLength);

                Array.Copy(ReceivedDataBuffer, handshakeLength - 8, last8Bytes, 0, 8);

                //现在使用的是比较新的Websocket协议
                if (rawClientHandshake.IndexOf(header, StringComparison.Ordinal) != -1)
                {
                    _isDataMasked = true;
                    string[] rawClientHandshakeLines = rawClientHandshake.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    string acceptKey = "";
                    foreach (string line in rawClientHandshakeLines)
                    {
                        if (line.Contains("Sec-WebSocket-Key:"))
                        {
                            acceptKey = ComputeWebSocketHandshakeSecurityHash09(line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 2));
                        }
                    }

                    _newHandshake = string.Format(_newHandshake, acceptKey);
                    byte[] newHandshakeText = Encoding.UTF8.GetBytes(_newHandshake);
                    ConnectionSocket.BeginSend(newHandshakeText, 0, newHandshakeText.Length, 0, HandshakeFinished, null);
                    return;
                }
                string clientHandshake = decoder.GetString(ReceivedDataBuffer, 0, handshakeLength - 8);
                string[] clientHandshakeLines = clientHandshake.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in clientHandshakeLines)
                {
                    if (line.Contains("Sec-WebSocket-Key1:"))
                        BuildServerPartialKey(1, line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 2));
                    if (line.Contains("Sec-WebSocket-Key2:"))
                        BuildServerPartialKey(2, line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 2));
                    if (line.Contains("Origin:"))
                        try
                        {
                            _handshake = string.Format(_handshake, line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 2));
                        }
                        catch
                        {
                            _handshake = string.Format(_handshake, "null");
                        }
                }
                byte[] handshakeText = Encoding.UTF8.GetBytes(_handshake);
                byte[] serverHandshakeResponse = new byte[handshakeText.Length + 16];
                byte[] serverKey = BuildServerFullKey(last8Bytes);
                Array.Copy(handshakeText, serverHandshakeResponse, handshakeText.Length);
                Array.Copy(serverKey, 0, serverHandshakeResponse, handshakeText.Length, 16);
                ConnectionSocket.BeginSend(serverHandshakeResponse, 0, handshakeText.Length + 16, 0, HandshakeFinished, null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
        {
            const String magicKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            String ret = secWebSocketKey + magicKey;
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
            string secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }

        private void HandshakeFinished(IAsyncResult status)
        {
            ConnectionSocket.EndSend(status);
            ConnectionSocket.BeginReceive(ReceivedDataBuffer, 0, ReceivedDataBuffer.Length, 0, new AsyncCallback(Read), null);
        }
    }

    public class WebSocketServer : IDisposable
    {
        private bool _alreadyDisposed;
        private Socket _listener;
        private int _connectionsQueueLength;
        private int _maxBufferSize;
        private byte[] _firstByte;
        private byte[] _lastByte;

        private readonly List<SocketConnection> _connectionSocketList = new List<SocketConnection>();

        public ServerStatusLevel Status { get; private set; }
        public int ServerPort { get; set; }
        public string ServerLocation { get; set; }
        public string ConnectionOrigin { get; set; }
        public event DataReceivedEventHandler DataReceived;

        protected virtual void OnDataReceived(string message)
        {
            DataReceivedEventHandler handler = DataReceived;
            if (handler != null) handler(this, message, EventArgs.Empty);
        }

        public delegate string ProcessMessage(string msg);

        private readonly ProcessMessage _processMessage;
        private void Initialize()
        {
            _alreadyDisposed = false;

            Status = ServerStatusLevel.Off;
            _connectionsQueueLength = 500;
            _maxBufferSize = 1024 * 100;
            _firstByte = new byte[_maxBufferSize];
            _lastByte = new byte[_maxBufferSize];
            _firstByte[0] = 0x00;
            _lastByte[0] = 0xFF;
        }

        public WebSocketServer(ProcessMessage pm)
        {
            _processMessage = pm;
            ServerPort = 4141;
            ServerLocation = string.Format("ws://{0}:4141/chat", GetLocalmachineIPAddress());
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
            if (!_alreadyDisposed)
            {
                _alreadyDisposed = true;
                if (_listener != null) _listener.Close();
                foreach (SocketConnection item in _connectionSocketList)
                {
                    item.ConnectionSocket.Close();
                }
                _connectionSocketList.Clear();
                GC.SuppressFinalize(this);
            }
        }

        public static IPAddress GetLocalmachineIPAddress()
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

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _listener.Bind(new IPEndPoint(GetLocalmachineIPAddress(), ServerPort));
            _listener.Listen(_connectionsQueueLength);
            Console.WriteLine(string.Format("[6/6]启动完毕，WebSocket: ws://{0}:{1}/chat", GetLocalmachineIPAddress(), ServerPort));

            while (true)
            {
                Socket sc = _listener.Accept();

                if (sc != null)
                {
                    Thread.Sleep(100);
                    SocketConnection socketConn = new SocketConnection();
                    socketConn.ConnectionSocket = sc;
                    socketConn.DataReceived += DataRecivedCallBack;
                    socketConn.ConnectionSocket.BeginReceive(socketConn.ReceivedDataBuffer,
                                                             0, socketConn.ReceivedDataBuffer.Length,
                                                             0, socketConn.ManageHandshake,
                                                             socketConn.ConnectionSocket.Available);
                    _connectionSocketList.Add(socketConn);
                }
            }
        }

        /// <summary>
        /// 单点发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public void DataRecivedCallBack(Object sender, string message, EventArgs e)
        {
            Logger.Info("Get Query:" + message);
            SocketConnection item = sender as SocketConnection;
            if (!item.ConnectionSocket.Connected) return;
            string msg = _processMessage(message);
            try
            {
                if (item.IsDataMasked)
                {
                    DataFrame dr = new DataFrame(msg);
                    item.ConnectionSocket.Send(dr.GetBytes());
                }
                else
                {
                    item.ConnectionSocket.Send(_firstByte);
                    item.ConnectionSocket.Send(Encoding.UTF8.GetBytes(msg));
                    item.ConnectionSocket.Send(_lastByte);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
