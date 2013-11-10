/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.IO;
using System.Net.Sockets;
using Iveely.Framework.Log;

namespace Iveely.Framework.Network
{
    public class FileTransfer
    {
        public void Send(string filePath, string machine, int port)
        {
            string path = filePath;
            TcpClient client = new TcpClient();
            client.Connect(machine, port);
            if (!File.Exists(path))
            {
                File.WriteAllText("NotFound.txt", "Your File Cannot Found.");
                path = "NotFound.txt";
            }
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
            File.Delete("NotFound.txt");
        }

        public void Receive(int port, string saveFileName)
        {
            string path = saveFileName;
            int maxRetryCount = 5;
            while (maxRetryCount > 0)
            {
                try
                {
                    TcpListener tcpListener = new TcpListener(port);
                    tcpListener.Start();
                    Socket socket = tcpListener.AcceptSocket();
                    FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                    BinaryWriter binarywrite = new BinaryWriter(fileStream);
                    int count;
                    byte[] bytes = new byte[4096];
                    while ((count = socket.Receive(bytes, 4096, SocketFlags.None)) != 0)
                    {
                        binarywrite.Write(bytes, 0, count);
                    }
                    binarywrite.Close();
                    fileStream.Close();
                    socket.Close();
                    tcpListener.Stop();
                    maxRetryCount = -1;
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    maxRetryCount--;
                    if (maxRetryCount > 0)
                        Console.WriteLine("Now retry...");

                }
            }

        }
    }
}
