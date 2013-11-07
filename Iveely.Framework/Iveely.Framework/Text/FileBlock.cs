/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Text
{
#if DEBUG
    [TestClass]
#endif
    public class FileBlock
    {
        public static void Split(string filePath, int blockCount)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader fileReader = new BinaryReader(fileStream);
            string saveFolder = filePath + ".part";
            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }
            Directory.CreateDirectory(saveFolder);
            int childFileSize = (int)fileStream.Length / blockCount;
            for (int i = 1; i <= blockCount; i++)
            {
                string sTempFileName = saveFolder + "//" + i.ToString(CultureInfo.InvariantCulture); //小文件名
                FileStream tempStream = new FileStream(sTempFileName, FileMode.OpenOrCreate);
                BinaryWriter tempWriter = new BinaryWriter(tempStream);
                byte[] tempBytes = fileReader.ReadBytes(childFileSize);
                tempWriter.Write(tempBytes);
                tempWriter.Close();
                tempStream.Close();
            }
        }

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
