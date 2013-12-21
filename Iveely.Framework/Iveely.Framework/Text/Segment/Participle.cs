// ------------------------------------------------------------------------------------------------
//  <copyright file="Participle.cs" company="Iveely">
//    Copyright (c) Iveely Liu.  All rights reserved.
//  </copyright>
//  
//  <Create Time>
//    12/03/2012 20:12 
//  </Create Time>
//  
//  <contact owner>
//    liufanping@iveely.com 
//  </contact owner>
//  -----------------------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

#endregion

namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// 分词操作类
    /// </summary>
    [Serializable]
    public class Participle : HMM
    {
        /// <summary>
        /// 唯一单例
        /// </summary>
        private static Participle participle;

        /// <summary>
        /// 分界符（用于句子中的分割）
        /// </summary>
        private string Delimiter;

        private string[] states = new string[] { "单字成词", "词头", "词中", "词尾" };

        /// <summary>
        /// 分词结果
        /// </summary>
        private string Result;

        private Participle()
        {
            this.SetState(states);
            Delimiter = ".*?([，。,；！？…：●—－\r\n]+) ><=!@#%^&*";
            this.Train(File.ReadAllText("msr_train.txt", Encoding.UTF8));
            Result = "";
        }

        private void Train(string corpus)
        {
            string[] sentences = corpus.Split(this.Delimiter.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            string lastwordType = states[0];
            string currentWordType = string.Empty;
            foreach (var sentence in sentences)
            {
                char[] list = sentence.ToArray();
                for (int i = 0; i < list.Length; i = i + 3)
                {
                    string word = list[i].ToString(CultureInfo.InvariantCulture);
                    string wordType = list[i + 2].ToString(CultureInfo.InvariantCulture).ToLower();
                    //添加观察状态元素
                    this.AddObserver(word);
                    if (wordType.Equals("s"))
                    {
                        currentWordType = states[0];
                    }
                    else if (wordType.Equals("e"))
                    {
                        currentWordType = states[3];
                    }
                    else if (wordType.Equals("m"))
                    {
                        currentWordType = states[2];
                    }
                    else if (wordType.Equals("b"))
                    {
                        currentWordType = states[1];
                    }
                    //初始状态转移矩阵
                    AddInitialStateProbability(currentWordType, 1);
                    //观察状态转移矩阵
                    AddComplexProbability(currentWordType, word);
                    //隐含状态转移矩阵
                    AddTransferProbability(lastwordType, currentWordType, 1);
                    lastwordType = currentWordType;
                }
            }
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns> </returns>
        public static Participle GetInstance(string dir = "")
        {
            string serializedFile = dir + "participle.Ser.global";
            if (participle == null && File.Exists(serializedFile))
            {
                participle = Serializer.DeserializeFromFile<Participle>(serializedFile);
            }
            else if (participle == null)
            {
                participle = new Participle();
                Serializer.SerializeToFile(participle, serializedFile);
            }
            return participle;
        }

        /// <summary>
        /// 执行分词操作
        /// </summary>
        /// <returns> 分词结果 </returns>
        public string Split(string sentences)
        {
            this.Result = string.Empty;
            foreach (string sentence in sentences.Split(this.Delimiter.ToArray()))
            {

                string[] words = sentence.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray();
                if (words.Length != 0)
                {
                    double pro;
                    int x;
                    int[] path = this.Decode(words, out pro, 1);

                    for (int i = 0; i < words.Length; i++)
                    {
                        Result += words[i];
                        //这个是为了解决数字分词问题，不建议这样
                        if (path[i] == 0 && i + 1 < words.Length && int.TryParse(words[i], out x))
                        {
                            continue;
                        }
                        if (path[i] == 0 || path[i] == 3)
                        {
                            Result += "/";
                        }
                    }
                }

            }
            return Result;
        }

        public void TextAccuracy()
        {
            string[] lines = File.ReadAllLines("splitTest.txt");
            int totalCount = 0;
            int right = 0;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string line in lines)
            {
                string textCorpus = line.Replace(" ", "");
                string splitResult = this.Split(textCorpus);
                string[] results = splitResult.Split('/');
                List<string> words = new List<string>(line.Split(' '));
                totalCount += words.Count;
                foreach (string result in results)
                {
                    if (words.Contains(result))
                    {
                        right++;
                    }
                }
                stringBuilder.AppendLine(line);
                stringBuilder.AppendLine(splitResult);
                stringBuilder.AppendLine("");
            }
            File.WriteAllText("accrucyresult.txt", stringBuilder.ToString());
            Console.WriteLine(right * 1.0 / totalCount);
        }


        #region Test

        public static void Test(string content = "")
        {
            Participle instance = GetInstance();
            if (content.Length < 1)
            {
                Console.WriteLine(instance.Split("你好世界！"));
                Console.WriteLine(instance.Split("杭州都下雪啦！"));
                Console.WriteLine(instance.Split("我的心痛竟是你的快乐！"));
                Console.WriteLine(instance.Split("可是我不想对你念念不舍！"));
                Console.WriteLine(instance.Split("但什么让我辗转反侧！"));
                /// /Console.WriteLine(
                //    participle.Split(
                //        "【武汉地铁站冠名全被取消】煮熟的鸭子飞走了！即将开通的武汉地铁2号线，包括周黑鸭在内的9个车站冠名，将全部取消。已经挂出的标识，正式开通前都会更改过来。花2700万竞得冠名权的7家企业，部分已接到通知，周黑鸭很淡定。网友建议，冠名应该听听市民的意见。武汉晚报"));
                // Console.WriteLine(participle.Split("Hello World!"));
            }
            else
            {
                Console.WriteLine(instance.Split(content));
            }
        }

        #endregion
    }
}