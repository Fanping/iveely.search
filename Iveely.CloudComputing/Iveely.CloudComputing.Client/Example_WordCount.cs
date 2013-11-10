using System;
using System.Collections;
using System.Text;
using Iveely.CloudComputing.Client;


namespace Iveely.CloudComputing.Example
{
    public sealed class WordCount : Application
    {
        public override void Run(object[] args)
        {
            //0.先初始化
            this.Init(args);

            //1.虚拟数据构建
            string[] contents =
            {
                "This is Iveely Computing",
                "Weclome here",
                "Iveely is I void every thing,except love",
                "Thanks,Iveely Team."
            };
            StringBuilder dataBuilder = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                Random random = new Random(i);
                int index = random.Next(0, 4);
                dataBuilder.AppendLine(contents[index]);
            }

            string[] words = dataBuilder.ToString().Split(new[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            WriteToConsole("local words count:" + words.Length);
            int globalWordCount = Mathematics.Sum<int>(words.Length);
            WriteToConsole("global words count:" + globalWordCount);

            //2.子节点处理数据
            Hashtable table = new Hashtable();
            foreach (string word in words)
            {
                if (table.ContainsKey(word))
                {
                    table[word] = int.Parse(table[word].ToString()) + 1;
                }
                else
                {
                    table.Add(word, 1);
                }
            }

            //3.归并所有结点处理结果
            WriteToConsole("local word frequency count:" + table.Keys.Count);
            table = Mathematics.CombineTable(table, args);
            WriteToConsole("global word frequency count:" + table.Keys.Count);

            //4.写入文件
            StringBuilder builder = new StringBuilder();
            foreach (DictionaryEntry dictionaryEntry in table)
            {
                builder.AppendLine(dictionaryEntry.Key + " " + dictionaryEntry.Value);
            }
            WriteText(builder.ToString(), "WordCount.result", true);
        }
    }
}
