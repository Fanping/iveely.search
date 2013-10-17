using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Iveely.CloudComputting.Client
{
    public sealed class Example : Application
    {
        public override void Run(object[] args)
        {
            //1.读取文件数据
            string content = ReadText("news", false, args);
            string[] words = content.Split(new[] { ' ', '"', '.', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            DiagnosticsWrite("local words count:" + words.Length, args);
            int globalWordCount = Mathematics.Sum<int>(words.Length, args);
            DiagnosticsWrite("global words count:" + globalWordCount, args);

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
            DiagnosticsWrite("local word frequency count:" + table.Keys.Count, args);
            table = Mathematics.CombineTable(table, args);
            DiagnosticsWrite("global word frequency count:" + table.Keys.Count, args);

            //4.写入文件
            StringBuilder builder = new StringBuilder();
            foreach (DictionaryEntry dictionaryEntry in table)
            {
                builder.AppendLine(dictionaryEntry.Key + " " + dictionaryEntry.Value);
            }
            WriteText(builder.ToString(), "WordFrequency", true, args);
        }
    }
}
