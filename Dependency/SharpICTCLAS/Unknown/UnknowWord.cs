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

namespace SharpICTCLAS
{
   public class UnknowWord
   {
      public WordDictionary m_dict = new WordDictionary(); //Unknown dictionary

      private Span m_roleTag = new Span(); //Role tagging
      private int m_nPOS; //The POS of such a category
      private string m_sUnknownFlags = null;

      #region Recognition Method

      //Unknown word recognition
      //pWordSegResult:word Segmentation result;
      //graphOptimum: The optimized segmentation graph
      //graphSeg: The original segmentation graph
      public bool Recognition(WordResult[] pWordSegResult, RowFirstDynamicArray<ChainContent> graphOptimum,
         List<AtomNode> atomSegment, WordDictionary dictCore)
      {
         ChainItem<ChainContent> item;
         int nStartPos = 0, j = 0, nAtomStart, nAtomEnd, nPOSOriginal;
         double dValue;
         m_roleTag.POSTagging(pWordSegResult, dictCore, m_dict);
         //Tag the segmentation with unknown recognition roles according the core dictionary and unknown recognition dictionary
         for (int i = 0; i < m_roleTag.m_nUnknownWordsCount; i++)
         {
            while (j < atomSegment.Count && nStartPos < m_roleTag.m_nUnknownWords[i, 0])
               nStartPos += atomSegment[j++].sWord.Length;

            nAtomStart = j;
            while (j < atomSegment.Count && nStartPos < m_roleTag.m_nUnknownWords[i, 1])
               nStartPos += atomSegment[j++].sWord.Length;

            nAtomEnd = j;
            if (nAtomStart < nAtomEnd)
            {
               item = graphOptimum.GetElement(nAtomStart, nAtomEnd);
               if (item != null)
               {
                  dValue = item.Content.eWeight;
                  nPOSOriginal = item.Content.nPOS;
               }
               else
                  dValue = Predefine.INFINITE_VALUE;

               if (dValue > m_roleTag.m_dWordsPossibility[i])
                  //Set the element with less frequency
                  graphOptimum.SetElement(nAtomStart, nAtomEnd, new ChainContent(m_roleTag.m_dWordsPossibility[i], m_nPOS, m_sUnknownFlags));
            }
         }
         return true;
      }

      #endregion

      #region Configure Method

      //Load unknown recognition dictionary
      //Load context
      //type: Unknown words type (including person,place,transliterion and so on)
      public bool Configure(string sConfigFile, TAG_TYPE type)
      {
         //Load the unknown recognition dictionary
         m_dict.Load(sConfigFile + ".dct");

         //Load the unknown recognition context
         m_roleTag.LoadContext(sConfigFile + ".ctx");

         //Set the tagging type
         m_roleTag.SetTagType(type);
         switch (type)
         {
            case TAG_TYPE.TT_PERSON:
            case TAG_TYPE.TT_TRANS_PERSON:
               //Set the special flag for transliterations
               m_nPOS = -28274; //-'n'*256-'r';
               m_sUnknownFlags = "未##人";
               break;
            case TAG_TYPE.TT_PLACE:
               m_nPOS = -28275; //-'n'*256-'s';
               m_sUnknownFlags = "未##地";
               break;
            default:
               m_nPOS = 0;
               break;
         }
         return true;
      }

      #endregion

      #region IsGivenName Method

      //Judge whether the name is a given name
      public bool IsGivenName(string sName)
      {
         char sFirstChar, sSecondChar;
         double dGivenNamePossibility = 0, dSingleNamePossibility = 0;
         if (sName.Length != 2)
            return false;

         sFirstChar = sName.ToCharArray()[0];
         sSecondChar = sName.ToCharArray()[1];

         //The possibility of P(Wi|Ti)
         dGivenNamePossibility += Math.Log((double)m_dict.GetFrequency(sFirstChar.ToString(), 2) + 1.0)
           - Math.Log(m_roleTag.m_context.GetFrequency(0, 2) + 1.0);
         dGivenNamePossibility += Math.Log((double)m_dict.GetFrequency(sSecondChar.ToString(), 3) + 1.0)
            - Math.Log(m_roleTag.m_context.GetFrequency(0, 3) + 1.0);
         //The possibility of conversion from 2 to 3
         dGivenNamePossibility += Math.Log(m_roleTag.m_context.GetContextPossibility(0, 2, 3) + 1.0)
            - Math.Log(m_roleTag.m_context.GetFrequency(0, 2) + 1.0);

         //The possibility of P(Wi|Ti)
         dSingleNamePossibility += Math.Log((double)m_dict.GetFrequency(sFirstChar.ToString(), 1) + 1.0)
            - Math.Log(m_roleTag.m_context.GetFrequency(0, 1) + 1.0);
         dSingleNamePossibility += Math.Log((double)m_dict.GetFrequency(sSecondChar.ToString(), 4) + 1.0)
            - Math.Log(m_roleTag.m_context.GetFrequency(0, 4) + 1.0);
         //The possibility of conversion from 1 to 4
         dSingleNamePossibility += Math.Log(m_roleTag.m_context.GetContextPossibility(0, 1, 4) + 1.0)
            - Math.Log(m_roleTag.m_context.GetFrequency(0, 1) + 1.0);

         if (dSingleNamePossibility >= dGivenNamePossibility)
            //张震||m_dict.GetFrequency(sFirstChar,1)/m_dict.GetFrequency(sFirstChar,2)>=10
            //The possibility being a single given name is more than being a 2-char given name
            return false;
         return true;
      }

      #endregion

      #region ReleaseUnknowWord Method

      public void ReleaseUnknowWord()
      {
         m_dict.ReleaseDict();
         m_roleTag.ReleaseSpan();
      }

      #endregion

   }
}
