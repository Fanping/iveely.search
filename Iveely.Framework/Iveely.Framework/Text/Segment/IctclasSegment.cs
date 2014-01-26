/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpICTCLAS;

namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// 中科院分词
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    [Serializable]
    public class IctclasSegment
    {

        private readonly int _nKind = 1;

        private readonly WordSegment _wordSegment;

        private static IctclasSegment _innerSeg;

        public static IctclasSegment GetInstance()
        {
            if (_innerSeg == null)
            {
                string path = "Init\\Corpus\\";
                if (Directory.Exists(path))
                {
                    _innerSeg = new IctclasSegment(path);
                }
            }
            return _innerSeg;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dictPath"></param>
        /// <param name="nKind"></param>
        private IctclasSegment(string dictPath, int nKind = 1)
        {
            this._nKind = nKind;
            this._wordSegment = new WordSegment();
            //wordSegment.PersonRecognition = false;
            //wordSegment.PlaceRecognition = false;
            //wordSegment.TransPersonRecognition = false;

            //---------- 订阅分词过程中的事件 ----------
            //_wordSegment.OnSegmentEvent += new SegmentEventHandler(this.OnSegmentEventHandler);
            _wordSegment.InitWordSegment(dictPath);
        }

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<WordResult[]> SplitToArray(string sentence)
        {
            return _wordSegment.Segment(sentence, _nKind);
        }

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public string SplitToString(string sentence)
        {
            List<WordResult[]> result = SplitToArray(sentence);
            string buffer = string.Empty;
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 1; j < result[i].Length - 1; j++)
                {
                    buffer += result[i][j].sWord + " ";
                }
            }
            return buffer;
        }

        public string SplitToSemantic(string sentence)
        {
            List<WordResult[]> results = SplitToArray(sentence);
            string textResult = string.Empty;
            for (int i = 0; i < results.Count; i++)
            {
                for (int j = 1; j < results[i].Length - 1; j++)
                {
                   textResult+=results[i][j].sWord + " " + Utility.GetPOSString(results[i][j].nPOS)+" ";
                }
            }
            return textResult.Trim();
        }

        /// <summary>
        /// 获取分词后的词性标注
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<string> GetSemantic(string sentence)
        {
            List<WordResult[]> results = SplitToArray(sentence);
            List<string> semanticList = new List<string>();
            for (int i = 0; i < results.Count; i++)
            {
                for (int j = 1; j < results[i].Length - 1; j++)
                {
                    semanticList.Add(Utility.GetPOSString(results[i][j].nPOS));
                }
            }
            return semanticList;
        }

            ///// <summary>
        ///// 中间结果输出
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnSegmentEventHandler(object sender, SegmentEventArgs e)
        //{
        //    switch (e.Stage)
        //    {
        //        case SegmentStage.BeginSegment:
        //            Console.WriteLine("\r\n==== 原始句子：\r\n");
        //            Console.WriteLine(e.Info + "\r\n");
        //            break;
        //        case SegmentStage.AtomSegment:
        //            Console.WriteLine("\r\n==== 原子切分：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.GenSegGraph:
        //            Console.WriteLine("\r\n==== 生成 segGraph：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.GenBiSegGraph:
        //            Console.WriteLine("\r\n==== 生成 biSegGraph：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.NShortPath:
        //            Console.WriteLine("\r\n==== NShortPath 初步切分的到的 N 个结果：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.BeforeOptimize:
        //            Console.WriteLine("\r\n==== 经过数字、日期合并等策略处理后的 N 个结果：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.OptimumSegment:
        //            Console.WriteLine("\r\n==== 将 N 个结果归并入OptimumSegment：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.PersonAndPlaceRecognition:
        //            Console.WriteLine("\r\n==== 加入对姓名、翻译人名以及地名的识别：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.BiOptimumSegment:
        //            Console.WriteLine("\r\n==== 对加入对姓名、地名的OptimumSegment生成BiOptimumSegment：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //        case SegmentStage.FinishSegment:
        //            Console.WriteLine("\r\n==== 最终识别结果：\r\n");
        //            Console.WriteLine(e.Info);
        //            break;
        //    }
        //}

#if DEBUG
        [TestMethod]
        public static string TestSplit(string str)
        {
            IctclasSegment segment = IctclasSegment.GetInstance();
            string result = segment.SplitToString(str);
            return result;
        }
#endif
    }
}
