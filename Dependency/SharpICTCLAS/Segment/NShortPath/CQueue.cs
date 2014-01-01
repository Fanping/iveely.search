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
   internal class QueueElement
   {
      public int nParent;
      public int nIndex;
      public double eWeight;
      public QueueElement next = null;

      public QueueElement(int nParent, int nIndex, double eWeight)
      {
         this.nParent = nParent;
         this.nIndex = nIndex;
         this.eWeight = eWeight;
      }
   }

   internal class CQueue
   {
      private QueueElement pHead = null;
      private QueueElement pLastAccess = null;

      //====================================================================
      // 将QueueElement根据eWeight由小到大的顺序插入队列
      //====================================================================
      public void EnQueue(QueueElement newElement)
      {
         QueueElement pCur = pHead, pPre = null;

         while (pCur != null && pCur.eWeight < newElement.eWeight)
         {
            pPre = pCur;
            pCur = pCur.next;
         }

         newElement.next = pCur;

         if (pPre == null)
            pHead = newElement;
         else
            pPre.next = newElement;
      }

      //====================================================================
      // 从队列中取出前面的一个元素
      //====================================================================
      public QueueElement DeQueue()
      {
         if (pHead == null)
            return null;

         QueueElement pRet = pHead;
         pHead = pHead.next;

         return pRet;
      }

      //====================================================================
      // 读取第一个元素，但不执行DeQueue操作
      //====================================================================
      public QueueElement GetFirst()
      {
         pLastAccess = pHead;
         return pLastAccess;
      }

      //====================================================================
      // 读取上次读取后的下一个元素，不执行DeQueue操作
      //====================================================================
      public QueueElement GetNext()
      {
         if (pLastAccess != null)
            pLastAccess = pLastAccess.next;

         return pLastAccess;
      }

      //====================================================================
      // 是否仍然有下一个元素可供读取
      //====================================================================
      public bool CanGetNext
      {
         get
         {
            return (pLastAccess.next != null);
         }
      }

      //====================================================================
      // 清除所有元素
      //====================================================================
      public void Clear()
      {
         pHead = null;
         pLastAccess = null;
      }
   }
}
