/***********************************************************************************
 * ICTCLAS简介：计算所汉语词法分析系统ICTCLAS
 *              Institute of Computing Technology, Chinese Lexical Analysis System
 *              功能有：中文分词；词性标注；未登录词识别。
 *              分词正确率高达97.58%(973专家评测结果)，
 *              未登录词识别召回率均高于90%，其中中国人名的识别召回率接近98%;
 *              处理速度为31.5Kbytes/s。
 * 著作权：  Copyright(c)2002-2005中科院计算所 职务著作权人：张华平
 * 遵循协议：自然语言处理开放资源许可证1.0
 * Email: zhanghp@software.ict.ac.cn
 * Homepage:www.i3s.ac.cn
 * 
 *----------------------------------------------------------------------------------
 * 
 * Copyright (c) 2000, 2001
 *     Institute of Computing Tech.
 *     Chinese Academy of Sciences
 *     All rights reserved.
 *
 * This file is the confidential and proprietary property of
 * Institute of Computing Tech. and the posession or use of this file requires
 * a written license from the author.
 * Author:   Kevin Zhang
 *          (zhanghp@software.ict.ac.cn)、
 * 
 *----------------------------------------------------------------------------------
 * 
 * SharpICTCLAS：.net平台下的ICTCLAS
 *               是由河北理工大学经管学院吕震宇根据Free版ICTCLAS改编而成，
 *               并对原有代码做了部分重写与调整
 * 
 * Email: zhenyulu@163.com
 * Blog: http://www.cnblogs.com/zhenyulu
 * 
 ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpICTCLAS
{
   public class Segment
   {
      private WordDictionary biDict, coreDict;

      public List<AtomNode> atomSegment;
      public RowFirstDynamicArray<ChainContent> segGraph;
      public ColumnFirstDynamicArray<ChainContent> biGraphResult;
      public RowFirstDynamicArray<ChainContent> m_graphOptimum;
      public List<WordResult[]> m_pWordSeg;  //存放多个分词结果

      public event SegmentEventHandler OnSegmentEvent;

      #region 构造函数

      public Segment(WordDictionary biDict, WordDictionary coreDict)
      {
         this.biDict = biDict;
         this.coreDict = coreDict;
      }

      #endregion

      #region BiSegment Method

      public int BiSegment(string sSentence, double smoothPara, int nKind)
      {
         WordResult[] tmpResult;
         WordLinkedArray linkedArray;

         if (biDict == null || coreDict == null)
            throw new Exception("biDict 或 coreDict 尚未初始化！");

         //---原子分词
         atomSegment = AtomSegment(sSentence);
         OnAtomSegment(atomSegment);

         //---检索词库，加入所有可能分词方案并存入链表结构
         segGraph = GenerateWordNet(atomSegment, coreDict);
         OnGenSegGraph(segGraph);

         //---检索所有可能的两两组合
         biGraphResult = BiGraphGenerate(segGraph, smoothPara, biDict, coreDict);
         OnGenBiSegGraph(biGraphResult);

         //---N 最短路径计算出多个分词方案
         NShortPath.Calculate(biGraphResult, nKind);
         List<int[]> spResult = NShortPath.GetNPaths(Predefine.MAX_SEGMENT_NUM);
         OnNShortPath(spResult, segGraph);

         m_pWordSeg = new List<WordResult[]>();
         m_graphOptimum = new RowFirstDynamicArray<ChainContent>();

         for (int i = 0; i < spResult.Count; i++)
         {
            linkedArray = BiPath2LinkedArray(spResult[i], segGraph, atomSegment);
            tmpResult = GenerateWord(spResult[i], linkedArray, m_graphOptimum);

            if (tmpResult != null)
               m_pWordSeg.Add(tmpResult);
         }

         OnBeforeOptimize(m_pWordSeg);

         return m_pWordSeg.Count;
      }

      #endregion

      #region BiOptimumSegment Method

      public int BiOptimumSegment(int nResultCount, double dSmoothingPara)
      {
         WordResult[] tmpResult;
         WordLinkedArray linkedArray;

         //Generate the biword link net
         ColumnFirstDynamicArray<ChainContent> aBiwordsNet = BiGraphGenerate(m_graphOptimum, dSmoothingPara, biDict, coreDict);
         OnGenBiOptimumSegGraph(aBiwordsNet);

         NShortPath.Calculate(aBiwordsNet, nResultCount);
         List<int[]> spResult = NShortPath.GetNPaths(Predefine.MAX_SEGMENT_NUM);

         m_pWordSeg = new List<WordResult[]>();
         segGraph = m_graphOptimum;
         m_graphOptimum = new RowFirstDynamicArray<ChainContent>();

         for (int i = 0; i < spResult.Count; i++)
         {
            linkedArray = BiPath2LinkedArray(spResult[i], segGraph, atomSegment);
            tmpResult = GenerateWord(spResult[i], linkedArray, m_graphOptimum);

            if (tmpResult != null)
               m_pWordSeg.Add(tmpResult);
         }

         return m_pWordSeg.Count;
      }

      #endregion

      #region AtomSegment Method

      //====================================================================
      // 对sSentence进行单个汉字的切割
      //====================================================================
      public static List<AtomNode> AtomSegment(string sSentence)
      {
         List<AtomNode> atomSegment = new List<AtomNode>();
         AtomNode tmpEnd = null;
         int startIndex = 0, length = sSentence.Length, pCur = 0, nCurType, nNextType;
         StringBuilder sb = new StringBuilder();
         char c;

         // 如果是开始符号
         if (sSentence.StartsWith(Predefine.SENTENCE_BEGIN))
         {
            atomSegment.Add(new AtomNode(Predefine.SENTENCE_BEGIN, Predefine.CT_SENTENCE_BEGIN));
            startIndex = Predefine.SENTENCE_BEGIN.Length;
            length -= startIndex;
         }

         // 如果是结束符号
         if (sSentence.EndsWith(Predefine.SENTENCE_END))
         {
            tmpEnd = new AtomNode(Predefine.SENTENCE_END, Predefine.CT_SENTENCE_END);
            length -= Predefine.SENTENCE_END.Length;
         }

         //==============================================================================================
         // by zhenyulu:
         //
         // TODO: 使用一系列正则表达式将句子中的完整成分（百分比、日期、电子邮件、URL等）预先提取出来
         //==============================================================================================

         char[] charArray = sSentence.ToCharArray(startIndex, length);
         int[] charTypeArray = new int[charArray.Length];

         // 生成对应单个汉字的字符类型数组
         for (int i = 0; i < charArray.Length; i++)
         {
            c = charArray[i];
            charTypeArray[i] = Utility.charType(c);

            if (c == '.' && i < (charArray.Length - 1) && Utility.charType(charArray[i + 1]) == Predefine.CT_NUM)
               charTypeArray[i] = Predefine.CT_NUM;
            else if (c == '.' && i < (charArray.Length - 1) && charArray[i + 1] >= '0' && charArray[i + 1] <= '9')
               charTypeArray[i] = Predefine.CT_SINGLE;
            else if (charTypeArray[i] == Predefine.CT_LETTER)
               charTypeArray[i] = Predefine.CT_SINGLE;
         }

         // 根据字符类型数组中的内容完成原子切割
         while (pCur < charArray.Length)
         {
            nCurType = charTypeArray[pCur];

            if (nCurType == Predefine.CT_CHINESE || nCurType == Predefine.CT_INDEX ||
               nCurType == Predefine.CT_DELIMITER || nCurType == Predefine.CT_OTHER)
            {
               if (charArray[pCur].ToString().Trim().Length != 0)
                  atomSegment.Add(new AtomNode(charArray[pCur].ToString(), nCurType));
               pCur++;
            }
            //如果是字符、数字或者后面跟随了数字的小数点“.”则一直取下去。
            else if (pCur < charArray.Length - 1 && (nCurType == Predefine.CT_SINGLE || nCurType == Predefine.CT_NUM))
            {
               sb.Remove(0, sb.Length);
               sb.Append(charArray[pCur]);

               bool reachEnd = true;
               while (pCur < charArray.Length - 1)
               {
                  nNextType = charTypeArray[++pCur];

                  if (nNextType == nCurType)
                     sb.Append(charArray[pCur]);
                  else
                  {
                     reachEnd = false;
                     break;
                  }
               }
               atomSegment.Add(new AtomNode(sb.ToString(), nCurType));
               if (reachEnd)
                  pCur++;
            }
            // 对于所有其它情况
            else
            {
               atomSegment.Add(new AtomNode(charArray[pCur].ToString(), nCurType));
               pCur++;
            }
         }

         // 增加结束标志
         if (tmpEnd != null)
            atomSegment.Add(tmpEnd);

         return atomSegment;
      }

      #endregion

      #region GenerateWordNet Method

      //====================================================================
      // Func Name  : GenerateWordNet
      // Description: Generate the segmentation word net according 
      //              the original sentence
      // Parameters : sSentence: the sentence
      //              dictCore : core dictionary
      //              bOriginalFreq=false: output original frequency
      // Returns    : bool
      //====================================================================
      public static RowFirstDynamicArray<ChainContent> GenerateWordNet(List<AtomNode> atomSegment, WordDictionary coreDict)
      {
         string sWord = "", sMaxMatchWord;
         int nPOSRet, nPOS, nTotalFreq;
         double dValue = 0;

         RowFirstDynamicArray<ChainContent> m_segGraph = new RowFirstDynamicArray<ChainContent>();
         m_segGraph.SetEmpty();

         // 将原子部分存入m_segGraph
         for (int i = 0; i < atomSegment.Count; i++)//Init the cost array
         {
            if (atomSegment[i].nPOS == Predefine.CT_CHINESE)
               m_segGraph.SetElement(i, i + 1, new ChainContent(0, 0, atomSegment[i].sWord));
            else
            {
               sWord = atomSegment[i].sWord;//init the word 
               dValue = Predefine.MAX_FREQUENCE;
               switch (atomSegment[i].nPOS)
               {
                  case Predefine.CT_INDEX:
                  case Predefine.CT_NUM:
                     nPOS = -27904;//'m'*256
                     sWord = "未##数";
                     dValue = 0;
                     break;
                  case Predefine.CT_DELIMITER:
                     nPOS = 30464;//'w'*256;
                     break;
                  case Predefine.CT_LETTER:
                     nPOS = -28280; // -'n' * 256 - 'x';
                     dValue = 0;
                     sWord = "未##串";
                     break;
                  case Predefine.CT_SINGLE://12021-2129-3121
                     if (Regex.IsMatch(atomSegment[i].sWord, @"^(-?\d+)(\.\d+)?$"))　　//匹配浮点数
                     {
                        nPOS = -27904;//'m'*256
                        sWord = "未##数";
                     }
                     else
                     {
                        nPOS = -28280; // -'n' * 256 - 'x'
                        sWord = "未##串";
                     }
                     dValue = 0;
                     break;
                  default:
                     nPOS = atomSegment[i].nPOS;//'?'*256;
                     break;
               }
               m_segGraph.SetElement(i, i + 1, new ChainContent(dValue, nPOS, sWord));//init the link with minimum
            }
         }

         // 将所有可能的组词存入m_segGraph
         for (int i = 0; i < atomSegment.Count; i++)//All the word
         {
            sWord = atomSegment[i].sWord;//Get the current atom
            int j = i + 1;

            while (j < atomSegment.Count && coreDict.GetMaxMatch(sWord, out sMaxMatchWord, out nPOSRet))
            {
               if (sMaxMatchWord == sWord)  // 就是我们要找的词
               {
                  WordInfo info = coreDict.GetWordInfo(sWord); // 该词可能就有多种词性

                  // 计算该词的所有词频之和
                  nTotalFreq = 0;
                  for (int k = 0; k < info.Count; k++)
                     nTotalFreq += info.Frequencies[k];

                  // 限制出现某些特殊词
                  if (sWord.Length == 2 && (sWord.StartsWith("年") || sWord.StartsWith("月")) && i >= 1 &&
                     (Utility.IsAllNum(atomSegment[i - 1].sWord) ||
                     Utility.IsAllChineseNum(atomSegment[i - 1].sWord)))
                  {
                     //1年内、1999年末
                     if ("末内中底前间初".IndexOf(sWord.Substring(1)) >= 0)
                        break;
                  }

                  // 如果该词只有一个词性，则存储，否则词性记录为 0
                  if (info.Count == 1)
                     m_segGraph.SetElement(i, j, new ChainContent(nTotalFreq, info.POSs[0], sWord));
                  else
                     m_segGraph.SetElement(i, j, new ChainContent(nTotalFreq, 0, sWord));
               }

               sWord += atomSegment[j++].sWord;
            }
         }
         return m_segGraph;
      }

      #endregion

      #region BiGraphGenerate Method

      //====================================================================
      // 生成两两词之间的二叉图表
      //====================================================================
      public static ColumnFirstDynamicArray<ChainContent> BiGraphGenerate(
         RowFirstDynamicArray<ChainContent> aWord, double smoothPara, WordDictionary biDict, WordDictionary coreDict)
      {
         ColumnFirstDynamicArray<ChainContent> aBiWordNet = new ColumnFirstDynamicArray<ChainContent>();

         ChainItem<ChainContent> pCur, pNextWords;
         int nTwoWordsFreq = 0, nCurWordIndex, nNextWordIndex;
         double dCurFreqency, dValue, dTemp;
         string sTwoWords;
         StringBuilder sb = new StringBuilder();

         //Record the position map of possible words
         int[] m_npWordPosMapTable = PreparePositionMap(aWord);

         pCur = aWord.GetHead();
         while (pCur != null)
         {
            if (pCur.Content.nPOS >= 0)
               //It's not an unknown words
               dCurFreqency = pCur.Content.eWeight;
            else
               //Unknown words
               dCurFreqency = coreDict.GetFrequency(pCur.Content.sWord, 2);

            //Get next words which begin with pCur.col（注：很特殊的对应关系）
            pNextWords = aWord.GetFirstElementOfRow(pCur.col);

            while (pNextWords != null && pNextWords.row == pCur.col)
            {
               sb.Remove(0, sb.Length);
               sb.Append(pCur.Content.sWord);
               sb.Append(Predefine.WORD_SEGMENTER);
               sb.Append(pNextWords.Content.sWord);

               sTwoWords = sb.ToString();

               //Two linked Words frequency
               nTwoWordsFreq = biDict.GetFrequency(sTwoWords, 3);

               //Smoothing
               dTemp = 1.0 / Predefine.MAX_FREQUENCE;

               //-log{a*P(Ci-1)+(1-a)P(Ci|Ci-1)} Note 0<a<1
               dValue = -Math.Log(smoothPara * (1.0 + dCurFreqency) / (Predefine.MAX_FREQUENCE + 80000.0)
                 + (1.0 - smoothPara) * ((1.0 - dTemp) * nTwoWordsFreq / (1.0 + dCurFreqency) +
                 dTemp));

               //Unknown words: P(Wi|Ci);while known words:1
               if (pCur.Content.nPOS < 0)
                  dValue += pCur.Content.nPOS;

               //Get the position index of current word in the position map table
               nCurWordIndex = Utility.BinarySearch(pCur.row * Predefine.MAX_SENTENCE_LEN + pCur.col, m_npWordPosMapTable);
               nNextWordIndex = Utility.BinarySearch(pNextWords.row * Predefine.MAX_SENTENCE_LEN + pNextWords.col, m_npWordPosMapTable);

               aBiWordNet.SetElement(nCurWordIndex, nNextWordIndex, new ChainContent(dValue, pCur.Content.nPOS, sTwoWords));

               pNextWords = pNextWords.next; //Get next word
            }
            pCur = pCur.next;
         }

         return aBiWordNet;
      }

      //====================================================================
      // 准备PositionMap，用于记录词的位置
      //====================================================================
      private static int[] PreparePositionMap(RowFirstDynamicArray<ChainContent> aWord)
      {
         int[] m_npWordPosMapTable;
         ChainItem<ChainContent> pTail, pCur;
         int nWordIndex = 0, m_nWordCount;

         //Get tail element and return the words count
         m_nWordCount = aWord.GetTail(out pTail);

         if (m_nWordCount > 0)
            m_npWordPosMapTable = new int[m_nWordCount];
         else
            m_npWordPosMapTable = null;

         //Record the  position of possible words
         pCur = aWord.GetHead();
         while (pCur != null)
         {
            m_npWordPosMapTable[nWordIndex++] = pCur.row * Predefine.MAX_SENTENCE_LEN + pCur.col;
            pCur = pCur.next;
         }

         return m_npWordPosMapTable;
      }

      #endregion

      #region Private Static Functions

      #region BiPath2LinkedArray Method

      //====================================================================
      // 将BiPath转换为LinkedArray
      // 例如“他说的确实在理”
      // BiPath：（0, 1, 2, 3, 6, 9, 11, 12）
      //    0    1   2   3   4     5   6     7   8     9   10    11  12
      // 始##始  他  说  的  的确  确  确实  实  实在  在  在理  理  末##末
      //====================================================================
      private static WordLinkedArray BiPath2LinkedArray(int[] biPath, RowFirstDynamicArray<ChainContent> segGraph, List<AtomNode> atomSegment)
      {
         List<ChainItem<ChainContent>> list = segGraph.ToListItems();
         StringBuilder sb = new StringBuilder();

         WordLinkedArray result = new WordLinkedArray();

         for (int i = 0; i < biPath.Length; i++)
         {
            WordNode node = new WordNode();

            node.row = list[biPath[i]].row;
            node.col = list[biPath[i]].col;
            node.sWordInSegGraph = list[biPath[i]].Content.sWord;

            node.theWord = new WordResult();
            if (node.sWordInSegGraph == "未##人" || node.sWordInSegGraph == "未##地" ||
               node.sWordInSegGraph == "未##数" || node.sWordInSegGraph == "未##时" || node.sWordInSegGraph == "未##串")
            {
               sb.Remove(0, sb.Length);
               for (int j = node.row; j < node.col; j++)
                  sb.Append(atomSegment[j].sWord);

               node.theWord.sWord = sb.ToString();
            }
            else
               node.theWord.sWord = list[biPath[i]].Content.sWord;

            node.theWord.nPOS = list[biPath[i]].Content.nPOS;
            node.theWord.dValue = list[biPath[i]].Content.eWeight;

            result.AppendNode(node);
         }

         return result;
      }

      #endregion

      #region GenerateWord Method

      //====================================================================
      // Generate Word according the segmentation route
      //====================================================================
      private static WordResult[] GenerateWord(int[] uniPath, WordLinkedArray linkedArray, RowFirstDynamicArray<ChainContent> m_graphOptimum)
      {
         if (linkedArray.Count == 0)
            return null;

         //--------------------------------------------------------------------
         //Merge all seperate continue num into one number
         MergeContinueNumIntoOne(ref linkedArray);

         //--------------------------------------------------------------------
         //The delimiter "－－"
         ChangeDelimiterPOS(ref linkedArray);

         //--------------------------------------------------------------------
         //如果前一个词是数字，当前词以“－”或“-”开始，并且不止这一个字符，
         //那么将此“－”符号从当前词中分离出来。
         //例如 “3 / -4 / 月”需要拆分成“3 / - / 4 / 月”
         SplitMiddleSlashFromDigitalWords(ref linkedArray);

         //--------------------------------------------------------------------
         //1、如果当前词是数字，下一个词是“月、日、时、分、秒、月份”中的一个，则合并,且当前词词性是时间
         //2、如果当前词是可以作为年份的数字，下一个词是“年”，则合并，词性为时间，否则为数字。
         //3、如果最后一个汉字是"点" ，则认为当前数字是时间
         //4、如果当前串最后一个汉字不是"∶·．／"和半角的'.''/'，那么是数
         //5、当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
         CheckDateElements(ref linkedArray);

         //--------------------------------------------------------------------
         //输出结果
         WordResult[] result = new WordResult[linkedArray.Count];

         WordNode pCur = linkedArray.first;
         int i = 0;
         while (pCur != null)
         {
            WordResult item = new WordResult();
            item.sWord = pCur.theWord.sWord;
            item.nPOS = pCur.theWord.nPOS;
            item.dValue = pCur.theWord.dValue;
            result[i] = item;

            m_graphOptimum.SetElement(pCur.row, pCur.col, new ChainContent(item.dValue, item.nPOS, pCur.sWordInSegGraph));

            pCur = pCur.next;
            i++;
         }

         return result;
      }

      #endregion

      #region MergeContinueNumIntoOne Method

      private static void MergeContinueNumIntoOne(ref WordLinkedArray linkedArray)
      {
         if (linkedArray.Count < 2)
            return;

         string tmp;
         WordNode pCur = linkedArray.first;
         WordNode pNext = pCur.next;

         while (pNext != null)
         {
            if ((Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord)) &&
               (Utility.IsAllNum(pNext.theWord.sWord) || Utility.IsAllChineseNum(pNext.theWord.sWord)))
            {
               tmp = pCur.theWord.sWord + pNext.theWord.sWord;
               if (Utility.IsAllNum(tmp) || Utility.IsAllChineseNum(tmp))
               {
                  pCur.theWord.sWord += pNext.theWord.sWord;
                  pCur.col = pNext.col;
                  pCur.next = pNext.next;
                  linkedArray.Count--;
                  pNext = pCur.next;
                  continue;
               }
            }

            pCur = pCur.next;
            pNext = pNext.next;
         }
      }

      #endregion

      #region ChangeDelimiterPOS Method

      private static void ChangeDelimiterPOS(ref WordLinkedArray linkedArray)
      {
         WordNode pCur = linkedArray.first;
         while (pCur != null)
         {
            if (pCur.theWord.sWord == "－－" || pCur.theWord.sWord == "—" || pCur.theWord.sWord == "-")
            {
               pCur.theWord.nPOS = 30464; //'w'*256;Set the POS with 'w'
               pCur.theWord.dValue = 0;
            }

            pCur = pCur.next;
         }
      }

      #endregion

      #region SplitMiddleSlashFromDigitalWords Method

      //====================================================================
      //如果前一个词是数字，当前词以“－”或“-”开始，并且不止这一个字符，
      //那么将此“－”符号从当前词中分离出来。
      //例如 “3 / -4 / 月”需要拆分成“3 / - / 4 / 月”
      //====================================================================
      private static void SplitMiddleSlashFromDigitalWords(ref WordLinkedArray linkedArray)
      {
         if (linkedArray.Count < 2)
            return;

         WordNode pCur = linkedArray.first.next;
         WordNode pPre = linkedArray.first;

         while (pCur != null)
         {
            //27904='m'*256
            if ((Math.Abs(pPre.theWord.nPOS) == 27904 || Math.Abs(pPre.theWord.nPOS) == 29696) &&
               (Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord)) &&
               ("-－".IndexOf(pCur.theWord.sWord.ToCharArray()[0]) >= 0) && pCur.theWord.sWord.Length > 1)
            {
               // 将“－”拆分出来。
               WordNode newNode = new WordNode();
               newNode.row = pCur.row + 1;
               newNode.col = pCur.col;
               newNode.sWordInSegGraph = pCur.theWord.sWord.Substring(1);
               WordResult theWord = new WordResult();
               theWord.sWord = newNode.sWordInSegGraph;
               theWord.nPOS = 27904;
               theWord.dValue = pCur.theWord.dValue;
               newNode.theWord = theWord;

               pCur.col = pCur.row + 1;
               pCur.theWord.sWord = pCur.theWord.sWord.Substring(0, 1);
               pCur.theWord.nPOS = 30464; //'w'*256;
               pCur.theWord.dValue = 0;

               newNode.next = pCur.next;
               pCur.next = newNode;

               linkedArray.Count++;
            }
            pCur = pCur.next;
            pPre = pPre.next;
         }
      }

      #endregion

      #region CheckDateElements Method

      //====================================================================
      //1、如果当前词是数字，下一个词是“月、日、时、分、秒、月份”中的一个，则合并且当前词词性是时间
      //2、如果当前词是可以作为年份的数字，下一个词是“年”，则合并，词性为时间，否则为数字。
      //3、如果最后一个汉字是"点" ，则认为当前数字是时间
      //4、如果当前串最后一个汉字不是"∶·．／"和半角的'.''/'，那么是数
      //5、当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
      //====================================================================
      private static void CheckDateElements(ref WordLinkedArray linkedArray)
      {
         if (linkedArray.Count < 2)
            return;

         string nextWord;
         WordNode pCur = linkedArray.first;
         WordNode pNext = pCur.next;

         while (pNext != null)
         {
            if (Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord))
            {
               //===== 1、如果当前词是数字，下一个词是“月、日、时、分、秒、月份”中的一个，则合并且当前词词性是时间
               nextWord = pNext.theWord.sWord;
               if ((nextWord.Length == 1 && "月日时分秒".IndexOf(nextWord) != -1) || (nextWord.Length == 2 && nextWord == "月份"))
               {
                  //2001年
                  pCur.theWord.sWord += nextWord;
                  pCur.col = pNext.col;
                  pCur.sWordInSegGraph = "未##时";
                  pCur.theWord.nPOS = -29696; //'t'*256;//Set the POS with 'm'
                  pCur.next = pNext.next;
                  pNext = pCur.next;
                  linkedArray.Count--;
               }
               //===== 2、如果当前词是可以作为年份的数字，下一个词是“年”，则合并，词性为时间，否则为数字。
               else if (nextWord == "年")
               {
                  if (IsYearTime(pCur.theWord.sWord))
                  {
                     pCur.theWord.sWord += nextWord;
                     pCur.col = pNext.col;
                     pCur.sWordInSegGraph = "未##时";
                     pCur.theWord.nPOS = -29696; //'t'*256;//Set the POS with 'm'
                     pCur.next = pNext.next;
                     pNext = pCur.next;
                     linkedArray.Count--;
                  }
                  //===== 否则当前词就是数字了 =====
                  else
                  {
                     pCur.sWordInSegGraph = "未##数";
                     pCur.theWord.nPOS = -27904; //Set the POS with 'm'
                  }
               }
               else
               {
                  //===== 3、如果最后一个汉字是"点" ，则认为当前数字是时间
                  if (pCur.theWord.sWord.EndsWith("点"))
                  {
                     pCur.sWordInSegGraph = "未##时";
                     pCur.theWord.nPOS = -29696; //Set the POS with 't'
                  }
                  else
                  {
                     char[] tmpcharArray = pCur.theWord.sWord.ToCharArray();
                     string lastChar = tmpcharArray[tmpcharArray.Length - 1].ToString();
                     //===== 4、如果当前串最后一个汉字不是"∶·．／"和半角的'.''/'，那么是数
                     if ("∶·．／./".IndexOf(lastChar) == -1)
                     {
                        pCur.sWordInSegGraph = "未##数";
                        pCur.theWord.nPOS = -27904; //'m'*256;Set the POS with 'm'
                     }
                     //===== 5、当前串最后一个汉字是"∶·．／"和半角的'.''/'，且长度大于1，那么去掉最后一个字符。例如"1."
                     else if (pCur.theWord.sWord.Length > 1)
                     {
                        pCur.theWord.sWord = pCur.theWord.sWord.Substring(0, pCur.theWord.sWord.Length - 1);

                        pCur.sWordInSegGraph = "未##数";
                        pCur.theWord.nPOS = -27904; //'m'*256;Set the POS with 'm'
                     }
                  }
               }
            }

            pCur = pCur.next;
            pNext = pNext.next;
         }
      }

      #endregion

      #region IsYearTime Method

      private static bool IsYearTime(string sNum)
      {
         //Judge whether the sNum is a num genearating year
         int nLen = sNum.Length;
         char[] charArray = sNum.ToCharArray();

         //1992年, 90年
         if (Utility.IsAllNum(sNum) && (nLen == 4 || nLen == 2 && "５６７８９56789".IndexOf(charArray[0]) != -1))
            return true;

         if (Utility.GetCharCount("零○一二三四五六七八九壹贰叁肆伍陆柒捌玖", sNum) == nLen && nLen >= 2)
            return true;

         //二仟零二年
         if (nLen == 4 && Utility.GetCharCount("千仟零○", sNum) == 2)
            return true;

         if (nLen == 1 && Utility.GetCharCount("千仟", sNum) == 1)
            return true;

         if (nLen == 2 && Regex.IsMatch(sNum, "^[甲乙丙丁戊己庚辛壬癸][子丑寅卯辰巳午未申酉戌亥]$"))
            return true;

         return false;
      }

      #endregion

      #endregion

      #region Events

      private void SendEvents(SegmentEventArgs e)
      {
         if (OnSegmentEvent != null)
            OnSegmentEvent(this, e);
      }

      private void OnAtomSegment(List<AtomNode> nodes)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < nodes.Count; i++)
            sb.Append(string.Format("{0}, ", nodes[i].sWord));

         sb.Append("\r\n");

         SendEvents(new SegmentEventArgs(SegmentStage.AtomSegment, sb.ToString()));
      }

      private void OnGenSegGraph(RowFirstDynamicArray<ChainContent> segGraph)
      {
         SendEvents(new SegmentEventArgs(SegmentStage.GenSegGraph, segGraph.ToString()));
      }

      private void OnGenBiSegGraph(ColumnFirstDynamicArray<ChainContent> biGraph)
      {
         SendEvents(new SegmentEventArgs(SegmentStage.GenBiSegGraph, biGraph.ToString()));
      }

      private void OnNShortPath(List<int[]> paths, RowFirstDynamicArray<ChainContent> segGraph)
      {
         List<ChainItem<ChainContent>> list = segGraph.ToListItems();
         string theWord;

         int[] aPath;
         StringBuilder sb = new StringBuilder();

         for (int i = 0; i < paths.Count; i++)
         {
            aPath = paths[i];
            for (int j = 0; j < aPath.Length; j++)
            {
               theWord = list[aPath[j]].Content.sWord;
               if (theWord == "未##人" || theWord == "未##地" || theWord == "未##数" || theWord == "未##时" || theWord == "未##串")
               {
                  for (int k = list[aPath[j]].row; k < list[aPath[j]].col; k++)
                     sb.Append(atomSegment[k].sWord);
                  sb.Append(", ");
               }
               else
                  sb.Append(string.Format("{0}, ", list[aPath[j]].Content.sWord));
            }

            sb.Append("\r\n");
         }

         SendEvents(new SegmentEventArgs(SegmentStage.NShortPath, sb.ToString()));
      }

      private void OnBeforeOptimize(List<WordResult[]> m_pWordSeg)
      {
         StringBuilder sb = new StringBuilder();
         for (int k = 0; k < m_pWordSeg.Count; k++)
         {
            for (int j = 0; j < m_pWordSeg[k].Length; j++)
               sb.Append(string.Format("{0}, ", m_pWordSeg[k][j].sWord));
            sb.Append("\r\n");
         }

         SendEvents(new SegmentEventArgs(SegmentStage.BeforeOptimize, sb.ToString()));
      }

      private void OnOptimumSegment(RowFirstDynamicArray<ChainContent> m_graphOptimum)
      {
         SendEvents(new SegmentEventArgs(SegmentStage.OptimumSegment, m_graphOptimum.ToString()));
      }

      private void OnGenBiOptimumSegGraph(ColumnFirstDynamicArray<ChainContent> biOptGraph)
      {
         SendEvents(new SegmentEventArgs(SegmentStage.GenBiSegGraph, biOptGraph.ToString()));
      }

      #endregion
   }
}
