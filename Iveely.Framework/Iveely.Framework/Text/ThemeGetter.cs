using System;
using System.Collections.Generic;

namespace Iveely.Framework.Text
{
    /// <summary>
    /// 主题提取
    /// </summary>
    public class ThemeGetter
    {
        private int[,] L;

        private string Common;

        public string Str1 { get; set; }

        public string Str2 { get; set; }

        private string Content;

        private List<string> all = new List<string>();

        public string GetTheme(string text)
        {
            all.Clear();
            Content = text;
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines.Length; j++)
                {
                    if (i != j)
                    {
                        GetSentenceTheme(lines[i], lines[j]);
                    }
                }
            }

            string result = GetCommonString();
            return result;
        }

        public string GetTheme(string text, string[] splitChar)
        {
            all.Clear();
            Content = text;
            string[] lines = text.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines.Length; j++)
                {
                    if (i != j)
                    {
                        GetSentenceTheme(lines[i], lines[j]);
                    }
                }
            }

            string result = GetCommonString();
            return result;
        }

        public string GetTheme(string text, int count)
        {
            all.Clear();
            Content = text;
            if (count < 0)
            {
                throw new Exception();
            }

            int len = text.Length;
            List<string> list = new List<string>();
            string temp = string.Empty;
            for (int i = 0; i < len; i++)
            {
                if ((i + 1) % (len / count) != 0)
                {
                    temp += text[i];
                }
                else
                {
                    list.Add(temp);
                    temp = string.Empty;
                }
            }
            if (temp.Length > 0)
            {
                list.Add(temp);
            }

            string[] lines = list.ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines.Length; j++)
                {
                    if (i != j)
                    {
                        GetSentenceTheme(lines[i], lines[j]);
                    }
                }
            }

            string result = GetCommonString();
            return result;
        }

        public string GetTheme(string text, string compareText)
        {
            all.Clear();
            Content = text;
            GetSentenceTheme(text, compareText);
            return GetCommonString();
        }


        private void GetSentenceTheme(string s1, string s2)
        {
            Str1 = s1;
            Str2 = s2;

            int R = s1.Length, C = s2.Length;
            int lasti = -1;
            int lastj = -1;
            L = new int[R + 1, C + 1];
            Common = "";
            for (int i = 0; i <= R; i++)
                L[i, 0] = 0;//初始化第0列
            for (int j = 0; j <= C; j++)
                L[0, j] = 0;//初始化第0行
            for (int i = 1; i <= R; i++)
                for (int j = 1; j <= C; j++)
                {
                    if (Str1[i - 1] == Str2[j - 1])
                    {
                        L[i, j] = L[i - 1, j - 1] + 1;
                        if (i != lasti)
                        {
                            if ((lasti != i - 1 || lastj != j - 1) && lasti != -1)
                            {
                                if (Common.Trim().Length > 1)
                                {
                                    all.Add(Common);
                                }
                                Common = string.Empty;
                            }
                            Common += Str1[i - 1];
                            lasti = i;
                            lastj = j;
                        }
                    }
                    else
                    {
                        L[i, j] = Math.Max(L[i, j - 1], L[i - 1, j]);
                    }
                }
            if (!string.IsNullOrEmpty(Common))
            {
                all.Add(Common);
            }

        }

        private string GetCommonString()
        {
            //1. 根据词频过滤
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (string str in all)
            {
                if (dic.ContainsKey(str))
                {
                    dic[str] += 1;
                }
                else
                {
                    dic[str] = 1;
                }
            }

            dic = GetSortByValueDict<string, int>(dic);

            // 2. 定位到原文，提取完整意思
            //int num1 = 0;
            //string str1 = string.Empty;
            //foreach (KeyValuePair<string, int> keyValuePair in dic)
            //{
            //    if (!keyValuePair.Key.Contains(",") && !keyValuePair.Key.Contains("。") && (!keyValuePair.Key.Contains("!") && !keyValuePair.Key.Contains("，")))
            //    {
            //        if (keyValuePair.Key.Length * keyValuePair.Value > num1)
            //        {
            //            str1 = keyValuePair.Key;
            //            num1 = keyValuePair.Key.Length * keyValuePair.Value;
            //        }
            //    }
            //}
            //return str1.Trim();

            int[] rankArray = new int[Content.Length];
            foreach (KeyValuePair<string, int> kv in dic)
            {
                string[] contentArray = Content.Split(new[] { kv.Key }, StringSplitOptions.None);
                int lastIndex = 0;
                for (int i = 0; i < contentArray.Length - 1; i++)
                {
                    lastIndex += contentArray[i].Length;
                    for (int j = 0; j < kv.Key.Length; j++)
                    {
                        rankArray[lastIndex + j] += kv.Value;
                    }
                    lastIndex += kv.Key.Length;
                }
            }

            int maxScore = 0;
            string result = string.Empty;

            int tempScore = 0;
            string temp = string.Empty;
            for (int i = 0; i < rankArray.Length; i++)
            {
                if (rankArray[i] != 0)
                {
                    tempScore += rankArray[i];
                    temp += Content[i];
                }
                else
                {
                    if (tempScore > maxScore)
                    {
                        maxScore = tempScore;
                        result = temp;
                    }
                    tempScore = 0;
                    temp = "";
                }
            }
            if (tempScore > maxScore)
            {
                maxScore = tempScore;
                result = temp;
            }

            return result.Replace("-", "").Trim(new char[] { ',', '。' });
        }

        /// <summary>
        /// 把一个字典俺value的顺序排序
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="distinctDict"></param>
        /// <returns></returns>
        private Dictionary<K, V> GetSortByValueDict<K, V>(IDictionary<K, V> distinctDict)
        {
            //用于给tempDict.Values排序的临时数组
            V[] tempSortList = new V[distinctDict.Count];
            distinctDict.Values.CopyTo(tempSortList, 0);
            Array.Sort(tempSortList); //给数据排序
            Array.Reverse(tempSortList);//反转

            //用于保存按value排序的字典
            Dictionary<K, V> sortByValueDict =
                new Dictionary<K, V>(distinctDict.Count);
            for (int i = 0; i < tempSortList.Length; i++)
            {
                foreach (KeyValuePair<K, V> pair in distinctDict)
                {
                    //比较两个泛型是否相当要用Equals，不能用==操作符
                    if (pair.Value.Equals(tempSortList[i]) && !sortByValueDict.ContainsKey(pair.Key))
                        sortByValueDict.Add(pair.Key, pair.Value);
                }
            }
            return sortByValueDict;
        }

    }
}
