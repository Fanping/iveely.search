/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using log4net;

namespace Iveely.CloudComputing.Client
{
    public class Program
    {
        /// <summary>
        /// 供提交客户端应用程序
        /// 格式：submit filepath namespace.classname appname
        ///    split filepath remotepath(ex. split test.txt system/test.txt)
        ///    split filepath remotepath splitstring key1 key2 key3...
        ///    download remotepath filepath
        ///    delete remotepath
        ///    rename filepath newfileName
        ///    list /folder
        ///    exit
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "Iveely Cloud Computting Platform";
            while (true)
            {
                try
                {
                    //0. 检查传递参数
                    if (args == null || args.Length == 0)
                    {
                        ConsoleColor color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("Cmd Input:");
                        Console.ForegroundColor = color;
                        string readLine = Console.ReadLine();
                        if (!string.IsNullOrEmpty(readLine))
                            args = readLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        else
                            continue;
                    }
                    if (ProcessCommand(args))
                    {
                        break;
                    }
                    args = null;
                }
                catch (Exception exception)
                {
                    LogHelper.Error(exception);
                    return;
                }
            }
            Console.WriteLine("Command line has been finished,press anykey to exit.");
            Console.ReadLine();
        }

        private static bool ProcessCommand(string[] args)
        {
            string cmd = args[0].ToLower();
            RemoteCommand command;
            //1. 如果是提交程序
            #region submit
            if (cmd == "submit")
            {
                command = new SubmitCmd();
                command.ProcessCmd(args);
            }
            #endregion

            //2. 如果是切分文件
            #region split
            else if (cmd == "split")
            {
                command = new SplitCommond();
                command.ProcessCmd(args);
            }
            #endregion

            //3. 如果是下载
            #region download

            else if (cmd == "download")
            {
                command = new DownloadCmd();
                command.ProcessCmd(args);
            }

            #endregion

            //4. 如果是删除
            else if (cmd == "delete")
            {
                command = new DeleteCmd();
                command.ProcessCmd(args);
            }

            //5. 如果是显示文件
            else if (cmd == "list")
            {
                command = new ListCmd();
                command.ProcessCmd(args);
            }

            //6. 如果是重命名
            else if (cmd == "rename")
            {
                command = new RenameCmd();
                command.ProcessCmd(args);
            }

            //7. 如果是退出命令
            else if (cmd == "exit")
            {
                return true;
            }

            //8. 如果是停止任务命令
            else if(cmd=="kill")
            {
                command =new KillCmd();
                command.ProcessCmd(args);
            }

            //9. 如果是显示所有任务命令
            else if (cmd == "task")
            {
                command=new TaskCmd();
                command.ProcessCmd(args);
            }

            //10. 如果是显示可用磁盘空间
            else if (cmd == "disk")
            {
                command = new DiskCmd();
                command.ProcessCmd(args);
            }

            //11. 如果是显示可用内存空间
            else if (cmd == "memory")
            {
                command = new MemCmd();
                command.ProcessCmd(args);
            }

            else
            {
                //Example_Data_Sort dataSort = new Example_Data_Sort();
                ////                Args[0] 数据回写IP
                ////Args[1] 数据回写端口
                ////Args[2] 当前进程远程执行worker的IP
                ////Args[3] 当进程远程执行worker的端口号
                ////Args[4] 应用程序运行提交的时间戳
                ////Args[5 ] 应用程序的名称 
                //object[] array = new object[6];
                //array[0] = "127.0.0.1";
                //array[1] = "8800";
                //array[2] = "127.0.0.1";
                //array[3] = "8001";
                //array[4] = "111111";
                //array[5] = "example";
                //dataSort.Run(array);
                RemoteCommand.UnknowCommand();
            }
            return false;
        }
    }
}
