/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Network
{
    public class FileTransfer
    {
        public void Send(string filePath, string machineIp, int port)
        {
            string path = filePath;
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(machineIp), port);
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryreader = new BinaryReader(file);
            byte[] bytes = new byte[4096];
            int data;
            while ((data = binaryreader.Read(bytes, 0, 4096)) != 0)
            {
                client.Client.Send(bytes, data, SocketFlags.None);
            }
            client.Client.Shutdown(SocketShutdown.Both);
            binaryreader.Close();
            file.Close();
        }

        public void Receive(int port, string saveFileName)
        {
            string path = saveFileName;
            TcpListener tcpListener = new TcpListener(port);
            tcpListener.Start();
            Socket socket = tcpListener.AcceptSocket();
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter binarywrite = new BinaryWriter(fs);
            int count;
            byte[] bytes = new byte[4096];
            while ((count = socket.Receive(bytes, 4096, SocketFlags.None)) != 0)
            {
                binarywrite.Write(bytes, 0, count);
            }
            binarywrite.Close();
            fs.Close();
            socket.Close();
            tcpListener.Stop();
        }
    }
}
