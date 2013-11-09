/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;

namespace Iveely.CloudComputting.Client
{
    public class Program
    {
        /// <summary>
        /// 供提交客户端应用程序
        /// 格式：submit filepath namespace.classname appname
        ///       split filepath remotepath(ex. split test.txt system/test.txt)
        ///       download remotepath filepath
        ///       delete remotepath
        ///       rename filepath newfileName
        ///       list /folder
        ///       exit
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "Iveely Cloud Computting Platform";
            while (true)
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
            else
            {
                RemoteCommand.UnknowCommand();
            }
            return false;
        }
    }
}
