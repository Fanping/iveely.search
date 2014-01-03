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
using System.Text;
using System.Collections.Generic;

namespace SharpICTCLAS
{
    [Serializable]
    public abstract class DynamicArray<T>
    {
        protected ChainItem<T> pHead;  //The head pointer of array chain
        public int ColumnCount, RowCount;  //The row and col of the array

        #region Constructor

        public DynamicArray()
        {
            pHead = null;
            RowCount = 0;
            ColumnCount = 0;
        }

        #endregion

        #region ItemCount Property

        public int ItemCount
        {
            get
            {
                ChainItem<T> pCur = pHead;
                int nCount = 0;
                while (pCur != null)
                {
                    nCount++;
                    pCur = pCur.next;
                }
                return nCount;
            }
        }

        #endregion

        #region GetElement Method

        //====================================================================
        // 查找行、列值为nRow, nCol的结点
        //====================================================================
        public ChainItem<T> GetElement(int nRow, int nCol)
        {
            ChainItem<T> pCur = pHead;

            while (pCur != null && !(pCur.col == nCol && pCur.row == nRow))
                pCur = pCur.next;

            return pCur;
        }

        #endregion

        #region SetElement Method

        //====================================================================
        // 设置或插入一个新的结点
        //====================================================================
        public abstract void SetElement(int nRow, int nCol, T content);

        #endregion

        #region GetHead, GetTail, SetEmpty Method

        //====================================================================
        // Return the head element of ArrayChain
        //====================================================================
        public ChainItem<T> GetHead()
        {
            return pHead;
        }

        //====================================================================
        //Get the tail Element buffer and return the count of elements
        //====================================================================
        public int GetTail(out ChainItem<T> pTailRet)
        {
            ChainItem<T> pCur = pHead, pPrev = null;
            int nCount = 0;
            while (pCur != null)
            {
                nCount++;
                pPrev = pCur;
                pCur = pCur.next;
            }
            pTailRet = pPrev;
            return nCount;
        }

        //====================================================================
        // Set Empty
        //====================================================================
        public void SetEmpty()
        {
            pHead = null;
            ColumnCount = 0;
            RowCount = 0;
        }

        #endregion

        #region ToString Method

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ChainItem<T> pCur = pHead;

            while (pCur != null)
            {
                sb.Append(string.Format("row:{0,3},  col:{1,3},  ", pCur.row, pCur.col));
                sb.Append(pCur.Content.ToString());
                sb.Append("\r\n");
                pCur = pCur.next;
            }

            return sb.ToString();
        }

        #endregion

        #region

        public List<ChainItem<T>> ToListItems()
        {
            List<ChainItem<T>> result = new List<ChainItem<T>>();

            ChainItem<T> pCur = pHead;
            while (pCur != null)
            {
                result.Add(pCur);
                pCur = pCur.next;
            }

            return result;
        }

        #endregion
    }
}
