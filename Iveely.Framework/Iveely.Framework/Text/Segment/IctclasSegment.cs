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
using System.Runtime.InteropServices;
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
        /// <summary>
        /// 标注集类型。
        /// </summary>
        public enum NlpirMap
        {
            /// <summary>
            /// 计算所一级标注集。
            /// </summary>
            IctPosMapFirst = 1,

            /// <summary>
            /// 计算所二级标注集。
            /// </summary>
            IctPosMapSecond = 0,

            /// <summary>
            /// 北大一级标注集。
            /// </summary>
            PkuPosMapFirst = 3,

            /// <summary>
            /// 北大二级标注集。
            /// </summary>
            PkuPosMapSecond = 2
        }

        /// <summary>
        /// 编码类型。
        /// </summary>
        public enum NlpirCode
        {
            /// <summary>
            /// GBK编码。
            /// </summary>
            GbkCode = 0,

            /// <summary>
            /// UTF8编码。
            /// </summary>
            Utf8Code = 1,

            /// <summary>
            /// BIG5编码。
            /// </summary>
            Big5Code = 2,

            /// <summary>
            /// GBK编码，里面包含繁体字。
            /// </summary>
            GbkFantiCode = 3
        }

        /// <summary>
        /// 分词结果结构体。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct result_t
        {
            /// <summary>
            /// 词语在输入句子中的开始位置。
            /// </summary>
            public int start;

            /// <summary>
            /// 词语的长度。
            /// </summary>
            public int length;

            /// <summary>
            /// 词性ID值，可以快速的获取词性表。
            /// </summary>
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string sPos;

            /// <summary>
            /// 词性标注的编号。
            /// </summary>
            public int POS_id;

            /// <summary>
            /// 该词的内部ID号，如果是未登录词，设成0或者-1。
            /// </summary>
            public int word_ID;

            /// <summary>
            /// 区分用户词典，1是用户词典中的词，0非用户词典中的词。
            /// </summary>
            public int word_type;

            /// <summary>
            /// 权值。
            /// </summary>
            public int weight;
        }

        /// <summary>
        /// 分词类。
        /// </summary>
        public class NLPIR
        {
            #region 对变量进行声明
            private static bool _Init = false;
            private static bool _NWIStart = false;
            private static bool _NWIComplete = false;
            private const string rootDir = @"Init\";
            #endregion

            #region 对函数进行声明和包装
            #region 预判断
            private static void JudgeInit()
            {
                if (!_Init) throw new Exception("未进行初始化！");
            }

            private static void JudgeNWIStart()
            {
                JudgeInit();
                if (!_NWIStart) throw new Exception("未启动新词识别！");
            }

            private static void JudgeNWIComplete()
            {
                JudgeInit();
                if (!_NWIComplete) throw new Exception("未结束新词识别！");
            }
            #endregion

            #region 初始化、退出
            /// <summary>
            /// 初始化。
            /// </summary>
            /// <param name="sInitDirPath">Data所在目录。</param>
            /// <param name="encoding">编码类型。</param>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Init")]
            private static extern bool NLPIR_Init(string sInitDirPath, int encoding = (int)NlpirCode.GbkCode);

            /// <summary>
            /// 初始化，编码类型为GBK_CODE。
            /// </summary>
            /// <param name="sInitDirPath">Data所在目录。</param>
            /// <returns>是否执行成功。</returns>
            public static bool Init(string sInitDirPath)
            {
                _Init = NLPIR_Init(sInitDirPath);
                return _Init;
            }
            /// <summary>
            /// 初始化。
            /// </summary>
            /// <param name="sInitDirPath">Data所在目录。</param>
            /// <param name="encoding">编码类型。</param>
            /// <returns>是否执行成功。</returns>
            public static bool Init(string sInitDirPath, NlpirCode encoding)
            {
                _Init = NLPIR_Init(sInitDirPath, (int)encoding);
                return _Init;
            }

            /// <summary>
            /// 退出并释放资源。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Exit")]
            private static extern bool NLPIR_Exit();
            /// <summary>
            /// 退出并释放资源。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            public static bool Exit()
            {
                _Init = false;
                return NLPIR_Exit();
            }
            #endregion

            #region 分词操作
            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="bPOStagged">是否进行词性标注。</param>
            /// <returns>处理结果。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcess")]
            private static extern IntPtr NLPIR_ParagraphProcess(string sParagraph, int bPOStagged = 1);
            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="bPOStagged">是否进行词性标注。</param>
            /// <returns>处理结果。</returns>
            public static string ParagraphProcess(string sParagraph, bool bPOStagged)
            {
                JudgeInit();
                IntPtr intPtr = NLPIR_ParagraphProcess(sParagraph, bPOStagged ? 1 : 0);
                return Marshal.PtrToStringAnsi(intPtr);
            }

            /// <summary>
            /// 处理文本文件。
            /// </summary>
            /// <param name="sSrcFilename">源文件。</param>
            /// <param name="sDestFilename">目标文件。</param>
            /// <param name="bPOStagged">是否进行词性标注。</param>
            /// <returns>执行成功返回处理速度；否则返回0。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcess")]
            private static extern double NLPIR_FileProcess(
                string sSrcFilename, string sDestFilename, int bPOStagged = 1);
            /// <summary>
            /// 处理文本文件。
            /// </summary>
            /// <param name="sSrcFilename">源文件。</param>
            /// <param name="sDestFilename">目标文件。</param>
            /// <param name="bPOStagged">是否进行词性标注。</param>
            /// <returns>执行成功返回处理速度；否则返回0。</returns>
            public static double FileProcess(string sSrcFilename, string sDestFilename, bool bPOStagged)
            {
                JudgeInit();
                return NLPIR_FileProcess(sSrcFilename, sDestFilename, bPOStagged ? 1 : 0);
            }

            /// <summary>
            /// 处理文本内容，获取分词数。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>分词数。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetParagraphProcessAWordCount")]
            private static extern int NLPIR_GetParagraphProcessAWordCount(string sParagraph);
            /// <summary>
            /// 处理文本内容，获取分词数。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>分词数。</returns>
            public static int GetParagraphProcessAWordCount(string sParagraph)
            {
                JudgeInit();
                return NLPIR_GetParagraphProcessAWordCount(sParagraph);
            }

            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="nResultCount">分词数。</param>
            /// <returns>分词结果数组。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessA")]
            private static extern IntPtr NLPIR_ParagraphProcessA(string sParagraph, out int nResultCount);
            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>分词结果数组。</returns>
            public static result_t[] ParagraphProcessA(string sParagraph)
            {
                JudgeInit();
                int nCount = 0;
                IntPtr intPtr = NLPIR_ParagraphProcessA(sParagraph, out nCount);
                result_t[] results = new result_t[nCount];
                for (int i = 0; i < nCount; i++, intPtr = new IntPtr(
                    intPtr.ToInt32() + Marshal.SizeOf(typeof(result_t))))
                {
                    results[i] = (result_t)Marshal.PtrToStructure(intPtr, typeof(result_t));
                }
                return results;
            }

            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="nCount">分词数。</param>
            /// <param name="results">分词结果数组。</param>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessAW")]
            private static extern void NLPIR_ParagraphProcessAW(
                int nCount, [Out, MarshalAs(UnmanagedType.LPArray)] result_t[] result);
            /// <summary>
            /// 处理文本内容。
            /// </summary>
            /// <param name="nCount">分词数。</param>
            /// <returns>分词结果数组。</returns>
            public static result_t[] ParagraphProcessAW(int nCount)
            {
                JudgeInit();
                result_t[] results = new result_t[nCount];
                NLPIR_ParagraphProcessAW(nCount, results);
                return results;
            }
            #endregion

            #region 用户自定义词操作
            /// <summary>
            /// 导入用户自定义词典。
            /// 经测试没有写到磁盘，下次启动程序时需重新导入，即使调用NLPIR_SaveTheUsrDic。
            /// </summary>
            /// <param name="sFilename">用户自定义词典文件名（文本文件）。</param>
            /// <returns>用户自定义词数。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ImportUserDict")]
            private static extern int NLPIR_ImportUserDict(string sFilename);
            /// <summary>
            /// 导入用户自定义词典。
            /// 经测试没有写到磁盘，下次启动程序时需重新导入，即使调用NLPIR_SaveTheUsrDic。
            /// </summary>
            /// <param name="sFilename">用户自定义词典文件名（文本文件）。</param>
            /// <returns>用户自定义词数。</returns>
            public static int ImportUserDict(string sFilename)
            {
                JudgeInit();
                return NLPIR_ImportUserDict(sFilename);
            }

            /// <summary>
            /// 添加用户自定义词，格式为词+空格+词性，例“在国内 kkk”，不指定词性，默认为n。
            /// 若要下次启动程序时仍然有效，需执行NLPIR_SaveTheUsrDic。
            /// </summary>
            /// <param name="sWord">用户自定义词。</param>
            /// <returns>执行成功返回1；否则返回0。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_AddUserWord")]
            private static extern int NLPIR_AddUserWord(string sWord);
            /// <summary>
            /// 添加用户自定义词，格式为词+空格+词性，例“在国内 kkk”，不指定词性，默认为n。
            /// 若要下次启动程序时仍然有效，需执行SaveTheUsrDic。
            /// </summary>
            /// <param name="sWord">用户自定义词。</param>
            /// <returns>是否执行成功。</returns>
            public static bool AddUserWord(string sWord)
            {
                JudgeInit();
                return NLPIR_AddUserWord(sWord) == 1;
            }

            /// <summary>
            /// 删除用户自定义词，不能指定词性。
            /// 若要下次启动程序时仍然有效，需执行NLPIR_SaveTheUsrDic。
            /// </summary>
            /// <param name="sWord">用户自定义词。</param>
            /// <returns>执行成功返回用户自定义词句柄；否则返回-1。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_DelUsrWord")]
            private static extern int NLPIR_DelUsrWord(string sWord);
            /// <summary>
            /// 删除用户自定义词，不能指定词性。
            /// 若要下次启动程序时仍然有效，需执行SaveTheUsrDic。
            /// </summary>
            /// <param name="sWord">用户自定义词。</param>
            /// <returns>是否执行成功。</returns>
            public static bool DelUsrWord(string sWord)
            {
                JudgeInit();
                return NLPIR_DelUsrWord(sWord) != -1;
            }

            /// <summary>
            /// 保存用户自定义词到磁盘。
            /// </summary>
            /// <returns>执行成功返回1；否则返回0。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_SaveTheUsrDic")]
            private static extern int NLPIR_SaveTheUsrDic();
            /// <summary>
            /// 保存用户自定义词到磁盘。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            public static bool SaveTheUsrDic()
            {
                JudgeInit();
                return NLPIR_SaveTheUsrDic() == 1;
            }
            #endregion

            #region 新词操作
            /// <summary>
            /// 启动新词识别。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Start")]
            private static extern bool NLPIR_NWI_Start();
            /// <summary>
            /// 启动新词识别。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            public static bool NWI_Start()
            {
                JudgeInit();
                _NWIStart = NLPIR_NWI_Start();
                // 此处不能用_NWIComplete = ！_NWIStart。
                if (_NWIStart) _NWIComplete = false;
                return _NWIStart;
            }

            /// <summary>
            /// 新词识别添加内容结束，需要在运行NLPIR_NWI_Start()之后才有效。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Complete")]
            private static extern bool NLPIR_NWI_Complete();
            /// <summary>
            /// 新词识别添加内容结束，需要在运行NWI_Start()之后才有效。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            public static bool NWI_Complete()
            {
                JudgeNWIStart();
                _NWIStart = false;
                _NWIComplete = NLPIR_NWI_Complete();
                return _NWIComplete;
            }

            /// <summary>
            /// 往新词识别系统中添加待识别新词的文本文件，可反复添加，需要在运行NLPIR_NWI_Start()之后才有效。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddFile")]
            private static extern bool NLPIR_NWI_AddFile(string sFilename);
            /// <summary>
            /// 往新词识别系统中添加待识别新词的文本文件，可反复添加，需要在运行NWI_Start()之后才有效。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <returns>是否执行成功。</returns>
            public static bool NWI_AddFile(string sFilename)
            {
                JudgeNWIStart();
                return NLPIR_NWI_AddFile(sFilename);
            }

            /// <summary>
            /// 往新词识别系统中添加待识别新词的文本内容，可反复添加，需要在运行NLPIR_NWI_Start()之后才有效。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddMem")]
            private static extern bool NLPIR_NWI_AddMem(string sParagraph);
            /// <summary>
            /// 往新词识别系统中添加待识别新词的文本内容，可反复添加，需要在运行NWI_Start()之后才有效。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>是否执行成功。</returns>
            public static bool NWI_AddMem(string sParagraph)
            {
                JudgeNWIStart();
                return NLPIR_NWI_AddMem(sParagraph);
            }

            /// <summary>
            /// 获取新词识别的结果，需要在运行NLPIR_NWI_Complete()之后才有效。
            /// </summary>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_GetResult")]
            private static extern IntPtr NLPIR_NWI_GetResult(bool bWeightOut = false);
            /// <summary>
            /// 获取新词识别的结果，需要在运行NWI_Complete()之后才有效。
            /// </summary>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>是否执行成功。</returns>
            public static string NWI_GetResult(bool bWeightOut)
            {
                JudgeNWIComplete();
                IntPtr intPtr = NLPIR_NWI_GetResult(bWeightOut);
                return Marshal.PtrToStringAnsi(intPtr);
            }

            /// <summary>
            /// 将新词识别结果导入到用户词典中，需要在运行NLPIR_NWI_Complete()之后才有效。
            /// 经测试该函数会自动将结果写入磁盘，无需执行SaveTheUsrDic。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Result2UserDict")]
            private static extern bool NLPIR_NWI_Result2UserDict();
            /// <summary>
            /// 将新词识别结果导入到用户词典中，需要在运行NWI_Complete()之后才有效。
            /// 经测试该函数会自动将结果写入磁盘，无需执行SaveTheUsrDic。
            /// </summary>
            /// <returns>是否执行成功。</returns>
            public static bool NWI_Result2UserDict()
            {
                JudgeNWIComplete();
                return NLPIR_NWI_Result2UserDict();
            }
            #endregion

            #region 直接获取关键词或新词。
            /// <summary>
            /// 获取文本关键字。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="nMaxKeyLimit">关键字最大数，实际输出关键字数为nMaxKeyLimit+1。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>关键字列表。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetKeyWords")]
            private static extern IntPtr NLPIR_GetKeyWords(
                string sParagraph, int nMaxKeyLimit = 50, bool bWeightOut = false);
            /// <summary>
            /// 获取文本关键字。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="nMaxKeyLimit">关键字最大数。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>关键字列表。</returns>
            public static string GetKeyWords(string sParagraph, int nMaxKeyLimit, bool bWeightOut)
            {
                JudgeInit();
                IntPtr intPtr = NLPIR_GetKeyWords(sParagraph, nMaxKeyLimit - 1, bWeightOut);
                return Marshal.PtrToStringAnsi(intPtr);
            }

            /// <summary>
            /// 获取文本文件关键字。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <param name="nMaxKeyLimit">关键字最大数，实际输出关键字数为nMaxKeyLimit+1。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>关键字列表。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileKeyWords")]
            private static extern IntPtr NLPIR_GetFileKeyWords(
                string sFilename, int nMaxKeyLimit = 50, bool bWeightOut = false);
            /// <summary>
            /// 获取文本文件关键字。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <param name="nMaxKeyLimit">关键字最大数。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>关键字列表。</returns>
            public static string GetFileKeyWords(string sFilename, int nMaxKeyLimit, bool bWeightOut)
            {
                JudgeInit();
                IntPtr intPtr = NLPIR_GetFileKeyWords(sFilename, nMaxKeyLimit - 1, bWeightOut);
                return Marshal.PtrToStringAnsi(intPtr);
            }

            /// <summary>
            /// 获取文本新词。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="nMaxNewLimit">新词最大数，实际输出新词数为nMaxNewLimit+1。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>新词列表。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetNewWords")]
            private static extern IntPtr NLPIR_GetNewWords(
                string sParagraph, int nMaxNewLimit = 50, bool bWeightOut = false);
            /// <summary>
            /// 获取文本新词。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <param name="nMaxNewLimit">新词最大数。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>新词列表。</returns>
            public static string GetNewWords(string sParagraph, int nMaxNewLimit, bool bWeightOut)
            {
                JudgeInit();
                IntPtr intPtr = NLPIR_GetNewWords(sParagraph, nMaxNewLimit - 1, bWeightOut);
                return Marshal.PtrToStringAnsi(intPtr);
            }

            /// <summary>
            /// 获取文本文件新词。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <param name="nMaxNewLimit">新词最大数，实际输出新词数为nMaxNewLimit+1。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>新词列表。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileNewWords")]
            private static extern IntPtr NLPIR_GetFileNewWords(
                string sFilename, int nMaxNewLimit = 50, bool bWeightOut = false);
            /// <summary>
            /// 获取文本文件新词。
            /// </summary>
            /// <param name="sFilename">文本文件名。</param>
            /// <param name="nMaxNewLimit">新词最大数。</param>
            /// <param name="bWeightOut">是否输出权值。</param>
            /// <returns>新词列表。</returns>
            public static string GetFileNewWords(string sFilename, int nMaxNewLimit, bool bWeightOut)
            {
                JudgeInit();
                IntPtr intPtr = NLPIR_GetFileNewWords(sFilename, nMaxNewLimit - 1, bWeightOut);
                return Marshal.PtrToStringAnsi(intPtr);
            }
            #endregion

            #region 其他
            /// <summary>
            /// 设置标注集。
            /// </summary>
            /// <param name="nPOSmap">标注集。</param>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_SetPOSmap")]
            private static extern void NLPIR_SetPOSmap(int nPOSmap);
            /// <summary>
            /// 设置标注集。
            /// </summary>
            /// <param name="nPOSmap">标注集。</param>
            public static void SetPOSmap(NlpirMap nPOSmap)
            {
                JudgeInit();
                NLPIR_SetPOSmap((int)nPOSmap);
            }

            /// <summary>
            /// 从文本提取指纹信息。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>执行成功返回指纹信息；否则返回0。</returns>
            [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FingerPrint")]
            private static extern ulong NLPIR_FingerPrint(string sParagraph);

            /// <summary>
            /// 从文本提取指纹信息。
            /// </summary>
            /// <param name="sParagraph">文本内容。</param>
            /// <returns>执行成功返回指纹信息；否则返回0。</returns>
            public static ulong FingerPrint(string sParagraph)
            {
                JudgeInit();
                return NLPIR_FingerPrint(sParagraph);
            }
            #endregion
            #endregion

            /// <summary>
            /// 用法举例。
            /// </summary>
            public static void Example()
            {
                Init(rootDir);
              
                int count = GetParagraphProcessAWordCount("怎么去科技馆");
                result_t[] results = ParagraphProcessAW(count);
                int i = 1;
                byte[] bytes = Encoding.Default.GetBytes("怎么去科技馆");
                foreach (result_t r in results)
                {
                    Console.WriteLine(r.sPos + "," + Encoding.Default.GetString(bytes, r.start, r.length));
                    Console.WriteLine("No.{0}:start:{1},length:{2},POS_ID:{3}\n" +
                        "Word_ID:{4},UserDefine:{5},Type:{6}",
                        i++, r.start, r.length, r.POS_id, r.word_ID, r.weight, r.sPos);
                }

                Exit();
            }
        }

        #region 对变量进行声明
        private static bool _Init = false;
        private static bool _NWIStart = false;
        private static bool _NWIComplete = false;
        private const string rootDir = @".\Init\";
        #endregion

        #region 预判断
        private static void JudgeInit()
        {
            if (!_Init) throw new Exception("未进行初始化！");
        }

        private static void JudgeNWIStart()
        {
            JudgeInit();
            if (!_NWIStart) throw new Exception("未启动新词识别！");
        }

        private static void JudgeNWIComplete()
        {
            JudgeInit();
            if (!_NWIComplete) throw new Exception("未结束新词识别！");
        }
        #endregion

        #region 初始化、退出
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="sInitDirPath">Data所在目录。</param>
        /// <param name="encoding">编码类型。</param>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Init")]
        private static extern bool NLPIR_Init(string sInitDirPath, int encoding = (int)NlpirCode.GbkCode);

        /// <summary>
        /// 初始化，编码类型为GBK_CODE。
        /// </summary>
        /// <param name="sInitDirPath">Data所在目录。</param>
        /// <returns>是否执行成功。</returns>
        public static bool Init(string sInitDirPath)
        {
            _Init = NLPIR_Init(sInitDirPath);
            return _Init;
        }
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="sInitDirPath">Data所在目录。</param>
        /// <param name="encoding">编码类型。</param>
        /// <returns>是否执行成功。</returns>
        public static bool Init(string sInitDirPath, NlpirCode encoding)
        {
            _Init = NLPIR_Init(sInitDirPath, (int)encoding);
            return _Init;
        }

        /// <summary>
        /// 退出并释放资源。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Exit")]
        private static extern bool NLPIR_Exit();
        /// <summary>
        /// 退出并释放资源。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        public static bool Exit()
        {
            _Init = false;
            return NLPIR_Exit();
        }
        #endregion

        #region 分词操作
        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="bPOStagged">是否进行词性标注。</param>
        /// <returns>处理结果。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcess")]
        private static extern IntPtr NLPIR_ParagraphProcess(string sParagraph, int bPOStagged = 1);
        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="bPOStagged">是否进行词性标注。</param>
        /// <returns>处理结果。</returns>
        public static string ParagraphProcess(string sParagraph, bool bPOStagged)
        {
            JudgeInit();
            IntPtr intPtr = NLPIR_ParagraphProcess(sParagraph, bPOStagged ? 1 : 0);
            return Marshal.PtrToStringAnsi(intPtr);
        }

        /// <summary>
        /// 处理文本文件。
        /// </summary>
        /// <param name="sSrcFilename">源文件。</param>
        /// <param name="sDestFilename">目标文件。</param>
        /// <param name="bPOStagged">是否进行词性标注。</param>
        /// <returns>执行成功返回处理速度；否则返回0。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcess")]
        private static extern double NLPIR_FileProcess(
            string sSrcFilename, string sDestFilename, int bPOStagged = 1);
        /// <summary>
        /// 处理文本文件。
        /// </summary>
        /// <param name="sSrcFilename">源文件。</param>
        /// <param name="sDestFilename">目标文件。</param>
        /// <param name="bPOStagged">是否进行词性标注。</param>
        /// <returns>执行成功返回处理速度；否则返回0。</returns>
        public static double FileProcess(string sSrcFilename, string sDestFilename, bool bPOStagged)
        {
            JudgeInit();
            return NLPIR_FileProcess(sSrcFilename, sDestFilename, bPOStagged ? 1 : 0);
        }

        /// <summary>
        /// 处理文本内容，获取分词数。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>分词数。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetParagraphProcessAWordCount")]
        private static extern int NLPIR_GetParagraphProcessAWordCount(string sParagraph);
        /// <summary>
        /// 处理文本内容，获取分词数。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>分词数。</returns>
        public static int GetParagraphProcessAWordCount(string sParagraph)
        {
            JudgeInit();
            return NLPIR_GetParagraphProcessAWordCount(sParagraph);
        }

        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="nResultCount">分词数。</param>
        /// <returns>分词结果数组。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessA")]
        private static extern IntPtr NLPIR_ParagraphProcessA(string sParagraph, out int nResultCount);
        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>分词结果数组。</returns>
        public static result_t[] ParagraphProcessA(string sParagraph)
        {
            JudgeInit();
            int nCount = 0;
            IntPtr intPtr = NLPIR_ParagraphProcessA(sParagraph, out nCount);
            result_t[] results = new result_t[nCount];
            for (int i = 0; i < nCount; i++, intPtr = new IntPtr(
                intPtr.ToInt32() + Marshal.SizeOf(typeof(result_t))))
            {
                results[i] = (result_t)Marshal.PtrToStructure(intPtr, typeof(result_t));
            }
            return results;
        }

        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="nCount">分词数。</param>
        /// <param name="results">分词结果数组。</param>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessAW")]
        private static extern void NLPIR_ParagraphProcessAW(
            int nCount, [Out, MarshalAs(UnmanagedType.LPArray)] result_t[] result);
        /// <summary>
        /// 处理文本内容。
        /// </summary>
        /// <param name="nCount">分词数。</param>
        /// <returns>分词结果数组。</returns>
        public static result_t[] ParagraphProcessAW(int nCount)
        {
            JudgeInit();
            result_t[] results = new result_t[nCount];
            NLPIR_ParagraphProcessAW(nCount, results);
            return results;
        }
        #endregion

        #region 用户自定义词操作
        /// <summary>
        /// 导入用户自定义词典。
        /// 经测试没有写到磁盘，下次启动程序时需重新导入，即使调用NLPIR_SaveTheUsrDic。
        /// </summary>
        /// <param name="sFilename">用户自定义词典文件名（文本文件）。</param>
        /// <returns>用户自定义词数。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ImportUserDict")]
        private static extern int NLPIR_ImportUserDict(string sFilename);
        /// <summary>
        /// 导入用户自定义词典。
        /// 经测试没有写到磁盘，下次启动程序时需重新导入，即使调用NLPIR_SaveTheUsrDic。
        /// </summary>
        /// <param name="sFilename">用户自定义词典文件名（文本文件）。</param>
        /// <returns>用户自定义词数。</returns>
        public static int ImportUserDict(string sFilename)
        {
            JudgeInit();
            return NLPIR_ImportUserDict(sFilename);
        }

        /// <summary>
        /// 添加用户自定义词，格式为词+空格+词性，例“在国内 kkk”，不指定词性，默认为n。
        /// 若要下次启动程序时仍然有效，需执行NLPIR_SaveTheUsrDic。
        /// </summary>
        /// <param name="sWord">用户自定义词。</param>
        /// <returns>执行成功返回1；否则返回0。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_AddUserWord")]
        private static extern int NLPIR_AddUserWord(string sWord);
        /// <summary>
        /// 添加用户自定义词，格式为词+空格+词性，例“在国内 kkk”，不指定词性，默认为n。
        /// 若要下次启动程序时仍然有效，需执行SaveTheUsrDic。
        /// </summary>
        /// <param name="sWord">用户自定义词。</param>
        /// <returns>是否执行成功。</returns>
        public static bool AddUserWord(string sWord)
        {
            JudgeInit();
            return NLPIR_AddUserWord(sWord) == 1;
        }

        /// <summary>
        /// 删除用户自定义词，不能指定词性。
        /// 若要下次启动程序时仍然有效，需执行NLPIR_SaveTheUsrDic。
        /// </summary>
        /// <param name="sWord">用户自定义词。</param>
        /// <returns>执行成功返回用户自定义词句柄；否则返回-1。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_DelUsrWord")]
        private static extern int NLPIR_DelUsrWord(string sWord);
        /// <summary>
        /// 删除用户自定义词，不能指定词性。
        /// 若要下次启动程序时仍然有效，需执行SaveTheUsrDic。
        /// </summary>
        /// <param name="sWord">用户自定义词。</param>
        /// <returns>是否执行成功。</returns>
        public static bool DelUsrWord(string sWord)
        {
            JudgeInit();
            return NLPIR_DelUsrWord(sWord) != -1;
        }

        /// <summary>
        /// 保存用户自定义词到磁盘。
        /// </summary>
        /// <returns>执行成功返回1；否则返回0。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_SaveTheUsrDic")]
        private static extern int NLPIR_SaveTheUsrDic();
        /// <summary>
        /// 保存用户自定义词到磁盘。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        public static bool SaveTheUsrDic()
        {
            JudgeInit();
            return NLPIR_SaveTheUsrDic() == 1;
        }
        #endregion

        #region 新词操作
        /// <summary>
        /// 启动新词识别。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Start")]
        private static extern bool NLPIR_NWI_Start();
        /// <summary>
        /// 启动新词识别。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        public static bool NWI_Start()
        {
            JudgeInit();
            _NWIStart = NLPIR_NWI_Start();
            // 此处不能用_NWIComplete = ！_NWIStart。
            if (_NWIStart) _NWIComplete = false;
            return _NWIStart;
        }

        /// <summary>
        /// 新词识别添加内容结束，需要在运行NLPIR_NWI_Start()之后才有效。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Complete")]
        private static extern bool NLPIR_NWI_Complete();
        /// <summary>
        /// 新词识别添加内容结束，需要在运行NWI_Start()之后才有效。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        public static bool NWI_Complete()
        {
            JudgeNWIStart();
            _NWIStart = false;
            _NWIComplete = NLPIR_NWI_Complete();
            return _NWIComplete;
        }

        /// <summary>
        /// 往新词识别系统中添加待识别新词的文本文件，可反复添加，需要在运行NLPIR_NWI_Start()之后才有效。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddFile")]
        private static extern bool NLPIR_NWI_AddFile(string sFilename);
        /// <summary>
        /// 往新词识别系统中添加待识别新词的文本文件，可反复添加，需要在运行NWI_Start()之后才有效。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <returns>是否执行成功。</returns>
        public static bool NWI_AddFile(string sFilename)
        {
            JudgeNWIStart();
            return NLPIR_NWI_AddFile(sFilename);
        }

        /// <summary>
        /// 往新词识别系统中添加待识别新词的文本内容，可反复添加，需要在运行NLPIR_NWI_Start()之后才有效。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddMem")]
        private static extern bool NLPIR_NWI_AddMem(string sParagraph);
        /// <summary>
        /// 往新词识别系统中添加待识别新词的文本内容，可反复添加，需要在运行NWI_Start()之后才有效。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>是否执行成功。</returns>
        public static bool NWI_AddMem(string sParagraph)
        {
            JudgeNWIStart();
            return NLPIR_NWI_AddMem(sParagraph);
        }

        /// <summary>
        /// 获取新词识别的结果，需要在运行NLPIR_NWI_Complete()之后才有效。
        /// </summary>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_GetResult")]
        private static extern IntPtr NLPIR_NWI_GetResult(bool bWeightOut = false);
        /// <summary>
        /// 获取新词识别的结果，需要在运行NWI_Complete()之后才有效。
        /// </summary>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>是否执行成功。</returns>
        public static string NWI_GetResult(bool bWeightOut)
        {
            JudgeNWIComplete();
            IntPtr intPtr = NLPIR_NWI_GetResult(bWeightOut);
            return Marshal.PtrToStringAnsi(intPtr);
        }

        /// <summary>
        /// 将新词识别结果导入到用户词典中，需要在运行NLPIR_NWI_Complete()之后才有效。
        /// 经测试该函数会自动将结果写入磁盘，无需执行SaveTheUsrDic。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Result2UserDict")]
        private static extern bool NLPIR_NWI_Result2UserDict();
        /// <summary>
        /// 将新词识别结果导入到用户词典中，需要在运行NWI_Complete()之后才有效。
        /// 经测试该函数会自动将结果写入磁盘，无需执行SaveTheUsrDic。
        /// </summary>
        /// <returns>是否执行成功。</returns>
        public static bool NWI_Result2UserDict()
        {
            JudgeNWIComplete();
            return NLPIR_NWI_Result2UserDict();
        }
        #endregion

        #region 直接获取关键词或新词。
        /// <summary>
        /// 获取文本关键字。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="nMaxKeyLimit">关键字最大数，实际输出关键字数为nMaxKeyLimit+1。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>关键字列表。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetKeyWords")]
        private static extern IntPtr NLPIR_GetKeyWords(
            string sParagraph, int nMaxKeyLimit = 50, bool bWeightOut = false);
        /// <summary>
        /// 获取文本关键字。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="nMaxKeyLimit">关键字最大数。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>关键字列表。</returns>
        public static string GetKeyWords(string sParagraph, int nMaxKeyLimit, bool bWeightOut)
        {
            JudgeInit();
            IntPtr intPtr = NLPIR_GetKeyWords(sParagraph, nMaxKeyLimit - 1, bWeightOut);
            return Marshal.PtrToStringAnsi(intPtr);
        }

        /// <summary>
        /// 获取文本文件关键字。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <param name="nMaxKeyLimit">关键字最大数，实际输出关键字数为nMaxKeyLimit+1。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>关键字列表。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileKeyWords")]
        private static extern IntPtr NLPIR_GetFileKeyWords(
            string sFilename, int nMaxKeyLimit = 50, bool bWeightOut = false);
        /// <summary>
        /// 获取文本文件关键字。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <param name="nMaxKeyLimit">关键字最大数。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>关键字列表。</returns>
        public static string GetFileKeyWords(string sFilename, int nMaxKeyLimit, bool bWeightOut)
        {
            JudgeInit();
            IntPtr intPtr = NLPIR_GetFileKeyWords(sFilename, nMaxKeyLimit - 1, bWeightOut);
            return Marshal.PtrToStringAnsi(intPtr);
        }

        /// <summary>
        /// 获取文本新词。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="nMaxNewLimit">新词最大数，实际输出新词数为nMaxNewLimit+1。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>新词列表。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetNewWords")]
        private static extern IntPtr NLPIR_GetNewWords(
            string sParagraph, int nMaxNewLimit = 50, bool bWeightOut = false);
        /// <summary>
        /// 获取文本新词。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <param name="nMaxNewLimit">新词最大数。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>新词列表。</returns>
        public static string GetNewWords(string sParagraph, int nMaxNewLimit, bool bWeightOut)
        {
            JudgeInit();
            IntPtr intPtr = NLPIR_GetNewWords(sParagraph, nMaxNewLimit - 1, bWeightOut);
            return Marshal.PtrToStringAnsi(intPtr);
        }

        /// <summary>
        /// 获取文本文件新词。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <param name="nMaxNewLimit">新词最大数，实际输出新词数为nMaxNewLimit+1。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>新词列表。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileNewWords")]
        private static extern IntPtr NLPIR_GetFileNewWords(
            string sFilename, int nMaxNewLimit = 50, bool bWeightOut = false);
        /// <summary>
        /// 获取文本文件新词。
        /// </summary>
        /// <param name="sFilename">文本文件名。</param>
        /// <param name="nMaxNewLimit">新词最大数。</param>
        /// <param name="bWeightOut">是否输出权值。</param>
        /// <returns>新词列表。</returns>
        public static string GetFileNewWords(string sFilename, int nMaxNewLimit, bool bWeightOut)
        {
            JudgeInit();
            IntPtr intPtr = NLPIR_GetFileNewWords(sFilename, nMaxNewLimit - 1, bWeightOut);
            return Marshal.PtrToStringAnsi(intPtr);
        }
        #endregion

        #region 其他
        /// <summary>
        /// 设置标注集。
        /// </summary>
        /// <param name="nPOSmap">标注集。</param>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_SetPOSmap")]
        private static extern void NLPIR_SetPOSmap(int nPOSmap);
        /// <summary>
        /// 设置标注集。
        /// </summary>
        /// <param name="nPOSmap">标注集。</param>
        public static void SetPOSmap(NlpirMap nPOSmap)
        {
            JudgeInit();
            NLPIR_SetPOSmap((int)nPOSmap);
        }

        /// <summary>
        /// 从文本提取指纹信息。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>执行成功返回指纹信息；否则返回0。</returns>
        [DllImport("Iveely.Framework.Segment.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FingerPrint")]
        private static extern ulong NLPIR_FingerPrint(string sParagraph);

        /// <summary>
        /// 从文本提取指纹信息。
        /// </summary>
        /// <param name="sParagraph">文本内容。</param>
        /// <returns>执行成功返回指纹信息；否则返回0。</returns>
        public static ulong FingerPrint(string sParagraph)
        {
            JudgeInit();
            return NLPIR_FingerPrint(sParagraph);
        }
        #endregion

        private static IctclasSegment _ictclasSegment;

        public static IctclasSegment GetInstance()
        {
            if (_ictclasSegment == null)
            {
                _ictclasSegment = new IctclasSegment();
            }
            return _ictclasSegment;
        }

        private IctclasSegment()
        {
            Init(rootDir);
        }

        public  Tuple<string[],string[]> SplitToArray(string input)
        {
            int count = GetParagraphProcessAWordCount(input);
            result_t[] results = ParagraphProcessAW(count);
            byte[] bytes = Encoding.Default.GetBytes(input);
            string[] wordsResult=new string[count];
            string[] semanticResult=new string[count];
            for (int i = 0; i < count; i++)
            {
                wordsResult[i] = Encoding.Default.GetString(bytes, results[i].start, results[i].length);
                semanticResult[i] = results[i].sPos;
            }
            Tuple<string[], string[]> tuple = new Tuple<string[], string[]>(wordsResult, semanticResult);
            return tuple;
        }

        public string SplitToString(string input)
        {
            Tuple<string[], string[]> tuple = SplitToArray(input);
            if (tuple.Item1 != null)
            {
                return string.Join(" ", tuple.Item1);
            }
            return string.Empty;
        }

        public string splitWithSemantic(string input)
        {
            int count = GetParagraphProcessAWordCount(input);
            result_t[] results = ParagraphProcessAW(count);
            byte[] bytes = Encoding.Default.GetBytes(input);
            string resultText = string.Empty;
            for (int i = 0; i < count; i++)
            {
               resultText+= Encoding.Default.GetString(bytes, results[i].start, results[i].length)+"/"+results[i].sPos+" ";
            }
            return resultText;
        }

        public string SplitToGetSemantic(string input)
        {
            Tuple<string[], string[]> tuple = SplitToArray(input);
            if (tuple.Item2 != null)
            {
                return string.Join(" ", tuple.Item2);
            }
            return string.Empty;
        }
    }
}
