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
   //==================================================
   // Original predefined in DynamicArray.h file
   //==================================================
   public class WordResult
   {
      //The word 
      public string sWord;

      //the POS of the word
      public int nPOS;

      //The -log(frequency/MAX)
      public double dValue;
   }

   //--------------------------------------------------
   // data structure for word item
   //--------------------------------------------------
   public class WordItem
   {
      public int nWordLen;

      //The word 
      public string sWord;

      //the process or information handle of the word
      public int nPOS;

      //The count which it appear
      public int nFrequency;
   }

   //--------------------------------------------------
   //data structure for dictionary index table item
   //--------------------------------------------------
   public class IndexTableItem
   {
      //The count number of words which initial letter is sInit
      public int nCount;

      //The  head of word items
      public WordItem[] WordItems;
   }

   //--------------------------------------------------
   //data structure for word item chain
   //--------------------------------------------------
   public class WordChain
   {
      public WordItem data;
      public WordChain next;
   }

   //--------------------------------------------------
   //data structure for dictionary index table item
   //--------------------------------------------------
   public class ModifyTableItem
   {
      //The count number of words which initial letter is sInit
      public int nCount;

      //The number of deleted items in the index table
      public int nDelete;

      //The head of word items
      public WordChain pWordItemHead;
   }

   //--------------------------------------------------
   // return value of GetWordInfos Method in Dictionary.cs
   //--------------------------------------------------
   public class WordInfo
   {
      public string sWord;
      public int Count = 0;

      public List<int> POSs = new List<int>();
      public List<int> Frequencies = new List<int>();
   }
}
