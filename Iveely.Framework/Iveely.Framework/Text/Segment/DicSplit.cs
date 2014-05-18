using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Iveely.Framework.Text.Segment
{
    [Serializable]
    internal class SplitList
    {

        private ArrayList m_seg;

        //private Hashtable wordPos = new Hashtable();

        public int MaxLength;
        public int Count
        {
            get
            {
                return m_seg.Count;
            }
        }

        public SplitList()
        {
            m_seg = new ArrayList();
            MaxLength = 0;
            //Count = 0 ;
        }

        public void Add(object obj)
        {
            m_seg.Add(obj);
            if (MaxLength < obj.ToString().Length)
            {
                MaxLength = obj.ToString().Length;
            }
        }

        public object GetElem(int i)
        {
            if (i < this.Count)
                return m_seg[i];
            else
                return null;
        }

        public void SetElem(int i, object obj)
        {
            m_seg[i] = obj;
        }

        public bool Contains(object obj)
        {
            return m_seg.Contains(obj);
        }

        /// <summary>
        /// 按长度排序
        /// </summary>
        /// <param name="list"></param>
        public void Sort(SplitList list)
        {
            int max = 0;
            for (int i = 0; i < list.Count - 1; ++i)
            {
                max = i;
                for (int j = i + 1; j < list.Count; ++j)
                {

                    string str1 = list.GetElem(j).ToString();
                    string str2 = list.GetElem(max).ToString();
                    int l1;
                    int l2;
                    if (str1 == "null")
                        l1 = 0;
                    else
                        l1 = str1.Length;

                    if (str2 == "null")
                        l2 = 0;
                    else
                        l2 = str2.Length;

                    if (l1 > l2)
                        max = j;
                }
                object o = list.GetElem(max);
                list.SetElem(max, list.GetElem(i));
                list.SetElem(i, o);
            }
        }

        public void Sort()
        {
            Sort(this);
        }
    }

    [Serializable]
    public class DicSplit
    {

        private string _mDicPath = File.ReadAllText("Init\\Resources\\sDict.txt");
        private string _mNoisePath = File.ReadAllText("Init\\Resources\\sNoise.txt");
        private string _mNumberPath = File.ReadAllText("Init\\Resources\\sNumber.txt");
        private string _mWordPath = File.ReadAllText("Init\\Resources\\sWord.txt");
        private string _mPrefixPath = File.ReadAllText("Init\\Resources\\sPrefix.txt");
        private double _mEventTime = 0;

        private Hashtable _htWords;
        private ArrayList _alNoise;
        private ArrayList _alNumber;
        private ArrayList _alWord;
        private ArrayList _alPrefix;
        public string Result = "";

        private static DicSplit Split;

        /// <summary>
        /// 分隔符
        /// </summary>
        private string _mSeparator = "/";

        /// <summary>
        /// 用于验证汉字的正则表达式
        /// </summary>
        private string strChinese = "[\u4e00-\u9fa5]";


        /// <summary>
        /// 基本词典路径
        /// </summary>
        private string DicPath
        {
            get
            {
                return _mDicPath;
            }
            set
            {
                _mDicPath = value;
            }
        }

        /// <summary>
        /// 暂时无用
        /// </summary>
        private string NoisePath
        {
            get
            {
                return _mNoisePath;
            }
            set
            {
                _mNoisePath = value;
            }
        }

        /// <summary>
        /// 数字词典路径
        /// </summary>
        private string NumberPath
        {
            get
            {
                return _mNumberPath;
            }
            set
            {
                _mNumberPath = value;
            }
        }

        /// <summary>
        /// 字母词典路径
        /// </summary>
        private string WordPath
        {
            get
            {
                return _mWordPath;
            }
            set
            {
                _mWordPath = value;
            }
        }

        /// <summary>
        /// 姓名前缀字典 用于纠错姓名
        /// </summary>
        private string PrefixPath
        {
            get
            {
                return _mPrefixPath;
            }
            set
            {
                _mPrefixPath = value;
            }
        }


        /// <summary>
        /// 是否开启姓名纠错功能
        /// </summary>
        private bool EnablePrefix
        {
            get
            {
                if (_alPrefix.Count == 0)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value)
                    _alPrefix = LoadWords(PrefixPath, _alPrefix);
                else
                    _alPrefix = new ArrayList();
            }
        }

        /// <summary>
        /// 辅助函数,判断字符类型,0为未知,1为数字,2为字母,3为汉字,4为汉字数字
        /// </summary>
        /// <param name="p_Char"></param>
        /// <returns></returns>
        private int GetCharType(string p_Char)
        {
            int CharType = 0;
            //汉字数字＆阿拉伯字母
            if (_alNumber.Contains(p_Char))
                CharType = 1;
            //字母
            if (_alWord.Contains(p_Char))
                CharType = 2;
            //汉字
            if (_htWords.ContainsKey(p_Char))
                CharType += 3;

            return CharType;


        }



        /// <summary>
        /// 对加载的词典排序并重新写入
        /// </summary>
        /// <param name="Reload">是否重新加载</param>
        private void SortDic(bool Reload)
        {
            DateTime start = DateTime.Now;
            StreamWriter sw = new StreamWriter(DicPath, false, System.Text.Encoding.UTF8);

            IDictionaryEnumerator idEnumerator1 = _htWords.GetEnumerator();
            while (idEnumerator1.MoveNext())
            {
                IDictionaryEnumerator idEnumerator2 = ((Hashtable)idEnumerator1.Value).GetEnumerator();
                while (idEnumerator2.MoveNext())
                {
                    SplitList aa = (SplitList)idEnumerator2.Value;
                    aa.Sort();
                    for (int i = 0; i < aa.Count; i++)
                    {
                        if (aa.GetElem(i).ToString() == "null")
                            sw.WriteLine(idEnumerator1.Key.ToString() + idEnumerator2.Key.ToString());
                        else
                            sw.WriteLine(idEnumerator1.Key.ToString()
                                + idEnumerator2.Key.ToString() + aa.GetElem(i).ToString());
                    }
                }
            }
            sw.Close();

            //重新加载
            if (Reload)
                InitWordDics();

            TimeSpan duration = DateTime.Now - start;
            _mEventTime = duration.TotalMilliseconds;
        }

        /// <summary>
        /// 重载字典写入,默认Reload=false
        /// </summary>
        private void SortDic()
        {
            SortDic(false);
        }

        public static DicSplit GetInstance()
        {
            string serFile = "Init\\DicSplit.ser";
            if (Split == null && File.Exists(serFile))
            {
                Split = Text.Serializer.DeserializeFromFile<DicSplit>(serFile);
            }
            else if (Split == null)
            {
                Split = new DicSplit();
                Text.Serializer.SerializeToFile(Split, serFile);
            }
            return Split;
        }

        /// <summary>
        /// 操作用时
        /// 每次进行加载或分词动作后改属性表示为上一次动作所用时间
        /// 已精确到毫秒但分词操作在字符串教短时可能为0
        /// 改属性只读
        /// </summary>
        private double EventTime
        {
            get
            {
                return _mEventTime;
            }
        }

        /// <summary>
        /// 分隔符,默认为空格
        /// </summary>
        private string Separator
        {
            get
            {
                return _mSeparator;
            }
            set
            {
                if (value != "" && value != null)
                    _mSeparator = value;
            }
        }



        /// <summary>
        /// 构造方法
        /// </summary>
        private DicSplit(string DicPath, string NoisePath, string NumberPath, string WordPath)
        {

            _mWordPath = DicPath;
            _mWordPath = NoisePath;
            _mWordPath = NumberPath;
            _mWordPath = WordPath;
            this.InitWordDics();
        }
        private DicSplit()
        {
            this.InitWordDics();
            this.Separator = "/";
        }


        #region 加载词列表
        /// <summary>
        /// 加载词列表
        /// </summary>
        private void InitWordDics()
        {

            //DateTime start = DateTime.Now;

            _htWords = new Hashtable();
            string strChar1;
            string strChar2;

            //Console.WriteLine(strDicPath);

            // StreamReader reader = new StreamReader(DicPath, System.Text.Encoding.UTF8);
            //string strline = reader.ReadLine();

            string[] myStri = DicPath.Split('_');
            Hashtable father = _htWords;

            Hashtable child = new Hashtable();
            Hashtable forfather = _htWords;
            SplitList list;
            long i = 0;
            string strline = myStri[0];
            while (strline != null && strline.Trim() != "")
            {

                i++;

                strChar1 = strline.Substring(0, 1);
                strChar2 = strline.Substring(1, 1);
                if (!_htWords.ContainsKey(strChar1))
                {
                    father = new Hashtable();
                    _htWords.Add(strChar1, father);
                }
                else
                {
                    father = (Hashtable)_htWords[strChar1];
                }

                if (!father.ContainsKey(strChar2))
                {
                    list = new SplitList();
                    if (strline.Length > 2)
                        list.Add(strline.Substring(2));
                    else
                        list.Add("null");
                    father.Add(strChar2, list);
                }
                else
                {
                    list = (SplitList)father[strChar2];
                    if (strline.Length > 2)
                    {
                        list.Add(strline.Substring(2));
                    }
                    else
                    {
                        list.Add("null");
                    }
                    father[strChar2] = list;


                }
                _htWords[strChar1] = father;



                strline = myStri[i];
            }
            try
            {
                // reader.Close();
            }
            catch
            { }

            _alNoise = LoadWords(NoisePath, _alNoise);
            _alNumber = LoadWords(NumberPath, _alNumber);
            _alWord = LoadWords(WordPath, _alWord);
            _alPrefix = LoadWords(PrefixPath, _alPrefix);
            //alPrefix = new ArrayList() ;

            // TimeSpan duration = DateTime.Now - start;

            // m_EventTime = duration.TotalMilliseconds;
            //			Console.WriteLine("加载时间:" + duration.TotalMilliseconds);
            //			Console.WriteLine(this.htWords.Count) ;
            //			Console.WriteLine(length) ;
            //OutWords();
            //outNumbers();

        }

        #endregion

        #region 加载文本词组到ArrayList
        /// <summary>
        /// 加载文本词组到ArrayList
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public ArrayList LoadWords(string strPath, ArrayList list)
        {
            //StreamReader reader = new StreamReader(strPath, System.Text.Encoding.UTF8);
            strPath = strPath.Replace("\r\n", "_") + "_";
            list = new ArrayList();
            //string strline = reader.ReadLine();
            string strline = strPath.Substring(0, strPath.IndexOf('_'));
            while (strline != null)
            {
                list.Add(strline);
                strPath = strPath.Replace(strline + "_", "");
                if (strPath == "")
                {
                    break;
                }
                strline = strPath.Substring(0, strPath.IndexOf('_'));
            }
            try
            {
                // reader.Close();
            }
            catch
            { }
            return list;
        }
        #endregion

        #region 输出
        /// <summary>
        /// 输出词列表
        /// </summary>
        private void OutWords()
        {
            IDictionaryEnumerator idEnumerator1 = _htWords.GetEnumerator();
            while (idEnumerator1.MoveNext())
            {
                IDictionaryEnumerator idEnumerator2 = ((Hashtable)idEnumerator1.Value).GetEnumerator();
                while (idEnumerator2.MoveNext())
                {
                    SplitList aa = (SplitList)idEnumerator2.Value;
                    for (int i = 0; i < aa.Count; i++)
                    {
                        Console.WriteLine(idEnumerator1.Key.ToString()
                            + idEnumerator2.Key.ToString() + aa.GetElem(i).ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 输出ArrayList
        /// </summary>
        private void OutArrayList(ArrayList list)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(list[i].ToString());
            }
        }

        #endregion


        /// <summary>
        /// 分词过程
        /// </summary>
        /// <param name="strText">要分词的文本</param>
        /// <returns>分词后的文本</returns>
        public string[] Do(string strText)
        {
            string reText = "";
            strText = strText.Trim();
            int length = 0;
            //Console.WriteLine(strText.Length.ToString());

            if (_htWords == null || strText.Length < 3)
            {
                return new string[1] { strText };
            }

            DateTime start = DateTime.Now;
            bool word = false;
            bool number = false;
            //bool han = false ;
            string strLastWords = Separator;
            string strPrefix = "";
            int preFix = 0;
            string strLastChar = "";
            //遍历每一个字
            for (int i = 0; i < strText.Length - 1; i++)
            {
                #region 对于每一个字的处理过程

                string strChar1 = strText.Substring(i, 1);
                string strChar2 = strText.Substring(i + 1, 1).Trim();
                Hashtable h;
                SplitList l;
                bool yes;



                if (reText.Length > 0) strLastChar = reText.Substring(reText.Length - 1);
                //首先处理掉空格
                if (strChar1 == " ")
                {
                    if ((number || word) && strLastChar != Separator)
                        reText += this.Separator;
                    yes = true;
                }
                else
                    yes = false;

                #region 开始判断字符类型

                int CharType = GetCharType(strChar1);
                switch (CharType)
                {
                    case 1:
                        #region 如果数字
                        //如果数字的上一位是字母要和后面的数字分开
                        if (word)
                        {
                            reText += Separator;
                        }

                        strLastWords = "";
                        word = false;
                        number = true;
                        break;
                        #endregion
                    case 2:
                    case 5:
                        #region 如果是字母
                        if (number)
                        {
                            strLastWords = Separator;
                        }
                        else
                            strLastWords = "";

                        word = true;
                        number = false;
                        break;
                        #endregion
                    case 3:
                    case 4:
                        #region 如果是汉字
                        // 第一级哈希表是否包含关键字
                        // 假如包含处理第二级哈希表
                        #region 处理第二级哈希表

                        //首先看上一个字是不是字母
                        if (word)
                            reText += Separator;

                        #region 检测上一个是否是数字 test code
                        //检测上一个是否是数字
                        //这个过程是用于修正数字后的量词的
                        if (number && CharType != 4)
                        {
                            h = (Hashtable)_htWords["n"];
                            if (h.ContainsKey(strChar1))
                            {
                                l = (SplitList)h[strChar1];
                                if (l.Contains(strChar2))
                                {
                                    reText += strChar1 + strChar2 + Separator;
                                    yes = true;
                                    i++;

                                }
                                else if (l.Contains("null"))
                                {
                                    reText += strChar1 + Separator;

                                    yes = true;
                                }
                            }
                            else
                            {
                                if (reText.Substring(reText.Length - 1, 1) != Separator)
                                    reText += Separator;
                            }
                        }

                        #endregion

                        //非汉字数字的汉字
                        if (CharType == 3)
                        {
                            //当前为汉字
                            word = false;
                            number = false;

                            //汉字词分隔符为" "
                            strLastWords = Separator;
                        }
                        else
                        {
                            //汉字数字
                            strLastWords = "";
                            word = false;
                            number = true;
                        }
                        // 第二级哈希表取出
                        h = (Hashtable)_htWords[strChar1];
                        //第二级哈希表是否包含关键字
                        if (h.ContainsKey(strChar2))
                        {


                            #region  第二级包含关键字

                            //取出ArrayList对象
                            l = (SplitList)h[strChar2];

                            //遍历每一个对象 看是否能组合成词
                            for (int j = 0; j < l.Count; j++)
                            {
                                bool have = false;
                                string strChar3 = l.GetElem(j).ToString();

                                //对于每一个取出的词进行检测,看是否匹配
                                //长度保护
                                if ((strChar3.Length + i + 2) < strText.Length)
                                {
                                    //向i+2后取出m长度的字
                                    string strChar = strText.Substring(i + 2, strChar3.Length).Trim();
                                    if (strChar3 == strChar && !yes)
                                    {
                                        if (strPrefix != "")
                                        {
                                            reText += strPrefix + Separator;
                                            strPrefix = "";
                                            preFix = 0;
                                        }
                                        reText += strChar1 + strChar2 + strChar;

                                        i += strChar3.Length + 1;
                                        have = true;
                                        yes = true;
                                        break;
                                    }
                                }
                                else if ((strChar3.Length + i + 2) == strText.Length)
                                {
                                    string strChar = strText.Substring(i + 2).Trim();
                                    if (strChar3 == strChar && !yes)
                                    {
                                        if (strPrefix != "")
                                        {
                                            reText += strPrefix + Separator;
                                            strPrefix = "";
                                            preFix = 0;
                                        }
                                        reText += strChar1 + strChar2 + strChar;


                                        i += strChar3.Length + 1;
                                        have = true;
                                        yes = true;
                                        break;
                                    }
                                }

                                //}//end for m
                                if (!have && j == l.Count - 1 && l.Contains("null") && !yes)
                                {

                                    #region
                                    if (preFix == 1)
                                    {
                                        reText += strPrefix + strChar1 + strChar2;
                                        strPrefix = "";
                                        preFix = 0;
                                    }
                                    else if (preFix > 1)
                                    {
                                        reText += strPrefix + strLastWords + strChar1 + strChar2;
                                        strPrefix = "";
                                        preFix = 0;
                                    }
                                    else
                                    {
                                        if (CharType == 4) reText += strChar1 + strChar2;
                                        else reText += strChar1 + strChar2;
                                        strLastWords = this.Separator;
                                        number = false;
                                    }
                                    #endregion


                                    i++;
                                    yes = true;
                                    break;
                                }
                                else if (have)
                                {
                                    break;
                                }

                            }//end for j
                            #endregion

                            //如果没有匹配还可能有一种情况
                            //这个词语只有两个字
                            //以这两个字开头的词语不存在
                            if (!yes && l.Contains("null"))
                            {
                                #region
                                if (preFix == 1)
                                {
                                    reText += strPrefix + strChar1 + strChar2;
                                    strPrefix = "";
                                    preFix = 0;
                                }
                                else if (preFix > 1)
                                {
                                    reText += strPrefix + strLastWords + strChar1 + strChar2;
                                    strPrefix = "";
                                    preFix = 0;
                                }
                                else
                                {
                                    if (CharType == 4) reText += strChar1 + strChar2;
                                    else reText += strChar1 + strChar2;
                                    strLastWords = this.Separator;
                                    number = false;
                                }
                                #endregion
                                i++;
                                yes = true;
                            }

                            if (reText.Length > 0) strLastChar = reText.Substring(reText.Length - 1);
                            if (CharType == 4 && GetCharType(strLastChar) == 4)
                            {
                                number = true;
                            }
                            else if (strLastChar != this.Separator)
                                reText += this.Separator;

                        }//end if h

                        #endregion
                        break;
                        #endregion
                    default:
                        #region 未知字符,可能是生僻字,也可能是标点符合之类

                        if (word && !yes)
                        {
                            reText += Separator;
                        }
                        else if (number && !yes)
                        {
                            if (reText.Substring(reText.Length - 1, 1) != Separator)
                                reText += Separator;

                        }
                        number = false;
                        word = false;
                        strLastWords = this.Separator;
                        break;

                        #endregion
                }

                #endregion

                #endregion

                if (!yes && number || !yes && word)
                {
                    reText += strChar1;
                    yes = true;
                }

                if (!yes)
                {
                    #region 处理姓名问题 test code
                    if (preFix == 0)
                    {
                        if (_alPrefix.Contains(strChar1 + strChar2))
                        {
                            i++;
                            strPrefix = strChar1 + strChar2;
                            preFix++;

                        }
                        else if (_alPrefix.Contains(strChar1))
                        {
                            if (!number)
                            {
                                strPrefix = strChar1;
                                preFix++;
                            }
                            else
                            {
                                reText += strChar1 + strLastWords;
                                number = false;
                                word = false;
                            }
                        }
                        else
                        {
                            #region
                            if (preFix == 3)
                            {
                                reText += strPrefix + Separator + strChar1 + Separator;
                                strPrefix = "";
                                preFix = 0;

                            }
                            else if (preFix > 0)
                            {
                                if (Regex.IsMatch(strChar1, strChinese))
                                {
                                    strPrefix += strChar1;
                                    preFix++;
                                }
                                else
                                {
                                    reText += strPrefix + Separator + strChar1 + Separator;
                                    strPrefix = "";
                                    preFix = 0;
                                }

                            }
                            else
                            {
                                reText += strChar1 + strLastWords;
                                number = false;
                                word = false;
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        #region
                        if (preFix == 3)
                        {
                            reText += strPrefix + Separator + strChar1 + Separator;
                            strPrefix = "";
                            preFix = 0;

                        }
                        else if (preFix > 0)
                        {
                            if (Regex.IsMatch(strChar1, strChinese))
                            {
                                strPrefix += strChar1;
                                preFix++;
                            }
                            else
                            {
                                reText += strPrefix + Separator + strChar1 + Separator;
                                strPrefix = "";
                                preFix = 0;
                            }

                        }
                        else
                        {
                            reText += strChar1 + strLastWords;
                            number = false;
                        }
                        #endregion
                    }
                    #endregion
                }

                length = i;
            }


            //最后防止最后一个字的丢失
            if (length < strText.Length - 1)
            {
                string strLastChar1 = strText.Substring(strText.Length - 1).Trim();
                string strLastChar2 = strText.Substring(strText.Length - 2).Trim();

                if (reText.Length > 0) strLastChar = reText.Substring(reText.Length - 1);
                if (preFix != 0)
                {
                    reText += strPrefix + strLastChar1;
                }
                else
                {
                    switch (GetCharType(strLastChar1))
                    {
                        case 1:
                            if (strLastChar1 != "." && strLastChar1 != "．")
                                reText += strLastChar1;
                            else
                            {
                                if (reText.Substring(reText.Length - 1, 1) == Separator)
                                    reText += strLastChar1;
                                else
                                    reText += Separator + strLastChar1;
                            }
                            break;
                        case 2:
                        case 5:
                            if (_alWord.Contains(strLastChar2))
                                reText += strLastChar1;
                            break;
                        case 3:
                        case 4:
                            if ((number || word) && strLastChar != Separator)
                                reText += Separator + strLastChar1;
                            else
                                reText += strLastChar1;
                            break;
                        default:
                            if (strLastChar != Separator)
                                reText += Separator + strLastChar1;
                            else
                                reText += strLastChar1;
                            break;

                    }
                }
                if (reText.Length > 0) strLastChar = (reText.Substring(reText.Length - 1));
                if (strLastChar != this.Separator) reText += this.Separator;

            }


            //计算时间
            TimeSpan duration = DateTime.Now - start;
            _mEventTime = duration.TotalMilliseconds;
            return reText.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
