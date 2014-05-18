using Iveely.General.Communication;
using Iveely.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iveely.Remote.Heap
{
    public class HeapServer
    {
        private CancellationTokenSource ShutdownTokenSource;
        private Thread Worker;

        public readonly IHeap Heap;
        public readonly TcpServer TcpServer;

        public HeapServer(IHeap heap, TcpServer tcpServer)
        {
            if (heap == null)
                throw new ArgumentNullException("heap");
            if (tcpServer == null)
                throw new ArgumentNullException("tcpServer");

            Heap = heap;
            TcpServer = tcpServer;
        }

        public HeapServer(IHeap heap, int port = 7183)
            : this(heap, new TcpServer(port))
        {
        }

        public void Start()
        {
            Stop();

            ShutdownTokenSource = new CancellationTokenSource();

            Worker = new Thread(DoWork);
            Worker.Start();
        }

        public void Stop()
        {
            if (!IsWorking)
                return;

            ShutdownTokenSource.Cancel(false);

            Thread thread = Worker;
            if (thread != null)
            {
                if (!thread.Join(5000))
                    thread.Abort();
            }
            Heap.Close();
        }

        private void DoWork()
        {
            try
            {
                TcpServer.Start();

                while (!ShutdownTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var order = TcpServer.RecievedPackets.Take(ShutdownTokenSource.Token);

                        BinaryReader reader = new BinaryReader(order.Value.Request);
                        MemoryStream ms = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(ms);

                        var code = (RemoteHeapCommandCodes)reader.ReadByte();

                        switch (code)
                        {
                            case RemoteHeapCommandCodes.ObtainHandle:
                                ObtainHandleCommand.WriteResponse(writer, Heap.ObtainNewHandle());
                                break;

                            case RemoteHeapCommandCodes.ReleaseHandle:
                                {
                                    var handle = ReleaseHandleCommand.ReadRequest(reader).Handle;
                                    Heap.Release(handle);
                                    break;
                                }

                            case RemoteHeapCommandCodes.HandleExist:
                                {
                                    long handle = HandleExistCommand.ReadRequest(reader).Handle;
                                    HandleExistCommand.WriteResponse(writer, Heap.Exists(handle));
                                    break;
                                }

                            case RemoteHeapCommandCodes.WriteCommand:
                                var cmd = WriteCommand.ReadRequest(reader);
                                Heap.Write(cmd.Handle, cmd.Buffer, cmd.Index, cmd.Count);
                                break;

                            case RemoteHeapCommandCodes.ReadCommand:
                                {
                                    var handle = ReadCommand.ReadRequest(reader).Handle;
                                    ReadCommand.WriteResponse(writer, Heap.Read(handle));
                                    break;
                                }

                            case RemoteHeapCommandCodes.CommitCommand:
                                Heap.Commit();
                                break;

                            case RemoteHeapCommandCodes.CloseCommand:
                                Heap.Close();
                                break;

                            case RemoteHeapCommandCodes.SetTag:
                                Heap.Tag = SetTagCommand.ReadRequest(reader).Tag;
                                break;

                            case RemoteHeapCommandCodes.GetTag:
                                GetTagCommand.WriteResponse(writer, Heap.Tag);
                                break;

                            case RemoteHeapCommandCodes.Size:
                                SizeCommand.WriteResponse(writer, Heap.Size);
                                break;

                            case RemoteHeapCommandCodes.DataBaseSize:
                                DataBaseSizeCommand.WriteResponse(writer, Heap.DataSize);
                                break;

                            default:
                                break;
                        }

                        ms.Position = 0;
                        order.Value.Response = ms;
                        order.Key.PendingPackets.Add(order.Value);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception exc)
                    {
                        TcpServer.LogError(exc);
                    }
                }
            }
            catch (Exception exc)
            {
                TcpServer.LogError(exc);
            }
            finally
            {
                TcpServer.Stop();
                Worker = null;
            }
        }

        public bool IsWorking
        {
            get { return Worker != null; }
        }

        public int ClientsCount
        {
            get { return TcpServer.ServerConnections.Count; }
        }
    }
}