/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Text
{

    /// <summary>
    /// 文件块操作
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class FileBlock
    {
        /// <summary>
        /// 切分文件
        /// </summary>
        /// <param name="filePath">原文件路径</param>
        /// <param name="blockCount">切分的块个数</param>
        public static void Split(string filePath, int blockCount)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader fileReader = new BinaryReader(fileStream);
            CheckFolder(filePath);
            int childFileSize = (int)fileStream.Length / blockCount;
            for (int i = 1; i <= blockCount; i++)
            {
                string sTempFileName = filePath + ".part//" + i.ToString(CultureInfo.InvariantCulture); //小文件名
                FileStream tempStream = new FileStream(sTempFileName, FileMode.OpenOrCreate);
                BinaryWriter tempWriter = new BinaryWriter(tempStream);
                byte[] tempBytes = fileReader.ReadBytes(childFileSize);
                tempWriter.Write(tempBytes);
                tempWriter.Close();
                tempStream.Close();
            }
        }

        /// <summary>
        /// 切分文件
        /// </summary>
        /// <param name="filePath">原文件路径</param>
        /// <param name="blockCount">切分的块个数</param>
        /// <param name="splitString">行里面列切分符</param>
        /// <param name="keys">切分按照keys进行切分</param>
        public static void Split(string filePath, int blockCount, string splitString, params int[] keys)
        {
            if (keys == null)
            {
                Split(filePath, blockCount);
            }
            else
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader fileReader = new StreamReader(fileStream);
                CheckFolder(filePath);
                long totalLineCount = 0;
                int maxErrorLine = 100;
                string line = fileReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    bool isErrorLine = false;
                    if (maxErrorLine < 1)
                    {
                        break;
                    }
                    string[] cloumns = line.Split(new[] { splitString }, StringSplitOptions.RemoveEmptyEntries);

                    string flagValue = string.Empty;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (keys[i] < cloumns.Length)
                        {
                            flagValue += cloumns[keys[i]];
                        }
                        else
                        {
                            Logger.Error("The split key can not found. index=" + keys[i] + " out of range.");
                            maxErrorLine--;
                            isErrorLine = true;
                        }
                    }
                    if (!isErrorLine)
                    {
                        totalLineCount++;
                        int partFlag = flagValue.GetHashCode() % blockCount;
                        string sTempFileName = filePath + ".part//" + partFlag.ToString(CultureInfo.InvariantCulture);
                        //小文件名
                        File.AppendAllText(sTempFileName, line + "\r\n", Encoding.UTF8);
                    }
                    line = fileReader.ReadLine();
                }
                fileReader.Close();
                fileStream.Close();
                Logger.Info("Total line " + totalLineCount + ",file name:" + filePath);
            }
        }

        /// <summary>
        /// 合并数据
        /// </summary>
        /// <param name="folder">数据存放的目录</param>
        /// <param name="fileName">合并后的文件名</param>
        public static void Merge(string folder, string fileName)
        {
            string[] fileArray = Directory.GetFiles(folder);
            int totalFileCount = fileArray.Length;
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            for (int i = 0; i < totalFileCount; i++)
            {
                FileStream tempStream = new FileStream(fileArray[i], FileMode.Open);
                BinaryReader tempReader = new BinaryReader(tempStream);
                binaryWriter.Write(tempReader.ReadBytes((int)tempStream.Length));
                tempReader.Close();
                tempStream.Close();
            }
            binaryWriter.Close();
            fileStream.Close();
        }

        private static void CheckFolder(string filePath)
        {
            string saveFolder = filePath + ".part";
            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }
            Directory.CreateDirectory(saveFolder);
        }

#if DEBUG
        [TestMethod]
        public void TestFileBlock_Split_Merge()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                builder.AppendLine(i.ToString());
            }
            File.WriteAllText("temp.txt", builder.ToString());

            Split("temp.txt", 2);
            Assert.IsTrue(Directory.GetFiles("temp.txt.part").Length == 2);

            Merge("temp.txt.part", "mytemp.txt");
            Assert.IsTrue(File.Exists("mytemp.txt"));

            File.Delete("temp.txt");
            File.Delete("mytemp.txt");
            Directory.Delete("temp.txt.part", true);
        }
#endif
    }
}
