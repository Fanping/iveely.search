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
using System.IO;

namespace SharpICTCLAS
{
   public class ContextStat
   {
      private int m_nTableLen;
      private int[] m_pSymbolTable;
      private ContextItem m_pContext = null;

      #region 构造函数

      public ContextStat()
      {
         m_pSymbolTable = null; //new buffer for symbol
      }

      #endregion

      #region SetSymbol Method

      public void SetSymbol(int[] nSymbol)
      {
         m_pSymbolTable = nSymbol;
      }

      #endregion

      #region Load Method

      public bool Load(string sFilename)
      {
         bool isSuccess = true;
         ContextItem pCur = null, pPre = null;

         FileStream fileStream = null;
         BinaryReader binReader = null;

         try
         {
            fileStream = new FileStream(sFilename, FileMode.Open, FileAccess.Read);
            if (fileStream == null)
               return false;

            binReader = new BinaryReader(fileStream, Encoding.GetEncoding("gb2312"));


            m_nTableLen = binReader.ReadInt32();   //write the table length
            m_pSymbolTable = new int[m_nTableLen]; //new buffer for symbol

            for (int i = 0; i < m_nTableLen; i++)  //write the symbol table
               m_pSymbolTable[i] = binReader.ReadInt32();

            while (binReader.PeekChar() != -1)
            {
               //Read the context 
               pCur = new ContextItem();
               pCur.next = null;
               pCur.nKey = binReader.ReadInt32();
               pCur.nTotalFreq = binReader.ReadInt32();

               pCur.aTagFreq = new int[m_nTableLen];
               for (int i = 0; i < m_nTableLen; i++)     //the every POS frequency
                  pCur.aTagFreq[i] = binReader.ReadInt32();


               pCur.aContextArray = new int[m_nTableLen][];
               for (int i = 0; i < m_nTableLen; i++)
               {
                  pCur.aContextArray[i] = new int[m_nTableLen];
                  for (int j = 0; j < m_nTableLen; j++)
                     pCur.aContextArray[i][j] = binReader.ReadInt32();
               }

               if (pPre == null)
                  m_pContext = pCur;
               else
                  pPre.next = pCur;

               pPre = pCur;
            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
            isSuccess = false;
         }
         finally
         {
            if (binReader != null)
               binReader.Close();

            if (fileStream != null)
               fileStream.Close();
         }

         return isSuccess;
      }

      #endregion

      #region GetItem Method

      //=========================================================
      // 返回nKey为指定nKey的结点，如果没找到，则返回前一个结点
      //=========================================================
      public bool GetItem(int nKey, out ContextItem pItemRet)
      {
         ContextItem pCur = m_pContext, pPrev = null;
         if (nKey == 0 && m_pContext != null)
         {
            pItemRet = m_pContext;
            return true;
         }

         while (pCur != null && pCur.nKey < nKey)
         {
            //delete the context array
            pPrev = pCur;
            pCur = pCur.next;
         }

         if (pCur != null && pCur.nKey == nKey)
         {
            //find it and return the current item
            pItemRet = pCur;
            return true;
         }

         pItemRet = pPrev;
         return false;
      }

      #endregion

      #region GetContextPossibility Method

      public double GetContextPossibility(int nKey, int nPrev, int nCur)
      {
         ContextItem pCur;
         int nCurIndex = Utility.BinarySearch(nCur, m_pSymbolTable);
         int nPrevIndex = Utility.BinarySearch(nPrev, m_pSymbolTable);

         //return a lower value, not 0 to prevent data sparse
         if (!GetItem(nKey, out pCur) || nCurIndex == -1 || nPrevIndex == -1 ||
             pCur.aTagFreq[nPrevIndex] == 0 || pCur.aContextArray[nPrevIndex][nCurIndex] == 0)
            return 0.000001;

         int nPrevCurConFreq = pCur.aContextArray[nPrevIndex][nCurIndex];
         int nPrevFreq = pCur.aTagFreq[nPrevIndex];

         //0.9 and 0.1 is a value based experience
         return 0.9 * (double)nPrevCurConFreq / (double)nPrevFreq + 0.1 * (double)
            nPrevFreq / (double)pCur.nTotalFreq;
      }

      #endregion

      #region GetFrequency Method

      //=========================================================
      //Get the frequency which nSymbol appears
      //=========================================================
      public int GetFrequency(int nKey, int nSymbol)
      {
         ContextItem pFound;

         int nIndex, nFrequency = 0;
         if (!GetItem(nKey, out pFound))
            //Not found such a item
            return 0;

         nIndex = Utility.BinarySearch(nSymbol, m_pSymbolTable);
         if (nIndex == -1)
            //error finding the symbol
            return 0;

         nFrequency = pFound.aTagFreq[nIndex]; //Add the frequency
         return nFrequency;
      }

      #endregion

      #region SetTableLen Method

      public void SetTableLen(int nTableLen)
      {
         m_nTableLen = nTableLen;
         m_pSymbolTable = new int[nTableLen];
         m_pContext = null;
      }

      #endregion

      #region ReleaseContextStat Method

      public void ReleaseContextStat()
      {
         m_pContext = null;
         m_pSymbolTable = null;
      }

      #endregion

      #region Add Method

      public bool Add(int nKey, int nPrevSymbol, int nCurSymbol, int nFrequency)
      {
         //Add the context symbol to the array
         ContextItem pRetItem, pNew;
         int nPrevIndex, nCurIndex;

         //Not get it
         if (!GetItem(nKey, out pRetItem))
         {
            pNew = new ContextItem();
            pNew.nKey = nKey;
            pNew.nTotalFreq = 0;
            pNew.next = null;
            pNew.aContextArray = new int[m_nTableLen][];
            pNew.aTagFreq = new int[m_nTableLen];
            for (int i = 0; i < m_nTableLen; i++)
               pNew.aContextArray[i] = new int[m_nTableLen];

            if (pRetItem == null)
               //Empty, the new item is head
               m_pContext = pNew;
            else
            //Link the new item between pRetItem and its next item
            {
               pNew.next = pRetItem.next;
               pRetItem.next = pNew;
            }
            pRetItem = pNew;
         }

         nPrevIndex = Utility.BinarySearch(nPrevSymbol, m_pSymbolTable);
         if (nPrevSymbol > 256 && nPrevIndex == -1)
            //Not find, just for 'nx' and other uncommon POS
            nPrevIndex = Utility.BinarySearch(nPrevSymbol - nPrevSymbol % 256, m_pSymbolTable);

         nCurIndex = Utility.BinarySearch(nCurSymbol, m_pSymbolTable);

         if (nCurSymbol > 256 && nCurIndex == -1)
            //Not find, just for 'nx' and other uncommon POS
            nCurIndex = Utility.BinarySearch(nCurSymbol - nCurSymbol % 256, m_pSymbolTable);

         if (nPrevIndex == -1 || nCurIndex == -1)
            //error finding the symbol
            return false;

         //Add the frequency
         pRetItem.aContextArray[nPrevIndex][nCurIndex] += nFrequency;
         pRetItem.aTagFreq[nPrevIndex] += nFrequency;
         pRetItem.nTotalFreq += nFrequency;

         return true;
      }

      #endregion

      #region Save Method

      public bool Save(string sFilename)
      {
         bool isSuccess = true;
         FileStream outputFile = null;
         FileStream logFile = null;
         BinaryWriter writer = null;
         StreamWriter sw = null;
         StringBuilder sb = new StringBuilder();

         try
         {
            outputFile = new FileStream(sFilename, FileMode.Create, FileAccess.Write);
            if (outputFile == null)
               return false;

            logFile = new FileStream(sFilename + ".shw", FileMode.Create, FileAccess.Write);
            if (logFile == null)
            {
               outputFile.Close();
               return false;
            }

            writer = new BinaryWriter(outputFile, Encoding.GetEncoding("gb2312"));
            sw = new StreamWriter(logFile);

            writer.Write(m_nTableLen);  //write the table length
            sb.Append(string.Format("Table Len={0}\r\nSymbol:\r\n", m_nTableLen));
            for (int i = 0; i < m_nTableLen; i++)  //write the symbol table
            {
               writer.Write(m_pSymbolTable[i]);
               sb.Append(string.Format("{0} ", m_pSymbolTable[i]));
            }
            sb.Append("\r\n");

            ContextItem pCur = m_pContext;
            while (pCur != null)
            {
               writer.Write(pCur.nKey);
               writer.Write(pCur.nTotalFreq);
               sb.Append(string.Format("nKey={0},Total frequency={1}:\r\n", pCur.nKey, pCur.nTotalFreq));

               for (int i = 0; i < m_nTableLen; i++)
                  writer.Write(pCur.aTagFreq[i]);

               //the every POS frequency
               for (int i = 0; i < m_nTableLen; i++)
               {
                  for (int j = 0; j < m_nTableLen; j++)
                     writer.Write(pCur.aContextArray[i][j]);

                  sb.Append(string.Format("No.{0,2}={1,3}: ", i, m_pSymbolTable[i]));
                  for (int j = 0; j < m_nTableLen; j++)
                     sb.Append(string.Format("{0,5} ", pCur.aContextArray[i][j]));

                  sb.Append(string.Format("total={0}:\r\n", pCur.aTagFreq[i]));
               }
               pCur = pCur.next;
            }

            sw.Write(sb.ToString());
         }
         catch
         {
            isSuccess = false;
         }
         finally
         {
            if (writer != null)
               writer.Close();

            if (outputFile != null)
               outputFile.Close();

            if (sw != null)
               sw.Close();

            if (logFile != null)
               logFile.Close();
         }

         return isSuccess;
      }

      #endregion

   }
}
