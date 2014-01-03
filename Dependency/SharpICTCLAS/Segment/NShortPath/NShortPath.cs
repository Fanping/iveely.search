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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace SharpICTCLAS
{
    [Serializable]
    public class NShortPath
    {
        private static ColumnFirstDynamicArray<ChainContent> m_apCost;
        private static int m_nValueKind; //The number of value kinds
        private static int m_nNode; //The number of Node in the graph
        private static CQueue[][] m_pParent; //The 2-dimension array for the nodes
        private static double[][] m_pWeight; //The weight of node

        #region 构造函数

        private NShortPath()
        {
        }

        #endregion

        #region InitNShortPath Method

        private static void InitNShortPath(ColumnFirstDynamicArray<ChainContent> apCost, int nValueKind)
        {
            m_apCost = apCost; //Set the cost
            m_nValueKind = nValueKind; //Set the value kind

            // 获取顶点的数目
            // ----------------- 注：by zhenyulu ------------------
            // 原来程序为m_nNode = Math.Max(apCost.ColumnCount, apCost.RowCount) + 1;
            // 但apCost.ColumnCount应该一定大于apCost.RowCount，所以改成这样。
            m_nNode = apCost.ColumnCount + 1;

            m_pParent = new CQueue[m_nNode - 1][]; //not including the first node
            m_pWeight = new double[m_nNode - 1][];

            //The queue array for every node
            for (int i = 0; i < m_nNode - 1; i++)
            {
                m_pParent[i] = new CQueue[nValueKind];
                m_pWeight[i] = new double[nValueKind];

                for (int j = 0; j < nValueKind; j++)
                    m_pParent[i][j] = new CQueue();
            }
        }

        #endregion

        #region Calculate Method

        //====================================================================
        // 计算出所有结点上可能的路径，为路径数据提供数据准备
        //====================================================================
        public static void Calculate(ColumnFirstDynamicArray<ChainContent> apCost, int nValueKind)
        {
            InitNShortPath(apCost, nValueKind);

            QueueElement tmpElement;
            CQueue queWork = new CQueue();
            double eWeight;

            for (int nCurNode = 1; nCurNode < m_nNode; nCurNode++)
            {
                // 将所有到当前结点（nCurNode)可能的边根据eWeight排序并压入队列
                EnQueueCurNodeEdges(ref queWork, nCurNode);

                // 初始化当前结点所有边的eWeight值
                for (int i = 0; i < m_nValueKind; i++)
                    m_pWeight[nCurNode - 1][i] = Predefine.INFINITE_VALUE;

                // 将queWork中的内容装入m_pWeight与m_pParent
                tmpElement = queWork.DeQueue();
                if (tmpElement != null)
                {
                    for (int i = 0; i < m_nValueKind; i++)
                    {
                        eWeight = tmpElement.eWeight;
                        m_pWeight[nCurNode - 1][i] = eWeight;
                        do
                        {
                            m_pParent[nCurNode - 1][i].EnQueue(new QueueElement(tmpElement.nParent, tmpElement.nIndex, 0));
                            tmpElement = queWork.DeQueue();
                            if (tmpElement == null)
                                goto nextnode;

                        } while (tmpElement.eWeight == eWeight);
                    }
                }
            nextnode: ;
            }
        }

        //====================================================================
        // 将所有到当前结点（nCurNode）可能的边根据eWeight排序并压入队列
        //====================================================================
        private static void EnQueueCurNodeEdges(ref CQueue queWork, int nCurNode)
        {
            int nPreNode;
            double eWeight;
            ChainItem<ChainContent> pEdgeList;

            queWork.Clear();
            pEdgeList = m_apCost.GetFirstElementOfCol(nCurNode);

            // Get all the edges
            while (pEdgeList != null && pEdgeList.col == nCurNode)
            {
                nPreNode = pEdgeList.row;  // 很特别的命令，利用了row与col的关系
                eWeight = pEdgeList.Content.eWeight; //Get the eWeight of edges

                for (int i = 0; i < m_nValueKind; i++)
                {
                    // 第一个结点，没有PreNode，直接加入队列
                    if (nPreNode == 0)
                    {
                        queWork.EnQueue(new QueueElement(nPreNode, i, eWeight));
                        break;
                    }

                    // 如果PreNode的Weight == Predefine.INFINITE_VALUE，则没有必要继续下去了
                    if (m_pWeight[nPreNode - 1][i] == Predefine.INFINITE_VALUE)
                        break;

                    queWork.EnQueue(new QueueElement(nPreNode, i, eWeight + m_pWeight[nPreNode - 1][i]));
                }
                pEdgeList = pEdgeList.next;
            }
        }

        #endregion

        #region GetPaths Method

        //====================================================================
        // 注：index ＝ 0 : 最短的路径； index = 1 ： 次短的路径
        //     依此类推。index <= this.m_nValueKind
        //====================================================================
        public static List<int[]> GetPaths(int index)
        {
            Debug.Assert(index <= m_nValueKind && index >= 0);

            Stack<PathNode> stack = new Stack<PathNode>();
            int curNode = m_nNode - 1, curIndex = index;
            QueueElement element;
            PathNode node;
            int[] aPath;
            List<int[]> result = new List<int[]>();

            element = m_pParent[curNode - 1][curIndex].GetFirst();
            while (element != null)
            {
                // ---------- 通过压栈得到路径 -----------
                stack.Push(new PathNode(curNode, curIndex));
                stack.Push(new PathNode(element.nParent, element.nIndex));
                curNode = element.nParent;

                while (curNode != 0)
                {
                    element = m_pParent[element.nParent - 1][element.nIndex].GetFirst();
                    stack.Push(new PathNode(element.nParent, element.nIndex));
                    curNode = element.nParent;
                }

                // -------------- 输出路径 --------------
                PathNode[] nArray = stack.ToArray();
                aPath = new int[nArray.Length];

                for (int i = 0; i < aPath.Length; i++)
                    aPath[i] = nArray[i].nParent;

                result.Add(aPath);

                // -------------- 出栈以检查是否还有其它路径 --------------
                do
                {
                    node = stack.Pop();
                    curNode = node.nParent;
                    curIndex = node.nIndex;

                } while (curNode < 1 || (stack.Count != 0 && !m_pParent[curNode - 1][curIndex].CanGetNext));

                element = m_pParent[curNode - 1][curIndex].GetNext();
            }

            return result;
        }

        #endregion

        #region GetBestPath Method

        //====================================================================
        // 获取唯一一条最短路径，当然最短路径可能不只一条
        //====================================================================
        public static int[] GetBestPath()
        {
            Debug.Assert(m_nNode > 2);

            Stack<int> stack = new Stack<int>();
            int curNode = m_nNode - 1, curIndex = 0;
            QueueElement element;

            element = m_pParent[curNode - 1][curIndex].GetFirst();

            stack.Push(curNode);
            stack.Push(element.nParent);
            curNode = element.nParent;

            while (curNode != 0)
            {
                element = m_pParent[element.nParent - 1][element.nIndex].GetFirst();
                stack.Push(element.nParent);
                curNode = element.nParent;
            }

            return stack.ToArray();
        }

        #endregion

        #region GetNPaths Method

        //====================================================================
        // 从短到长获取至多 n 条路径
        //====================================================================
        public static List<int[]> GetNPaths(int n)
        {
            List<int[]> result = new List<int[]>();
            List<int[]> tmp;
            int nCopy;

            for (int i = 0; i < m_nValueKind && result.Count < Predefine.MAX_SEGMENT_NUM; i++)
            {
                tmp = GetPaths(i);

                if (n - result.Count < tmp.Count)
                    nCopy = n - result.Count;
                else
                    nCopy = tmp.Count;

                for (int j = 0; j < nCopy; j++)
                    result.Add(tmp[j]);
            }

            return result;
        }

        #endregion

        #region 用于输出中间结果的测试代码

        public static void printResultByIndex()
        {
            QueueElement e;

            for (int i = 0; i < m_nValueKind; i++)
            {
                Console.WriteLine("\n\r============ Index = {0} ============", i);
                for (int nCurNode = 1; nCurNode < m_nNode; nCurNode++)
                {
                    Console.WriteLine("Node Num: {0}", nCurNode);

                    e = m_pParent[nCurNode - 1][i].GetFirst();
                    while (e != null)
                    {
                        Console.WriteLine("({0}, {1})  eWeight = {2}", e.nParent, e.nIndex, m_pWeight[nCurNode - 1][i]);
                        e = m_pParent[nCurNode - 1][i].GetNext();
                    }
                    Console.WriteLine("---------------------");
                }
            }
        }

        public static void printResultByNode()
        {
            QueueElement e;

            for (int nCurNode = 1; nCurNode < m_nNode; nCurNode++)
            {
                Console.WriteLine("\n\r============ 第 {0} 个结点 ============", nCurNode);
                for (int i = 0; i < m_nValueKind; i++)
                {
                    Console.WriteLine("N: {0}", i);

                    e = m_pParent[nCurNode - 1][i].GetFirst();
                    while (e != null)
                    {
                        Console.WriteLine("({0}, {1})  eWeight = {2}", e.nParent, e.nIndex, m_pWeight[nCurNode - 1][i]);
                        e = m_pParent[nCurNode - 1][i].GetNext();
                    }
                    Console.WriteLine("---------------------");
                }
            }
        }

        #endregion

    }
}
