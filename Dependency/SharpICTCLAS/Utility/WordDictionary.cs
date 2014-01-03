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
using System.IO;

namespace SharpICTCLAS
{
    public class WordDictionary
    {
        public bool bReleased = true;

        public IndexTableItem[] indexTable;
        public ModifyTableItem[] modifyTable;

        #region Load Method

        public bool Load(string sFilename)
        {
            return Load(sFilename, false);
        }

        //====================================================================
        // Func Name  : Load
        // Description: Load the dictionary from the file .dct
        // Parameters : sFilename: the file name
        // Returns    : success or fail
        //====================================================================
        public bool Load(string sFilename, bool bReset)
        {
            int frequency, wordLength, pos;   //频率、词长、读取词性
            bool isSuccess = true;
            FileStream fileStream = null;
            BinaryReader binReader = null;

            try
            {
                fileStream = new FileStream(sFilename, FileMode.Open, FileAccess.Read);
                if (fileStream == null)
                    return false;

                binReader = new BinaryReader(fileStream, Encoding.GetEncoding("gb2312"));

                indexTable = new IndexTableItem[Predefine.CC_NUM];

                bReleased = false;
                for (int i = 0; i < Predefine.CC_NUM; i++)
                {
                    //读取以该汉字打头的词有多少个
                    indexTable[i] = new IndexTableItem();
                    indexTable[i].nCount = binReader.ReadInt32();

                    if (indexTable[i].nCount <= 0)
                        continue;

                    indexTable[i].WordItems = new WordItem[indexTable[i].nCount];

                    for (int j = 0; j < indexTable[i].nCount; j++)
                    {
                        indexTable[i].WordItems[j] = new WordItem();

                        frequency = binReader.ReadInt32();   //读取频率
                        wordLength = binReader.ReadInt32();  //读取词长
                        pos = binReader.ReadInt32();      //读取词性

                        if (wordLength > 0)
                            indexTable[i].WordItems[j].sWord = Utility.ByteArray2String(binReader.ReadBytes(wordLength));
                        else
                            indexTable[i].WordItems[j].sWord = "";

                        //Reset the frequency
                        if (bReset)
                            indexTable[i].WordItems[j].nFrequency = 0;
                        else
                            indexTable[i].WordItems[j].nFrequency = frequency;

                        indexTable[i].WordItems[j].nWordLen = wordLength;
                        indexTable[i].WordItems[j].nPOS = pos;
                    }
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

        #region Save Method

        //====================================================================
        // Func Name  : Save
        // Description: Save the dictionary as the file .dct
        // Parameters : sFilename: the file name
        // Returns    : success or fail
        //====================================================================
        public bool Save(string sFilename)
        {
            bool isSuccess = true;
            FileStream outputFile = null;
            BinaryWriter writer = null;

            try
            {
                outputFile = new FileStream(sFilename, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new BinaryWriter(outputFile, Encoding.GetEncoding("gb2312"));

                //对图一中所示的6768个数据块进行遍历
                for (int i = 0; i < Predefine.CC_NUM; i++)
                {
                    //如果发生了修改，则完成indexTable与modifyTable归并排序式的合并工作并存盘（排序原则是先安sWord排，然后再按词性排）
                    if (modifyTable != null)
                        MergeAndSaveIndexTableItem(writer, indexTable[i], modifyTable[i]);
                    else
                        //否则直接写入indexTable
                        SaveIndexTableItem(writer, indexTable[i]);
                }
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
            }
            return isSuccess;
        }

        private void MergeAndSaveIndexTableItem(BinaryWriter writer, IndexTableItem item, ModifyTableItem modifyItem)
        {
            int j, nCount;   //频率、词长、读取词性
            WordChain pCur;

            //计算修改后有效词块的数目
            nCount = item.nCount + modifyItem.nCount - modifyItem.nDelete;
            writer.Write(nCount);

            pCur = modifyItem.pWordItemHead;

            j = 0;
            //对原表中的词块和修改表中的词块进行遍历,并把修改后的添加到原表中
            while (pCur != null && j < item.nCount)
            {
                //如果修改表中的词小于原表中对应位置的词或者长度相等但nHandle值比原表中的小,则把修改表中的写入到词典文件当中.
                if (Utility.CCStringCompare(pCur.data.sWord, item.WordItems[j].sWord) < 0 ||
                   ((pCur.data.sWord == item.WordItems[j].sWord) &&
                   (pCur.data.nPOS < item.WordItems[j].nPOS)))
                {
                    //Output the modified data to the file
                    SaveWordItem(writer, pCur.data);
                    pCur = pCur.next;
                }
                //频度nFrequecy等于-1说明该词已被删除,跳过它
                else if (item.WordItems[j].nFrequency == -1)
                    j++;
                //如果修改表中的词长度比原表中的长度大或  长度相等但句柄值要多,就把原表的词写入的词典文件中
                else if (Utility.CCStringCompare(pCur.data.sWord, item.WordItems[j].sWord) > 0 ||
                   ((pCur.data.sWord == item.WordItems[j].sWord) &&
                   (pCur.data.nPOS > item.WordItems[j].nPOS)))
                {
                    //Output the index table data to the file
                    SaveWordItem(writer, item.WordItems[j]);
                    j++;
                }
            }
            //如果归并结束后indexTable有剩余，则继续写完indexTable中的数据
            if (j < item.nCount)
            {
                for (int i = j; i < item.nCount; i++)
                    if (item.WordItems[j].nFrequency != -1)
                        SaveWordItem(writer, item.WordItems[i]);
            }
            //否则继续写完modifyTable中的数据
            else
                while (pCur != null)
                {
                    //Output the modified data to the file
                    SaveWordItem(writer, pCur.data);
                    pCur = pCur.next;
                }
        }

        private void SaveIndexTableItem(BinaryWriter writer, IndexTableItem item)
        {
            writer.Write(item.nCount);

            for (int i = 0; i < item.nCount; i++)
                SaveWordItem(writer, item.WordItems[i]);
        }

        private void SaveWordItem(BinaryWriter writer, WordItem item)
        {
            int frequency = item.nFrequency;
            int wordLength = item.nWordLen;
            int handle = item.nPOS;

            writer.Write(frequency);
            writer.Write(wordLength);
            writer.Write(handle);

            if (wordLength > 0)
                writer.Write(Encoding.GetEncoding("gb2312").GetBytes(item.sWord));
        }

        #endregion

        #region AddItem Method
        //====================================================================
        // Func Name  : AddItem
        // Description: Add a word item to the dictionary
        // Parameters : sWord: the word
        //              nHandle:the handle number
        //              nFrequency: the frequency
        // Returns    : success or fail
        //====================================================================
        public bool AddItem(string sWord, int nPOS, int nFrequency)
        {
            int nPos, nFoundPos;
            WordChain pRet, pTemp, pNext;
            string sWordAdd;

            //预处理,去掉词的前后的空格
            if (!PreProcessing(ref sWord, out nPos, out sWordAdd))
                return false;

            if (FindInOriginalTable(nPos, sWordAdd, nPOS, out nFoundPos))
            {
                //The word exists in the original table, so add the frequency
                //Operation in the index table and its items
                if (indexTable[nPos].WordItems[nFoundPos].nFrequency == -1)
                {
                    //The word item has been removed
                    indexTable[nPos].WordItems[nFoundPos].nFrequency = nFrequency;

                    if (modifyTable == null)
                        modifyTable = new ModifyTableItem[Predefine.CC_NUM];

                    modifyTable[nPos].nDelete -= 1;
                }
                else
                    indexTable[nPos].WordItems[nFoundPos].nFrequency += nFrequency;
                return true;
            }

            //The items not exists in the index table.
            //As following, we have to find the item whether exists in the modify data region
            //If exists, change the frequency .or else add a item
            if (modifyTable == null)
            {
                modifyTable = new ModifyTableItem[Predefine.CC_NUM];
                for (int i = 0; i < Predefine.CC_NUM; i++)
                    modifyTable[i] = new ModifyTableItem();
            }

            if (FindInModifyTable(nPos, sWordAdd, nPOS, out pRet))
            {
                if (pRet != null)
                    pRet = pRet.next;
                else
                    pRet = modifyTable[nPos].pWordItemHead;

                pRet.data.nFrequency += nFrequency;
                return true;
            }

            //find the proper position to add the word to the modify data table and link
            pTemp = new WordChain(); //Allocate the word chain node
            pTemp.data = new WordItem();
            pTemp.data.nPOS = nPOS; //store the handle
            pTemp.data.nWordLen = Utility.GetWordLength(sWordAdd);
            pTemp.data.sWord = sWordAdd;
            pTemp.data.nFrequency = nFrequency;
            pTemp.next = null;
            if (pRet != null)
            {
                pNext = pRet.next; //Get the next item before the current item
                pRet.next = pTemp; //link the node to the chain
            }
            else
            {
                pNext = modifyTable[nPos].pWordItemHead;
                modifyTable[nPos].pWordItemHead = pTemp; //Set the pAdd as the head node
            }
            pTemp.next = pNext; //Very important!!!! or else it will lose some node

            modifyTable[nPos].nCount++; //the number increase by one
            return true;
        }

        #endregion

        #region DelItem Method

        public bool DelItem(string sWord, int nPOS)
        {
            string sWordDel;
            int nPos, nFoundPos, nTemp;
            WordChain pPre, pTemp, pCur;

            if (!PreProcessing(ref sWord, out nPos, out sWordDel))
                return false;

            if (FindInOriginalTable(nPos, sWordDel, nPOS, out nFoundPos))
            {
                //Not prepare the buffer
                if (modifyTable == null)
                    modifyTable = new ModifyTableItem[Predefine.CC_NUM];

                indexTable[nPos].WordItems[nFoundPos].nFrequency = -1;
                modifyTable[nPos].nDelete += 1;

                //Remove all items which word is sWordDel,ignoring the handle
                if (nPOS == -1)
                {
                    nTemp = nFoundPos + 1; //Check its previous position
                    while (nTemp < indexTable[nPos].nCount &&
                       string.Compare(indexTable[nPos].WordItems[nFoundPos].sWord, sWordDel) == 0)
                    {
                        indexTable[nPos].WordItems[nTemp].nFrequency = -1;
                        modifyTable[nPos].nDelete += 1;
                        nTemp += 1;
                    }
                }
                return true;
            }

            //Operation in the modify table and its items
            if (FindInModifyTable(nPos, sWordDel, nPOS, out pPre))
            {
                pCur = modifyTable[nPos].pWordItemHead;
                if (pPre != null)
                    pCur = pPre.next;
                while (pCur != null && string.Compare(pCur.data.sWord, sWordDel, true) == 0 &&
                   (pCur.data.nPOS == nPOS || nPOS < 0))
                {
                    pTemp = pCur;
                    //pCur is the first item
                    if (pPre != null)
                        pPre.next = pCur.next;
                    else
                        modifyTable[nPos].pWordItemHead = pCur.next;

                    pCur = pCur.next;
                }
                return true;
            }
            return false;
        }

        #endregion

        #region IsExist Method

        //====================================================================
        // Func Name  : IsExist
        // Description: Check the sWord with nHandle whether exist
        // Parameters : sWord: the word
        //            : nHandle: the nHandle
        // Returns    : Is Exist
        //====================================================================
        public bool IsExist(string sWord, int nHandle)
        {
            string sWordFind;
            int nPos;

            if (!PreProcessing(ref sWord, out nPos, out sWordFind))
                return false;

            return (FindInOriginalTable(nPos, sWordFind, nHandle) || FindInModifyTable(nPos, sWordFind, nHandle));
        }

        #endregion

        #region GetWordType Method

        //====================================================================
        // Func Name  : GetWordType
        // Description: Get the type of word
        // Parameters : sWord: the word
        // Returns    : the type
        //====================================================================
        public int GetWordType(string sWord)
        {
            int nType = Utility.charType(sWord.ToCharArray()[0]);
            int nLen = Utility.GetWordLength(sWord);

            //Chinese word
            if (nLen > 0 && nType == Predefine.CT_CHINESE && Utility.IsAllChinese(sWord))
                return Predefine.WT_CHINESE;
            //Delimiter
            else if (nLen > 0 && nType == Predefine.CT_DELIMITER)
                return Predefine.WT_DELIMITER;
            //other invalid
            else
                return Predefine.WT_OTHER;
        }

        #endregion

        #region GetWordInfo Method

        public WordInfo GetWordInfo(string sWord)
        {
            WordInfo info = new WordInfo();
            info.sWord = sWord;

            string sWordGet;
            int nFirstCharId, nFoundPos;
            WordChain pPre, pCur;

            if (!PreProcessing(ref sWord, out nFirstCharId, out sWordGet))
                return null;

            if (FindFirstMatchItemInOrgTbl(nFirstCharId, sWordGet, out nFoundPos))
            {
                while (nFoundPos < indexTable[nFirstCharId].nCount && string.Compare(indexTable[nFirstCharId].WordItems[nFoundPos].sWord, sWordGet) == 0)
                {
                    info.POSs.Add(indexTable[nFirstCharId].WordItems[nFoundPos].nPOS);
                    info.Frequencies.Add(indexTable[nFirstCharId].WordItems[nFoundPos].nFrequency);
                    info.Count++;

                    nFoundPos++;
                }
                return info;
            }

            //Operation in the index table and its items
            if (FindInModifyTable(nFirstCharId, sWordGet, out pPre))
            {
                pCur = modifyTable[nFirstCharId].pWordItemHead;

                if (pPre != null)
                    pCur = pPre.next;

                while (pCur != null && string.Compare(pCur.data.sWord, sWordGet, true) == 0)
                {
                    info.POSs.Add(pCur.data.nPOS);
                    info.Frequencies.Add(pCur.data.nFrequency);
                    info.Count++;
                    pCur = pCur.next;
                }
                return info;
            }
            return null;
        }

        #endregion

        #region GetMaxMatch Method

        //====================================================================
        // Func Name  : GetMaxMatch
        // Description: Get the max match to the word
        // Parameters : nHandle: the only handle which will be attached to the word
        // Returns    : success or fail
        //====================================================================
        public bool GetMaxMatch(string sWord, out string sWordRet, out int nPOSRet)
        {
            string sWordGet, sFirstChar;
            int nFirstCharId;
            WordChain pCur;

            sWordRet = "";
            nPOSRet = -1;

            if (!PreProcessing(ref sWord, out nFirstCharId, out sWordGet))
                return false;

            sFirstChar = Utility.CC_ID2Char(nFirstCharId).ToString();

            //在indexTable中检索以sWordGet打头的项目
            int i = 0;
            while (i < indexTable[nFirstCharId].nCount)
            {
                if (indexTable[nFirstCharId].WordItems[i].sWord.StartsWith(sWordGet))
                {
                    sWordRet = sFirstChar + indexTable[nFirstCharId].WordItems[i].sWord;
                    nPOSRet = indexTable[nFirstCharId].WordItems[i].nPOS;
                    return true;
                }
                i++;
            }

            //在indexTable中没能找到，到modifyTable中去找
            if (modifyTable == null)
                return false;

            pCur = modifyTable[nFirstCharId].pWordItemHead;
            while (pCur != null)
            {
                if (pCur.data.sWord.StartsWith(sWordGet))
                {
                    sWordRet = sFirstChar + pCur.data.sWord;
                    nPOSRet = pCur.data.nPOS;
                    return true;
                }
                pCur = pCur.next;
            }

            return false;
        }

        #endregion

        #region GetFrequency Method

        //====================================================================
        // 查找词性为nPOS的sWord的词频
        //====================================================================
        public int GetFrequency(string sWord, int nPOS)
        {
            string sWordFind;
            int firstCharCC_ID, nIndex;
            WordChain pFound;

            if (!PreProcessing(ref sWord, out firstCharCC_ID, out sWordFind))
                return 0;

            if (FindInOriginalTable(firstCharCC_ID, sWordFind, nPOS, out nIndex))
                return indexTable[firstCharCC_ID].WordItems[nIndex].nFrequency;

            if (FindInModifyTable(firstCharCC_ID, sWordFind, nPOS, out pFound))
                return pFound.data.nFrequency;

            return 0;
        }

        #endregion

        #region ReleaseDict

        public void ReleaseDict()
        {
            for (int i = 0; i < Predefine.CC_NUM; i++)
                for (int j = 0; indexTable[i] != null && j < indexTable[i].nCount; j++)
                    indexTable[i] = null;

            modifyTable = null;
        }

        #endregion

        #region MergePOS Method

        //====================================================================
        // Func Name  : MergePOS
        // Description: Merge all the POS into nPOS,
        //              just get the word in the dictionary and set its POS as nPOS
        // Parameters : nPOS: the only handle which will be attached to the word
        // Returns    : the type
        //====================================================================
        public bool MergePOS(int nPOS)
        {
            int i, j, nCompare;
            string sWordPrev;
            WordChain pPre, pCur, pTemp;

            //Not prepare the buffer
            if (modifyTable == null)
                modifyTable = new ModifyTableItem[Predefine.CC_NUM];

            //Operation in the index table
            for (i = 0; i < Predefine.CC_NUM; i++)
            {
                //delete the memory of word item array in the dictionary
                sWordPrev = null; //Set empty
                for (j = 0; j < indexTable[i].nCount; j++)
                {
                    nCompare = Utility.CCStringCompare(sWordPrev, indexTable[i].WordItems[j].sWord);
                    if ((j == 0 || nCompare < 0) && indexTable[i].WordItems[j].nFrequency != -1)
                    {
                        //Need to modify its handle
                        indexTable[i].WordItems[j].nPOS = nPOS; //Change its handle
                        sWordPrev = indexTable[i].WordItems[j].sWord;
                        //Refresh previous Word
                    }
                    else if (nCompare == 0 && indexTable[i].WordItems[j].nFrequency != -1)
                    {
                        //Need to delete when not delete and same as previous word
                        indexTable[i].WordItems[j].nFrequency = -1; //Set delete flag
                        modifyTable[i].nDelete += 1; //Add the number of being deleted
                    }
                }
            }
            for (i = 0; i < Predefine.CC_NUM; i++)
            //Operation in the modify table
            {
                pPre = null;
                pCur = modifyTable[i].pWordItemHead;
                sWordPrev = null; //Set empty
                while (pCur != null)
                {
                    if (Utility.CCStringCompare(pCur.data.sWord, sWordPrev) > 0)
                    {
                        //The new word
                        pCur.data.nPOS = nPOS; //Chang its handle
                        sWordPrev = pCur.data.sWord; //Set new previous word
                        pPre = pCur; //New previous pointer
                        pCur = pCur.next;
                    }
                    else
                    {
                        //The same word as previous,delete it.
                        pTemp = pCur;
                        if (pPre != null)
                            //pCur is the first item
                            pPre.next = pCur.next;
                        else
                            modifyTable[i].pWordItemHead = pCur.next;
                        pCur = pCur.next;
                    }
                }
            }
            return true;
        }

        #endregion

        #region ToTextFile Method

        public bool ToTextFile(string sFileName)
        {
            bool isSuccess = true;
            FileStream outputFile = null;
            StreamWriter writer = null;

            //Modification made, not to output when modify table exists.
            if (modifyTable != null)
                return false;

            try
            {
                outputFile = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new StreamWriter(outputFile, Encoding.GetEncoding("gb2312"));

                for (int j = 0; j < Predefine.CC_NUM; j++)
                {
                    writer.WriteLine("====================================\r\n汉字:{0}, ID ：{1}\r\n", Utility.CC_ID2Char(j), j);

                    writer.WriteLine("  词长  频率  词性   词");
                    for (int i = 0; i < indexTable[j].nCount; i++)
                        writer.WriteLine("{0,5} {1,6} {2,5}  ({3}){4}",
                           indexTable[j].WordItems[i].nWordLen,
                           indexTable[j].WordItems[i].nFrequency,
                           Utility.GetPOSString(indexTable[j].WordItems[i].nPOS),
                           Utility.CC_ID2Char(j),
                           indexTable[j].WordItems[i].sWord);
                }
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
            }
            return isSuccess;
        }

        #endregion

        #region Merge Method

        //====================================================================
        //Merge dict2 into current dictionary and the frequency ratio from dict2 and current dict is nRatio
        //====================================================================
        public bool Merge(WordDictionary dict2, int nRatio)
        {
            int i, j, k, nCmpValue;
            string sWord;

            //Modification made, not to output when modify table exists.
            if (modifyTable != null || dict2.modifyTable != null)
                return false;

            for (i = 0; i < Predefine.CC_NUM; i++)
            {
                j = 0;
                k = 0;
                while (j < indexTable[i].nCount && k < dict2.indexTable[i].nCount)
                {
                    nCmpValue = Utility.CCStringCompare(indexTable[i].WordItems[j].sWord, dict2.indexTable[i].WordItems[k].sWord);
                    if (nCmpValue == 0)
                    //Same Words and determine the different handle
                    {
                        if (indexTable[i].WordItems[j].nPOS < dict2.indexTable[i].WordItems[k].nPOS)
                            nCmpValue = -1;
                        else if (indexTable[i].WordItems[j].nPOS > dict2.indexTable[i].WordItems[k].nPOS)
                            nCmpValue = 1;
                    }

                    if (nCmpValue == 0)
                    {
                        indexTable[i].WordItems[j].nFrequency = (nRatio * indexTable[i].WordItems[j].nFrequency + dict2.indexTable[i].WordItems[k].nFrequency) / (nRatio + 1);
                        j += 1;
                        k += 1;
                    }
                    //Get next word in the current dictionary
                    else if (nCmpValue < 0)
                    {
                        indexTable[i].WordItems[j].nFrequency = (nRatio * indexTable[i].WordItems[j].nFrequency) / (nRatio + 1);
                        j += 1;
                    }
                    else
                    //Get next word in the second dictionary
                    {
                        if (dict2.indexTable[i].WordItems[k].nFrequency > (nRatio + 1) / 10)
                        {
                            sWord = string.Format("{0}{1}", Utility.CC_ID2Char(i).ToString(), dict2.indexTable[i].WordItems[k].sWord);
                            AddItem(sWord, dict2.indexTable[i].WordItems[k].nPOS, dict2.indexTable[i].WordItems[k].nFrequency / (nRatio + 1));
                        }
                        k += 1;
                    }
                }

                //words in current dictionary are left
                while (j < indexTable[i].nCount)
                {
                    indexTable[i].WordItems[j].nFrequency = (nRatio * indexTable[i].WordItems[j].nFrequency) / (nRatio + 1);
                    j += 1;
                }

                //words in Dict2 are left
                while (k < dict2.indexTable[i].nCount)
                {
                    if (dict2.indexTable[i].WordItems[k].nFrequency > (nRatio + 1) / 10)
                    {
                        sWord = string.Format("{0}{1}", Utility.CC_ID2Char(i).ToString(), dict2.indexTable[i].WordItems[k].sWord);
                        AddItem(sWord, dict2.indexTable[i].WordItems[k].nPOS, dict2.indexTable[i].WordItems[k].nFrequency / (nRatio + 1));
                    }
                    k += 1;
                }
            }
            return true;
        }

        #endregion

        #region Optimum Method

        //====================================================================
        //Delete word item which
        //(1)frequency is 0
        //(2)word is same as following but the POS value is parent set of the following
        //for example "江泽民/n/0" will deleted, because "江泽民/nr/0" is more detail and correct
        //====================================================================
        public bool Optimum()
        {
            int nPrevPOS, i, j, nPrevFreq;
            string sPrevWord, sCurWord;
            for (i = 0; i < Predefine.CC_NUM; i++)
            {
                j = 0;
                sPrevWord = null;
                nPrevPOS = 0;
                nPrevFreq = -1;
                while (j < indexTable[i].nCount)
                {
                    sCurWord = string.Format("{0}{1}", Utility.CC_ID2Char(i).ToString(), indexTable[i].WordItems[j].sWord);
                    if (nPrevPOS == 30720 || nPrevPOS == 26368 || nPrevPOS == 29031 ||
                      (sPrevWord == sCurWord && nPrevFreq == 0 && indexTable[i].WordItems[j].nPOS / 256 * 256 == nPrevPOS))
                    {
                        //Delete Previous word item
                        //Delete word with POS 'x','g' 'qg'
                        DelItem(sPrevWord, nPrevPOS);
                    }
                    sPrevWord = sCurWord;
                    nPrevPOS = indexTable[i].WordItems[j].nPOS;
                    nPrevFreq = indexTable[i].WordItems[j].nFrequency;
                    j += 1; //Get next item in the original table.
                }
            }
            return true;
        }

        #endregion

        #region Private Functions

        #region PreProcessing Method

        //====================================================================
        // Func Name  : PreProcessing
        // Description: Get the type of word
        // Parameters : sWord: the word
        // Returns    : the type
        //====================================================================
        private bool PreProcessing(ref string sWord, out int nId, out string sWordRet)
        {
            if (sWord != " ")
                sWord = sWord.Trim();

            //Position for the delimeters
            int nType = Utility.charType(sWord.ToCharArray()[0]);

            if (sWord.Length != 0)
            {
                //Chinese word
                if (nType == Predefine.CT_CHINESE)
                {
                    //Get the inner code of the first Chinese Char
                    byte[] byteArray = Utility.String2ByteArray(sWord);
                    nId = Utility.CC_ID(byteArray[0], byteArray[1]);

                    //store the word,not store the first Chinese Char
                    sWordRet = sWord.Substring(1);
                    return true;
                }

                //Delimiter
                if (nType == Predefine.CT_DELIMITER)
                {
                    nId = 3755;
                    //Get the inner code of the first Chinese Char
                    sWordRet = sWord; //store the word, not store the first Chinese Char
                    return true;
                }
            }

            nId = 0;
            sWordRet = "";
            return false; //other invalid
        }

        #endregion

        #region FindInOriginalTable Method

        //====================================================================
        // Func Name  : FindInOriginalTable
        // Description: judge the word and handle exist in the inner table and its items
        // Parameters : nInnerCode: the inner code of the first CHines char
        //              sWord: the word
        //              nHandle:the handle number
        //              *nPosRet:the position which node is matched
        // Returns    : success or fail
        //====================================================================
        private bool FindInOriginalTable(int nInnerCode, string sWord, int nPOS, out int nPosRet)
        {
            WordItem[] pItems = indexTable[nInnerCode].WordItems;

            int nStart = 0, nEnd = indexTable[nInnerCode].nCount - 1;
            int nMid = (nStart + nEnd) / 2, nCmpValue;

            while (nStart <= nEnd)
            //Binary search
            {
                nCmpValue = Utility.CCStringCompare(pItems[nMid].sWord, sWord);
                if (nCmpValue == 0 && (pItems[nMid].nPOS == nPOS || nPOS == -1))
                {
                    if (nPOS == -1)
                    //Not very strict match
                    {
                        nMid -= 1;
                        while (nMid >= 0 && string.Compare(pItems[nMid].sWord, sWord) == 0)
                            //Get the first item which match the current word
                            nMid--;
                        if (nMid < 0 || string.Compare(pItems[nMid].sWord, sWord) != 0)
                            nMid++;
                    }
                    nPosRet = nMid;
                    return true;//find it
                }
                else if (nCmpValue < 0 || (nCmpValue == 0 && pItems[nMid].nPOS < nPOS && nPOS != -1))
                {
                    nStart = nMid + 1;
                }
                else if (nCmpValue > 0 || (nCmpValue == 0 && pItems[nMid].nPOS > nPOS && nPOS != -1))
                {
                    nEnd = nMid - 1;
                }
                nMid = (nStart + nEnd) / 2;
            }

            //Get the previous position
            nPosRet = nMid - 1;
            return false;
        }

        //====================================================================
        // Func Name  : FindInOriginalTable
        // Description: judge the word and handle exist in the inner table and its items
        // Parameters : nInnerCode: the inner code of the first CHines char
        //              sWord: the word
        //              nHandle:the handle number
        // Returns    : success or fail
        //====================================================================
        private bool FindInOriginalTable(int nInnerCode, string sWord, int nPOS)
        {
            WordItem[] pItems = indexTable[nInnerCode].WordItems;

            int nStart = 0, nEnd = indexTable[nInnerCode].nCount - 1;
            int nMid = (nStart + nEnd) / 2, nCmpValue;

            //Binary search
            while (nStart <= nEnd)
            {
                nCmpValue = Utility.CCStringCompare(pItems[nMid].sWord, sWord);

                if (nCmpValue == 0 && (pItems[nMid].nPOS == nPOS || nPOS == -1))
                    return true;//find it
                else if (nCmpValue < 0 || (nCmpValue == 0 && pItems[nMid].nPOS < nPOS && nPOS != -1))
                    nStart = nMid + 1;
                else if (nCmpValue > 0 || (nCmpValue == 0 && pItems[nMid].nPOS > nPOS && nPOS != -1))
                    nEnd = nMid - 1;

                nMid = (nStart + nEnd) / 2;
            }
            return false;
        }

        #endregion

        #region FindInModifyTable Method

        //====================================================================
        // Func Name  : FindInModifyTable
        // Description: judge the word and handle exist in the modified table and its items
        // Parameters : nInnerCode: the inner code of the first CHines char
        //              sWord: the word
        //              nHandle:the handle number
        //              *pFindRet: the node found
        // Returns    : success or fail
        //====================================================================
        private bool FindInModifyTable(int nInnerCode, string sWord, int nPOS, out WordChain pFindRet)
        {
            WordChain pCur, pPre;
            if (modifyTable != null)
            {
                pCur = modifyTable[nInnerCode].pWordItemHead;
                pPre = null;
                while (pCur != null && (Utility.CCStringCompare(pCur.data.sWord, sWord) < 0 ||
                   (string.Compare(pCur.data.sWord, sWord, true) == 0 && pCur.data.nPOS < nPOS)))
                //sort the link chain as alphabet
                {
                    pPre = pCur;
                    pCur = pCur.next;
                }

                pFindRet = pPre;

                if (pCur != null && string.Compare(pCur.data.sWord, sWord, true) == 0 && pCur.data.nPOS == nPOS)
                    //The node exists, delete the node and return
                    return true;
                else
                    return false;
            }

            pFindRet = null;
            return false;
        }

        //====================================================================
        // Func Name  : FindInModifyTable
        // Description: judge the word and handle exist in the modified table and its items
        // Parameters : nInnerCode: the inner code of the first CHines char
        //              sWord: the word
        //              nHandle:the handle number
        //              *pFindRet: the node found
        // Returns    : success or fail
        //====================================================================
        private bool FindInModifyTable(int nInnerCode, string sWord, out WordChain pFindRet)
        {
            WordChain pCur, pPre;
            if (modifyTable != null)
            {
                pCur = modifyTable[nInnerCode].pWordItemHead;
                pPre = null;
                while (pCur != null && (Utility.CCStringCompare(pCur.data.sWord, sWord) < 0))
                {
                    pPre = pCur;
                    pCur = pCur.next;
                }

                pFindRet = pPre;

                if (pCur != null && string.Compare(pCur.data.sWord, sWord, true) == 0)
                    return true;
                else
                    return false;
            }

            pFindRet = null;
            return false;
        }

        //====================================================================
        // Func Name  : FindInModifyTable
        // Description: judge the word and handle exist in the modified table and its items
        // Parameters : nInnerCode: the inner code of the first CHines char
        //              sWord: the word
        //              nHandle:the handle number
        // Returns    : success or fail
        //====================================================================
        private bool FindInModifyTable(int nInnerCode, string sWord, int nPOS)
        {
            WordChain pCur, pPre;
            if (modifyTable != null)
            {
                pCur = modifyTable[nInnerCode].pWordItemHead;
                pPre = null;

                //sort the link chain as alphabet
                while (pCur != null && (Utility.CCStringCompare(pCur.data.sWord, sWord) < 0 ||
                   (string.Compare(pCur.data.sWord, sWord, true) == 0 && pCur.data.nPOS < nPOS)))
                {
                    pPre = pCur;
                    pCur = pCur.next;
                }

                //The node exists
                if (pCur != null && string.Compare(pCur.data.sWord, sWord, true) == 0 &&
                    (pCur.data.nPOS == nPOS || nPOS < 0))
                    return true;
            }
            return false;
        }

        #endregion

        #region FindFirstMatchItemInOrgTbl Method

        //====================================================================
        // 查找第一个满足（int nInnerCode, string sWordFunc Name）条件的位置
        //====================================================================
        private bool FindFirstMatchItemInOrgTbl(int nInnerCode, string sWord, out int nPosRet)
        {
            WordItem[] pItems = indexTable[nInnerCode].WordItems;

            int nStart = 0, nEnd = indexTable[nInnerCode].nCount - 1;
            int nMid = (nStart + nEnd) / 2, nCmpValue;

            if (sWord.Length == 0)
            {
                nPosRet = 0;
                return true;
            }

            while (nStart <= nEnd)
            {
                nCmpValue = Utility.CCStringCompare(pItems[nMid].sWord, sWord);
                if (nCmpValue == 0)
                {
                    //Get the first item which match the current word
                    while (nMid >= 0 && pItems[nMid].sWord == sWord)
                        nMid--;

                    nPosRet = ++nMid;
                    return true;
                }
                else if (nCmpValue < 0)
                    nStart = nMid + 1;
                else if (nCmpValue > 0)
                    nEnd = nMid - 1;

                nMid = (nStart + nEnd) / 2;
            }

            nPosRet = -1;
            return false;
        }

        #endregion

        #endregion

    }
}