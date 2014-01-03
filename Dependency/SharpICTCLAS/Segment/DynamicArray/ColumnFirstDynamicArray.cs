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

namespace SharpICTCLAS
{
    [Serializable]
    public class ColumnFirstDynamicArray<T> : DynamicArray<T>
    {

        #region GetElement Method

        //====================================================================
        // 查找列为 nCol 的第一个结点
        //====================================================================
        public ChainItem<T> GetFirstElementOfCol(int nCol)
        {
            ChainItem<T> pCur = pHead;

            while (pCur != null && pCur.col != nCol)
                pCur = pCur.next;

            return pCur;
        }

        //====================================================================
        // 从 startFrom 处向后查找列为 nCol 的第一个结点
        //====================================================================
        public ChainItem<T> GetFirstElementOfCol(int nCol, ChainItem<T> startFrom)
        {
            ChainItem<T> pCur = startFrom;

            while (pCur != null && pCur.col != nCol)
                pCur = pCur.next;

            return pCur;
        }

        #endregion

        #region SetElement Method

        //====================================================================
        // 设置或插入一个新的结点
        //====================================================================
        public override void SetElement(int nRow, int nCol, T content)
        {
            ChainItem<T> pCur = pHead, pPre = null, pNew;  //The pointer of array chain

            if (nRow > RowCount)//Set the array row
                RowCount = nRow;

            if (nCol > ColumnCount)//Set the array col
                ColumnCount = nCol;

            while (pCur != null && (pCur.col < nCol || (pCur.col == nCol && pCur.row < nRow)))
            {
                pPre = pCur;
                pCur = pCur.next;
            }

            if (pCur != null && pCur.row == nRow && pCur.col == nCol)//Find the same position
                pCur.Content = content;//Set the value
            else
            {
                pNew = new ChainItem<T>();//malloc a new node
                pNew.col = nCol;
                pNew.row = nRow;
                pNew.Content = content;

                pNew.next = pCur;

                if (pPre == null)//link pNew after the pPre
                    pHead = pNew;
                else
                    pPre.next = pNew;
            }
        }

        #endregion

    }
}
