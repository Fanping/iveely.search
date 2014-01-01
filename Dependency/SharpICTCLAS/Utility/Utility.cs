/***********************************************************************************
 * ICTCLAS¼ò½é£º¼ÆËãËùººÓï´Ê·¨·ÖÎöÏµÍ³ICTCLAS
 *              Institute of Computing Technology, Chinese Lexical Analysis System
 *              ¹¦ÄÜÓÐ£ºÖÐÎÄ·Ö´Ê£»´ÊÐÔ±ê×¢£»Î´µÇÂ¼´ÊÊ¶±ð¡£
 *              ·Ö´ÊÕýÈ·ÂÊ¸ß´ï97.58%(973×¨¼ÒÆÀ²â½á¹û)£¬
 *              Î´µÇÂ¼´ÊÊ¶±ðÕÙ»ØÂÊ¾ù¸ßÓÚ90%£¬ÆäÖÐÖÐ¹úÈËÃûµÄÊ¶±ðÕÙ»ØÂÊ½Ó½ü98%;
 *              ´¦ÀíËÙ¶ÈÎª31.5Kbytes/s¡£
 * Öø×÷È¨£º  Copyright(c)2002-2005ÖÐ¿ÆÔº¼ÆËãËù Ö°ÎñÖø×÷È¨ÈË£ºÕÅ»ªÆ½
 * ×ñÑ­Ð­Òé£º×ÔÈ»ÓïÑÔ´¦Àí¿ª·Å×ÊÔ´Ðí¿ÉÖ¤1.0
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
 *          (zhanghp@software.ict.ac.cn)¡¢
 * 
 *----------------------------------------------------------------------------------
 * 
 * SharpICTCLAS£º.netÆ½Ì¨ÏÂµÄICTCLAS
 *               ÊÇÓÉºÓ±±Àí¹¤´óÑ§¾­¹ÜÑ§ÔºÂÀÕðÓî¸ù¾ÝFree°æICTCLAS¸Ä±à¶ø³É£¬
 *               ²¢¶ÔÔ­ÓÐ´úÂë×öÁË²¿·ÖÖØÐ´Óëµ÷Õû
 * 
 * Email: zhenyulu@163.com
 * Blog: http://www.cnblogs.com/zhenyulu
 * 
 ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpICTCLAS
{
    public sealed class Utility
    {
        private Utility()
        {
        }

        #region GetPOSValue Method

        public static int GetPOSValue(string sPOS)
        {
            char[] c = sPOS.ToCharArray();

            if (c.Length == 1)
                return Convert.ToInt32(c[0]) * 256;
            else if (c.Length == 2)
                return Convert.ToInt32(c[0]) * 256 + Convert.ToInt32(c[1]);
            else
            {
                string s1 = sPOS.Substring(0, sPOS.IndexOf('+'));
                string s2 = sPOS.Substring(sPOS.IndexOf('+') + 1);
                return 100 * GetPOSValue(s1) + Int32.Parse(s2);
            }
        }

        #endregion

        #region GetPOSString Method

        public static string GetPOSString(int nPOS)
        {
            string sPOSRet;
            char c = Convert.ToChar(nPOS % 256);
            string cstr = (c == '\0' ? string.Empty : c.ToString());

            if (nPOS > Convert.ToInt32('a') * 25600)
            {
                if ((nPOS / 100) % 256 != 0)
                    sPOSRet = string.Format("{0}{1}+{2}", Convert.ToChar(nPOS / 25600), Convert.ToChar((nPOS / 100) % 256), cstr);
                else
                    sPOSRet = string.Format("{0}+{1}", Convert.ToChar(nPOS / 25600), cstr);
            }
            else
            {

                if (nPOS > 256)
                    sPOSRet = string.Format("{0}{1}", Convert.ToChar(nPOS / 256), cstr);
                else
                    sPOSRet = string.Format("{0}", cstr);
            }
            return sPOSRet;
        }

        #endregion

        //====================================================================
        // ¸ù¾Ýºº×ÖµÄÁ½¸ö×Ö½Ú·µ»Ø¶ÔÓ¦µÄCC_ID
        //====================================================================
        public static int CC_ID(byte b1, byte b2)
        {
            return (Convert.ToInt32(b1) - 176) * 94 + (Convert.ToInt32(b2) - 161);
        }

        //====================================================================
        // ¸ù¾Ýºº×Ö·µ»Ø¶ÔÓ¦µÄCC_ID
        //====================================================================
        public static int CC_ID(char c)
        {
            byte[] b = Encoding.GetEncoding("gb2312").GetBytes(c.ToString());
            if (b.Length != 2)
                return -1;
            else
                return (Convert.ToInt32(b[0]) - 176) * 94 + (Convert.ToInt32(b[1]) - 161);
        }

        //====================================================================
        // ¸ù¾ÝCC_ID·µ»Ø¶ÔÓ¦µÄºº×Ö
        //====================================================================
        public static char CC_ID2Char(int cc_id)
        {
            if (cc_id < 0 || cc_id > Predefine.CC_NUM)
                return '\0';

            byte[] b = new byte[2];

            b[0] = CC_CHAR1(cc_id);
            b[1] = CC_CHAR2(cc_id);
            return (Encoding.GetEncoding("gb2312").GetChars(b))[0];
        }

        //====================================================================
        // ¸ù¾ÝCC_ID·µ»Ø¶ÔÓ¦ºº×ÖµÄµÚÒ»¸ö×Ö½Ú
        //====================================================================
        public static byte CC_CHAR1(int cc_id)
        {
            return Convert.ToByte(cc_id / 94 + 176);
        }

        //====================================================================
        // ¸ù¾ÝCC_ID·µ»Ø¶ÔÓ¦ºº×ÖµÄµÚ¶þ¸ö×Ö½Ú
        //====================================================================
        public static byte CC_CHAR2(int cc_id)
        {
            return Convert.ToByte(cc_id % 94 + 161);
        }

        //====================================================================
        // ½«×Ö·û´®×ª»»Îª×Ö½ÚÊý×é£¨ÓÃÓÚ½«ºº×ÖÐèÒª²ð·Ö³É2×Ö½Ú£©
        //====================================================================
        public static byte[] String2ByteArray(string s)
        {
            return Encoding.GetEncoding("gb2312").GetBytes(s);
        }

        //====================================================================
        // ½«×Ö½ÚÊý×éÖØÐÂ×ª»»Îª×Ö·û´®
        //====================================================================
        public static string ByteArray2String(byte[] byteArray)
        {
            return Encoding.GetEncoding("gb2312").GetString(byteArray);
        }

        //====================================================================
        // »ñÈ¡×Ö·û´®³¤¶È£¨Ò»¸öºº×Ö°´2¸ö³¤¶ÈËã£©
        //====================================================================
        public static int GetWordLength(string s)
        {
            return String2ByteArray(s).Length;
        }

        //====================================================================
        // Func Name  : charType
        // Description: Judge the type of sChar or (sChar,sChar+1)
        // Parameters : sFilename: the file name for the output CC List
        // Returns    : int : the type of char
        //====================================================================
        public static int charType(char c)
        {
            if (Convert.ToInt32(c) < 128)
            {
                string delimiters = " *!,.?()[]{}+=";
                //×¢ÊÍ£ºÔ­À´³ÌÐòÎª"\042!,.?()[]{}+="£¬"\042"Îª10½øÖÆ42ºÃASC×Ö·û£¬Îª*
                if (delimiters.IndexOf(c) >= 0)
                    return Predefine.CT_DELIMITER;
                return Predefine.CT_SINGLE;
            }

            byte[] byteArray = Encoding.GetEncoding("gb2312").GetBytes(c.ToString());

            if (byteArray.Length != 2)
                return Predefine.CT_OTHER;

            int b1 = Convert.ToInt32(byteArray[0]);
            int b2 = Convert.ToInt32(byteArray[1]);

            return DoubleByteCharType(b1, b2);
        }

        private static int DoubleByteCharType(int b1, int b2)
        {
            //-------------------------------------------------------
            /*
               code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
               A2A0     ¢¡ ¢¢ ¢£ ¢¤ ¢¥ ¢¦ ¢§ ¢¨ ¢© ¢ª ¢« ¢¬ ¢­ ¢® ¢¯
               A2B0  ¢° ¢± ¢² ¢³ ¢´ ¢µ ¢¶ ¢· ¢¸ ¢¹ ¢º ¢» ¢¼ ¢½ ¢¾ ¢¿
               A2C0  ¢À ¢Á ¢Â ¢Ã ¢Ä ¢Å ¢Æ ¢Ç ¢È ¢É ¢Ê ¢Ë ¢Ì ¢Í ¢Î ¢Ï
               A2D0  ¢Ð ¢Ñ ¢Ò ¢Ó ¢Ô ¢Õ ¢Ö ¢× ¢Ø ¢Ù ¢Ú ¢Û ¢Ü ¢Ý ¢Þ ¢ß
               A2E0  ¢à ¢á ¢â ¢ã ¢ä ¢å ¢æ ¢ç ¢è ¢é ¢ê ¢ë ¢ì ¢í ¢î ¢ï
               A2F0  ¢ð ¢ñ ¢ò ¢ó ¢ô ¢õ ¢ö ¢÷ ¢ø ¢ù ¢ú ¢û ¢ü ¢ý ¢þ   
             */
            if (b1 == 162)
                return Predefine.CT_INDEX;

            //-------------------------------------------------------
            //£° £± £² £³ £´ £µ £¶ £· £¸ £¹
            else if (b1 == 163 && b2 > 175 && b2 < 186)
                return Predefine.CT_NUM;

            //-------------------------------------------------------
            //£Á£Â£Ã£Ä£Å£Æ£Ç£È£É£Ê£Ë£Ì£Í£Î£Ï£Ð£Ñ£Ò£Ó£Ô£Õ£Ö£×£Ø£Ù£Ú
            //£á£â£ã£ä£å£æ£ç£è£é£ê£ë£ì£í£î£ï£ð£ñ£ò£ó£ô£õ£ö£÷£ø£ù£ú 
            else if (b1 == 163 && (b2 >= 193 && b2 <= 218 || b2 >= 225 && b2 <= 250))
                return Predefine.CT_LETTER;

            //-------------------------------------------------------
            /*
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A1A0     ¡¡ ¡¢ ¡£ ¡¤ ¡¥ ¡¦ ¡§ ¡¨ ¡© ¡ª ¡« ¡¬ ¡­ ¡® ¡¯
              A1B0  ¡° ¡± ¡² ¡³ ¡´ ¡µ ¡¶ ¡· ¡¸ ¡¹ ¡º ¡» ¡¼ ¡½ ¡¾ ¡¿
              A1C0  ¡À ¡Á ¡Â ¡Ã ¡Ä ¡Å ¡Æ ¡Ç ¡È ¡É ¡Ê ¡Ë ¡Ì ¡Í ¡Î ¡Ï
              A1D0  ¡Ð ¡Ñ ¡Ò ¡Ó ¡Ô ¡Õ ¡Ö ¡× ¡Ø ¡Ù ¡Ú ¡Û ¡Ü ¡Ý ¡Þ ¡ß
              A1E0  ¡à ¡á ¡â ¡ã ¡ä ¡å ¡æ ¡ç ¡è ¡é ¡ê ¡ë ¡ì ¡í ¡î ¡ï
              A1F0  ¡ð ¡ñ ¡ò ¡ó ¡ô ¡õ ¡ö ¡÷ ¡ø ¡ù ¡ú ¡û ¡ü ¡ý ¡þ   
              ÒÔÏÂ³ýÁË×ÖÄ¸ºÍÊý×ÖµÄ²¿·Ö
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A3A0     £¡ £¢ ££ £¤ £¥ £¦ £§ £¨ £© £ª £« £¬ £­ £® £¯
              A3B0                                £º £» £¼ £½ £¾ £¿
              A3C0  £À 
              A3D0                                   £Û £Ü £Ý £Þ £ß
              A3E0  £à 
              A3F0                                   £û £ü £ý £þ 
             */
            else if (b1 == 161 || b1 == 163)
                return Predefine.CT_DELIMITER;


            else if (b1 >= 176 && b1 <= 247)
                return Predefine.CT_CHINESE;


            else
                return Predefine.CT_OTHER;
        }

        //====================================================================
        // Func Name  : IsAllSingleByte
        // Description: Judge the string is all made up of Single Byte Char
        // Parameters : sSentence: the original sentence which includes Chinese or Non-Chinese char
        // Returns    : the end of the sub-sentence
        //====================================================================
        public static bool IsAllChinese(string sString)
        {
            byte[] byteArray = String2ByteArray(sString);

            int nLen = byteArray.Length, i = 0;

            while (i < nLen - 1 && Convert.ToInt32(byteArray[i]) < 248 && Convert.ToInt32(byteArray[i]) > 175)
            {
                i += 2;
            }
            if (i < nLen)
                return false;
            return true;
        }

        //====================================================================
        //Judge the string is all made up of Num Char
        //====================================================================
        public static bool IsAllNum(string sString)
        {
            return Regex.IsMatch(sString, @"^[¡À+£­\-£«]?[£°£±£²£³£´£µ£¶£·£¸£¹\d]*[¡Ã¡¤£®£¯./]?[£°£±£²£³£´£µ£¶£·£¸£¹\d]*[°ÙÇ§ÍòÒÚ°ÛÇª£¥¡ë%]?$");
        }

        //====================================================================
        //Decide whether the word is Chinese Num word
        //====================================================================
        public static bool IsAllChineseNum(string sWord)
        {
            //°Ù·ÖÖ®ÎåµãÁùµÄÈËÔçÉÏ°ËµãÊ®°Ë·ÖÆð´²
            return Regex.IsMatch(sWord, @"^[¼¸ÊýµÚÉÏ³É]?[Áã¡ðÒ»¶þÁ½ÈýËÄÎåÁùÆß°Ë¾ÅÊ®Ø¥°ÙÇ§ÍòÒÚÒ¼·¡ÈþËÁÎéÂ½Æâ°Æ¾ÁÊ°°ÛÇª¡Ã¡¤£®£¯µã]*[·ÖÖ®]?[Áã¡ðÒ»¶þÁ½ÈýËÄÎåÁùÆß°Ë¾ÅÊ®Ø¥°ÙÇ§ÍòÒÚÒ¼·¡ÈþËÁÎéÂ½Æâ°Æ¾ÁÊ°°ÛÇª]*$");
        }

        //====================================================================
        //Binary search a value in a table which len is nTableLen
        //====================================================================
        public static int BinarySearch(int nVal, int[] nTable)
        {
            int nStart = 0, nEnd = nTable.Length - 1, nMid = (nStart + nEnd) / 2;

            while (nStart <= nEnd)
            {
                if (nTable[nMid] == nVal)
                    return nMid;
                else if (nTable[nMid] < nVal)
                    nStart = nMid + 1;
                else
                    nEnd = nMid - 1;
                nMid = (nStart + nEnd) / 2;
            }

            return -1;
        }

        //====================================================================
        //Judge the string is all made up of Letter Char
        //====================================================================
        public static bool IsAllLetter(string sString)
        {
            char[] charArray = sString.ToCharArray();
            foreach (char c in charArray)
                if (charType(c) != Predefine.CT_LETTER)
                    return false;

            return true;
        }

        //====================================================================
        //Decide whether the word is all  non-foreign translation
        //====================================================================
        public static int GetForeignCharCount(string sWord)
        {
            int nForeignCount, nCount;
            nForeignCount = GetCharCount(Predefine.TRANS_ENGLISH, sWord); //English char counnts
            nCount = GetCharCount(Predefine.TRANS_JAPANESE, sWord); //Japan char counnts
            if (nForeignCount <= nCount)
                nForeignCount = nCount;
            nCount = GetCharCount(Predefine.TRANS_RUSSIAN, sWord); //Russian char counnts
            if (nForeignCount <= nCount)
                nForeignCount = nCount;
            return nForeignCount;
        }

        //====================================================================
        //Get the count of char which is in sWord and in sCharSet
        //====================================================================
        public static int GetCharCount(string sCharSet, string sWord)
        {
            int nCount = 0;
            char[] charArray = sWord.ToCharArray();
            foreach (char c in charArray)
                if (sCharSet.IndexOf(c) != -1)
                    nCount++;

            return nCount;
        }

        //====================================================================
        // °´ÕÕCC_IDµÄ´óÐ¡±È½ÏÁ½¸ö×Ö·û´®£¬ÀýÈç ³¬£­¡°Éù < Éú < ÏÖ¡±
        //====================================================================
        public static int CCStringCompare(string str1, string str2)
        {
            char[] ca1 = str1.ToCharArray();
            char[] ca2 = str2.ToCharArray();

            int minLength = Math.Min(ca1.Length, ca2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (Convert.ToInt32(ca1[i]) < 128 && Convert.ToInt32(ca2[i]) < 128) //Èç¹ûÁ½¸ö×Ö·û¶¼ÊÇ°ë½Ç
                {
                    if (ca1[i] < ca2[i])
                        return -1;
                    else if (ca1[i] > ca2[i])
                        return 1;
                }
                else if (Convert.ToInt32(ca1[i]) < 128)
                    return -1;
                else if (Convert.ToInt32(ca2[i]) < 128)
                    return 1;
                else //Á½¸ö×Ö·ûÈ«²¿ÊÇÈ«½Ç
                {
                    if (CC_ID(ca1[i]) < CC_ID(ca2[i]))
                        return -1;
                    if (CC_ID(ca1[i]) > CC_ID(ca2[i]))
                        return 1;
                }
            }

            if (ca1.Length > ca2.Length)
                return 1;
            else if (ca1.Length == ca2.Length)
                return 0;
            else
                return -1;
        }

        //====================================================================
        //Generate the GB2312 List file
        //====================================================================
        public static bool GB2312_Generate(string sFileName)
        {
            bool isSuccess = true;
            byte[] b = new byte[2];
            FileStream outputFile = null;
            StreamWriter writer = null;

            try
            {
                outputFile = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new StreamWriter(outputFile, Encoding.GetEncoding("gb2312"));

                for (int i = 161; i < 255; i++)
                    for (int j = 161; j < 255; j++)
                    {
                        b[0] = Convert.ToByte(i);
                        b[1] = Convert.ToByte(j);
                        writer.WriteLine(string.Format("{0}, {1}, {2}", ByteArray2String(b), i, j));
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

        //====================================================================
        //Generate the Chinese Char List file
        //====================================================================
        public static bool CC_Generate(string sFileName)
        {
            bool isSuccess = true;
            byte[] b = new byte[2];
            FileStream outputFile = null;
            StreamWriter writer = null;

            try
            {
                outputFile = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new StreamWriter(outputFile, Encoding.GetEncoding("gb2312"));

                for (int i = 176; i < 255; i++)
                    for (int j = 161; j < 255; j++)
                    {
                        b[0] = Convert.ToByte(i);
                        b[1] = Convert.ToByte(j);
                        writer.WriteLine(string.Format("{0}, {1}, {2}", ByteArray2String(b), i, j));
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

        #region È«½Ç°ë½Ç×ª»»

        /*==========================================
       * ÏÂÃæÁ½¸ö·½·¨£º
       * Author : Ú÷Î°¿Æ(kwklover)
       * Home   : http://www.cnblogs.com/kwklover
       *==========================================*/
        /// <summary>
        /// °Ñ×Ö·û×ª»»ÎªÈ«½ÇµÄ(°ë½Ç×ªÈ«½Ç)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ConvertToQJC(char c)
        {
            if (c == 32)
                return (char)12288;

            if (c < 127)
                return (char)(c + 65248);

            return c;
        }

        /// <summary>
        /// °Ñ×Ö·û×ª»»Îª°ë½ÇµÄ(È«½Ç×ª°ë½Ç)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ConvertToBJC(char c)
        {
            if (c == 12288)
                return (char)32;

            if (c > 65280 && c < 65375)
                return (char)(c - 65248);

            return c;
        }

        /*=============================================================================
         *ÏÂÃæÁ½¸ö·½·¨from http://hardrock.cnblogs.com/archive/2005/09/27/245255.html
         ==============================================================================*/
        /// <summary>
        /// ×ªÈ«½ÇµÄº¯Êý(SBC case)
        /// </summary>
        /// <param name="input">ÈÎÒâ×Ö·û´®</param>
        /// <returns>È«½Ç×Ö·û´®</returns>
        ///<remarks>
        ///È«½Ç¿Õ¸ñÎª12288£¬°ë½Ç¿Õ¸ñÎª32
        ///ÆäËû×Ö·û°ë½Ç(33-126)ÓëÈ«½Ç(65281-65374)µÄ¶ÔÓ¦¹ØÏµÊÇ£º¾ùÏà²î65248
        ///</remarks>        
        public static string ToSBC(string input)
        {
            //°ë½Ç×ªÈ«½Ç£º
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }


        /// <summary>
        /// ×ª°ë½ÇµÄº¯Êý(DBC case)
        /// </summary>
        /// <param name="input">ÈÎÒâ×Ö·û´®</param>
        /// <returns>°ë½Ç×Ö·û´®</returns>
        ///<remarks>
        ///È«½Ç¿Õ¸ñÎª12288£¬°ë½Ç¿Õ¸ñÎª32
        ///ÆäËû×Ö·û°ë½Ç(33-126)ÓëÈ«½Ç(65281-65374)µÄ¶ÔÓ¦¹ØÏµÊÇ£º¾ùÏà²î65248
        ///</remarks>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        #endregion

        public static string Traditional2Simplified(string s)
        {
            StringBuilder sb = new StringBuilder();

            string simp = "ï¹°¨°ª°­°®àÈæÈè¨êÓö°ÚÏï§ðÆ°¹°À°ÂæÁæñ÷¡°Ó°ÕîÙ°Ú°ÜßÂ°ä°ì°íîÓ°ï°ó°÷°ù°þ±¥±¦±¨±«ð±öµ±²±´±µ±·±¸±¹ðÇêÚï¼±Á±Ê±Ï±Ð±Ò±ÕÜêßÙääîéóÙõÏ±ß±à±á±ä±ç±èÜÐçÂóÖ±êæôì©ì­ïÚïð÷§±î±ð±ñ±ô±õ±ö±÷ÙÏçÍéÄéëë÷ïÙ÷Æ÷Þ±ýÙ÷²¦²§²¬²µâÄîàð¾²¹îß²Æ²Î²Ï²Ð²Ñ²Ò²Óæî÷õ²Ô²Õ²Ö²×²Þ²à²á²ââü²ã²ïïÊÙ­îÎ²ó²ô²õ²ö²÷²ø²ù²ú²û²üÙæÚÆÚßÝÛâãæ¿æöêèìøïâ³¡³¢³¤³¥³¦³§³©ØöÜÉâêãÑöð³®³µ³¹íº³¾³Â³ÄØ÷ÚÈé´í×ö³³Å³Æ³Í³Ï³ÒèÇèßîñîõ³Õ³Ù³Û³Ü³Ý³ãâÁð·³å³å³æ³èï¥³ë³ì³ï³ñÙ±àüöÅ³÷³ø³ú³û´¡´¢´¥´¦Û»ç©õé´«îË´¯´³´´âë´¸ç¶´¿ðÈ´Âê¡öº´Ç´Ê´ÍðË´Ï´Ð´Ñ´Ó´ÔÜÊæõèÈ´Õê£´Ú´Üß¥´íï±õº´ïßÕ÷²´ø´ûææçªµ£µ¥µ¦µ§µ¨µ¬µ®µ¯ééêæð÷óìµ±µ²µ³µ´µµÚÔí¸ñÉµ·µºµ»µ¼µÁìâµÆµËïëµÐµÓµÝµÞÙáÚ®ÚÐç°êëïáµßµãµæµçáÛîäñ²µöµ÷ï¢öôµýµþöø¶¤¶¥¶§¶©îú¶ªîû¶«¶¯¶°¶³á´ð´ñ¼¶¿¶À¶Á¶Ä¶ÆäÂèüë¹óÆ÷ò¶Í¶Ï¶Ðóý¶Ò¶Ó¶Ôí¡ïæ¶Ö¶Ù¶ÛìÀõ»¶á¶éîì¶ì¶î¶ï¶ñ¶öÚÌÛÑãÕéîï°ïÉðÊò¦ò§öùÚÀ¶ù¶û¶ü·¡åÇîïð¹öÜ·¢·£·§·©·¯·°·³···¹·Ã·ÄîÕöÐ·É·Ì·Ï·Ñç³ïÐöî·×·Ø·Ü·ß·àÙÇ·á·ã·æ·ç·è·ë·ì·í·ïãã·ô·ø¸§¸¨¸³¸´¸º¸¼¸¾¸¿Ùìæâç¦ç¨êçôïöÖöûîÅ¸Ã¸Æ¸Çêà¸Ë¸Ï¸Ñ¸ÓÞÏß¦ç¤¸Ô¸Õ¸Ö¸Ù¸Úí°¸äØºÚ¾çÉï¯¸é¸ë¸ó¸õ¸öæüïÓò£¸øØ¨âÙç®öá¹¨¹¬¹®¹±¹³¹µ¹¶¹¹¹º¹»Ú¸çÃêí¹Æ¹ËÚ¬ì±îÜïÀð³ðÀ÷½¹Ð¹Òð»Þâ¹Ø¹Û¹Ý¹ß¹áÚ´ÞèðÙ÷¤¹ãáî¹æ¹é¹ê¹ë¹ì¹î¹ó¹ôØÐØÛæ£èíöÙ÷¬¹õ¹öÙòçµöç¹ø¹ú¹ýÛößÃàþé¤òåîþº§º«ººãÛç¬ò¡ºÅå°ò«ºÒº×ºØÚ­ãØòÃºáºäºèºìÙäÚ§Ý¦ãÈö×ºø»¤»¦»§ä°ðÉ»©»ª»­»®»°æèèëîü»³»µ»¶»·»¹»º»»»½»¾»À»ÁÛ¼çÙïÌöé»Æ»Ñöü»Ó»Ô»Ù»ß»à»á»â»ã»ä»å»æÚ¶ÜößÜä«çÀçõêÍ»ç»ëÚ»âÆãÔ»ñ»õ»öîØïì»÷»ú»ý¼¢¼£¼¥¼¦¼¨¼©¼«¼­¼¶¼·¼¸¼»¼Á¼Ã¼Æ¼Ç¼Ê¼Ì¼ÍÚ¦ÚµÜùß´ßâæ÷çáêéì´í¶î¿ò²õÒö«öÝöê¼Ð¼Ô¼Õ¼Ö¼Ø¼Û¼ÝÛ£ä¤îòïØòÍ¼ß¼à¼á¼ã¼ä¼è¼ê¼ë¼ì¼î¼ï¼ð¼ñ¼ò¼ó¼õ¼ö¼÷¼ø¼ù¼ú¼û¼ü½¢½£½¤½¥½¦½§ÚÉçÌê§ê¯íúðÏóÈöä÷µ½«½¬½¯½°½±½²½´ç­çÖ½º½½½¾½¿½Á½Â½Ã½Ä½Å½È½É½Ê½Î½ÏÞØá½ðÔöÞ½×½Ú½à½á½ë½ìðÜò¢öÚ½ô½õ½ö½÷½ø½ú½ý¾¡¾¢¾£¾¥ÚáÝ£âËçÆêáêî¾¨¾ª¾­¾±¾²¾µ¾¶¾·¾º¾»ØÙãþåÉåòëÖö¦¾À¾Ç¾ÉãÎð¯ðÕ¾Ô¾Ù¾Ý¾â¾å¾çÚªåðé·ì«îÒï¸ñÀö´¾é¾îïÃïÔöÁ¾õ¾ö¾øÚÜçå¾û¾ü¿¥ñä¿ª¿­ØÜÛîâéâýîøïÇíèãÊîÖîí¿Å¿Ç¿Îæìç¼éðîÝï¾ò¥¿Ñ¿Òö¸ï¬¿Ù¿â¿ãà·¿é¿ëÛ¦ßàëÚ¿íáö÷Å¿ó¿õ¿öÚ²Ú¿Ú÷ÛÛæþêÜ¿÷¿ù¿úÀ¡À£ØÑÝÞã´ñùóñãÍï¿öïÀ©À«òÓÀ¯À°À³À´ÀµáÁáâäµäþêãíùïªñ®ô¥À¶À¸À¹ÀºÀ»À¼À½À¾À¿ÀÀÀÁÀÂÀÃÀÄá°é­ìµïçñÜÀÅãÏï¶ÀÌÀÍÀÔßëáÀîîï©ðìÀÖ÷¦ÀØÀÝÀàÀáÚ³çÐÀéÀêÀëÀðÀñÀöÀ÷ÀøÀùÀúÁ¤Á¥Ù³ÛªÛÞÜÂÝ°Ýñß¿åÎæêçÊèÀèÝéöíÂï®ð¿ðÝôÏõÈö¨öâ÷¯Á©ÁªÁ«Á¬Á­Á¯Á°Á±Á²Á³Á´ÁµÁ¶Á·ÝüÞÆäòçöéçñÍñÏöãÁ¸Á¹Á½Á¾ÁÂ÷ËÁÆÁÉÁÍçÔîÉðÓÁÔÁÙÁÚÁÛÁÝÁÞÝþâÞéÝê¥õïÁäÁåÁéÁëÁìç±èùòÉöìÁóÁõä¯æòç¸ïÖðÒÁúÁûÁüÁýÂ¢Â£Â¤Ü×ãñççèÐëÊíÃÂ¥Â¦Â§Â¨ÙÍÝäà¶áÐïÎðüñïò÷÷ÃÂ«Â¬Â­Â®Â¯Â°Â±Â²Â³Â¸Â»Â¼Â½Ûäß£ààãÌãòäËèÓéÖéñéûê¤ëªëÍðµðØôµöÔÂÍÂÎÂÏÂÐÂÒÙõæ®èïð½öÇÂÕÂÖÂ×ÂØÂÙÂÚÂÛàðÂÜÂÞÂßÂàÂáÂâÂæÂçÜýâ¤ãøé¡ëáïÝÂ¿ÂÀÂÁÂÂÂÅÂÆÂÇÂËÂÌéµñÚï²ß¼ÂèÂêÂëÂìÂíÂîÂðßéæÖè¿ÂòÂóÂôÂõÂöÛ½Â÷ÂøÂùÂúÃ¡çÏïÜòª÷©Ã¨ÃªÃ­Ã³÷áÃ»Ã¾ÃÅÃÆÃÇÞÑìËí¯îÍÃÌÃÎÃÐÃÕÃÖÃÙÃÝØÂÚ×â¨ìòÃàÃåäÅëïö¼Ãíç¿çÑÃðÃõÃöãÉçÅÃùÃúÃýÚÓÝëâÉéâïÒÄ±Ä¶îâÄÅÄÆÄÉÄÑÄÓÄÔÄÕÄÖîóÚ«ÄÙÄÚÄâÄåîêöòÄìéýöóÄðÄñÜàôÁÄôÄöÄ÷ÄøÚíÞÁà¿ò©õæÄûÄüÄþÅ¡Å¢ÜÑßÌñ÷Å¥Å¦Å§Å¨Å©Ù¯ßææåîÏÅµÙÐÅ±Å·Å¸Å¹Å»Å½Ú©âæê±ÅÌõçÅÓÅ×ðåÅâàÎÅçÅôç¢î¼îëÆ­ÚÒæéÆ®çÎÆµÆ¶æÉÆ»Æ¾ÆÀÆÃÆÄîÇÆËÆÌÆÓÆ×ïäïèÆÜÆêÆëÆïÆñÆôÆøÆúÆýÞ­æëç²èçíÓñýñþ÷¢Ç£Ç¥Ç¦Ç¨Ç©Ç«Ç®Ç¯Ç±Ç³Ç´ÇµÙÝÝ¡ã¥å¹ç×èýîÔÇ¹ÇºÇ½Ç¾Ç¿ÇÀæÍéÉê¨ìÁïºïÏïêôÇõÄÇÂÇÅÇÇÇÈÇÌÇÏÚ½ÚÛÜñçØíÍõÎÇÔã«ïÆóæÇÕÇ×ÇÞï·ÇáÇâÇãÇêÇëÇìÞìöëÇíÇîÜäòÌÛÏêäò±öúÇ÷ÇøÇûÇýÈ£Ú°á«ãÖêïð¶È§È¨È°Ú¹ç¹éúîýÈ´ÈµÈ·ã×ãÚí¨ÈÃÈÄÈÅÈÆÜéæ¬èãÈÈÈÍÈÏÈÒâ¿éíÈÙÈÞáÉòîçÈï¨ò­ÈíÈñò¹ÈòÈóÈ÷ÈøìªÈúÈüÉ¡ë§ôÖÉ¥É§É¨çÒÉ¬ØÄï¤ð£É±É²É´ï¡öèÉ¸É¹õ§É¾ÉÁÉÂÉÄÉÉÚ¨æ©æóîÌ÷­ÉÊÉËÉÍÛðéäõüÉÕÉÜÉÞÉãÉåÉèØÇäÜî´ÉðÉóÉôÉöÉøÚ·ÚÅäÉÉùÉþÊ¤Ê¦Ê¨ÊªÊ«Ê±Ê´ÊµÊ¶Ê»ÊÆÊÊÊÍÊÎÊÓÊÔÚÖÛõÝªß±éøêÛîæöåÊÙÊÞç·ÊàÊäÊéÊêÊôÊõÊ÷ÊúÊýÞóç£Ë§ãÅË«Ë­Ë°Ë³ËµË¶Ë¸îåË¿ËÇØËæáçÁïÈð¸ËÊËËËÌËÏËÐËÓÞ´âÈì¬ïËËÕËßËàÚÕöÕËäËæËçËêÚÇËïËðËñÝ¥áøËõËöËøßïíüÌ¡Ì¢ãËîè÷£Ì¨Ì¬îÑöØÌ¯Ì°Ì±Ì²Ì³Ì·Ì¸Ì¾ê¼îãïÄñüÌÀÌÌÙÎâ¼ï¦ïÛÌÎÌÐÌÖèºï«ÌÚÌÜÌàÌâÌåÌëç¾ðÃãÙÌõôÐö¶öæÌùÌúÌüÌýÌþÍ­Í³âúÍ·î×ÍºÍ¼îÊÍÅÞÒÍÇÍÉâ½ÍÑÍÒÍÔÍÕÍÖóêö¾Íàæ´ëðÍäÍåÍçÍòæýçºÍøéþÎ¤Î¥Î§ÎªÎ«Î¬Î­Î°Î±Î³Î½ÎÀÚÃàøãÇãíä¶çâè¸ì¿öÛÎÂÎÅÎÆÎÈÎÊãÓÎÍÎÎÎÏÎÐÎÑÎÔÝ«ö»ÎØÎÙÎÚÎÜÎÞÎßÎâÎëÎíÎñÎóÚùâÐâäåüæððÄðÍÎýÎþÏ®Ï°Ï³Ï·Ï¸â¾ãÒçôêêÏºÏ½Ï¿ÏÀÏÁÏÃÏÅíÌÏÊÏËÏÍÏÎÏÐÏÔÏÕÏÖÏ×ÏØÏÚÏÛÏÜÏßÜÈÝ²Þºá­áýæµðÂðïòºôÌõÑÏáÏâÏçÏêÏìÏîÜ¼âÃæøç½÷ÏÏôÏùÏúÏþÐ¥ßØäìæçç¯èÉóïÐ­Ð®Ð¯Ð²Ð³Ð´ÐºÐ»Ùôß¢ç¥çÓÐ¿ÐÆÐËÚêÜþÐ×ÐÚÐâÐåâÊð¼ÐéÐêÐëÐíÐðÐ÷ÐøÚ¼çïÐùÐüÑ¡Ñ¢Ñ¤ÚÎîçïàÑ§ÚÊí´÷¨Ñ«Ñ¯Ñ°Ñ±ÑµÑ¶Ñ·Û÷ä±öàÑ¹Ñ»Ñ¼ÑÆÑÇÑÈÛëæ«èâë²ÑËÑÌÑÎÑÏÑÒÑÕÑÖÑÞÑáÑâÑåÑèÑéØÉØÍÙ²ÙðÚÝâûãÆõ¦÷Ê÷Ð÷úÑìÑîÑïÑñÑôÑ÷ÑøÑùì¾ÑþÒ¡Ò¢Ò£Ò¤Ò¥Ò©é÷ðÎ÷¥Ò¯Ò³ÒµÒ¶ØÌÚËÚþêÊìÇÒ½Ò¿ÒÃÒÅÒÇÒÏÒÕÒÚÒäÒåÒèÒéÒêÒëÒìÒïÚ±ß½á»âÂâøæäçËéóêÝîÆï×ïîðùô¯ÒñÒõÒøÒûÒþî÷ñ«Ó£Ó¤Ó¥Ó¦Ó§Ó¨Ó©ÓªÓ«Ó¬Ó®Ó±ÜãÝºÝÓÝöÞüàÓäÞäëè¬ðÐñ¨ò¤ó¿Ó´ÓµÓ¶Ó¸Ó»Ó½ïÞÓÅÓÇÓÊÓËÓÌÓÕÝµîðöÏÓßÓãÓæÓéÓëÓìÓïÓüÓþÔ¤Ô¦ØñÙ¶ÚÄÚÍÝ÷áÎâÀãÐåýæúêìì£îÚðÁðÖö¹Ô§Ô¨Ô¯Ô°Ô±Ô²ÔµÔ¶éÚð°ö½Ô¼Ô¾Ô¿ÔÁÔÃÔÄîáÔÇÔÈÔÉÔËÔÌÔÍÔÎÔÏÛ©Ü¿ã¢ã³ç¡è¹éæëµÔÓÔÖÔØÔÜÔÝÔÞè¶ôõöÉÔßÔàæàÔäÔæÔðÔñÔòÔóØÓßõàýóåÔôÚÚÔù×ÛçÕÔþÕ¡Õ¢Õ¤Õ©Õ«Õ®Õ±ÕµÕ¶Õ·Õ¸Õ»Õ½ÕÀÚÞÕÅÕÇÕÊÕËÕÍÕÔÚ¯îÈÕÝÕÞÕàÕâÚØéüðÑÕêÕëÕìÕïÕòÕóä¥çÇèåéôêâìõð²ÕõÕöÕøÕùÖ¡Ö¢Ö£Ö¤Úºá¿îÛï£óÝÖ¯Ö°Ö´Ö½Ö¿ÖÀÖÄÖÊÖÍæïèÎèÙéòéùêÞðºòÏôêõÙõÜö£ÖÓÖÕÖÖÖ×ÖÚïñÖßÖáÖåÖçÖèæûç§ÖíÖîÖïÖòÖõÖöÖüÖý×¤ØùéÆîù×¨×©×ª×¬ßùâÍò¨×®×¯×°×±×³×´×¶×¸×¹×ºæíçÄ×»×¼×Å×ÇÚÂïí×È×Ê×ÕÚÑç»ê¢êßíöïÅö·öö×Ù×Ü×ÝÙÌ×ÞÚÁæãöí×ç×éïß×êçÚõò÷®°¿²¢²·³Á³óµíµü¶··¶¸É¸Þ¹è¹ñºó»ï½Õ½Ü¾÷¿äÀïÁèÃ´Ã¹ÄíÆàÇ¤Ê¥Ê¬Ì§Í¿ÍÝÎ¹ÎÛÏÇÏÌÐ«ÒÍÓ¿ÓÎÓõÓùÔ¸ÔÀÔÆÔîÔúÔýÖþÓÚÖ¾×¢µòÚ¥ÚÙÛ§ÛÂÛÊÛàÛâÛñÛûÛþÜÜÝ¤Ý§Ý¯Ý»ÝÔÞ»Þêß¸ßÄßÇßÐßÔàÙàèàëá¥á®áÕáÝáèáïáóâÅâÇâÌâÎã¶ãÀãÁãÜäÓäÙäãäíäóå£å¸æùç«ç´çëèÅèðèñéÀéÍéïéõêåëÉëËì®ìÎìÑìÖíªíµí¿íÞíîîÐîÞîôîöï­ï³ï´ïµï»ï½ïÁïÂïÍïÑïÕïãïåïéïïïùðÅðÌð×ðßðâðéñ³ñÐñßñìò¬òýôðöÑöÒöÓößöñöõö÷öýöþ÷ª÷«÷³÷¹÷þ";
            string trad = "åH°}Ì@µKÛ‡†‹Ü­a•áì\ÖOä@ùgóaÒ\ŠW‹‹òˆö—‰ÎÁTâZ”[”¡†hîCÞk½OâkŽÍ½‰æ^Ör„ƒï–ŒšˆóõUødý_Ý…Øä^ªN‚ä‘vùlÙSåQ¿‡¹P®…”ÀŽÅé]Éœ†ô§ãGº`Û‹ß…¾ŽÙH×ƒÞqÞpÆS¾œ»e˜ËòŠïRïjçSès÷B÷M„e°TžlžIÙe”Pƒ†À_™‰š›Äœè\óxôWïž·A“ÜÀãKñgðGâ“ùPÑaâ˜Ø”…¢ÐQšˆ‘M‘K Nò‰üoÉnÅ“‚}œæŽú‚ÈƒÔœyÅŒÓÔŒåšƒŠâO”v“½Ïsð’×‹ÀpçP®aêUî‡ÏÕ~×Êr‘Ô‹Èò–Ò—¶Uç†ˆö‡LéLƒ”ÄcS•³‚tÈOé‹öKânÜ‡Ø³Œ‰mêÒr‚áÖR™Â´~ýZ“Î·Q‘ÍÕ\òG—–™fä…èK°VßtñYuýXŸëï†ø|›_ÐnÏxŒ™ã|® ÜP»I¾Iƒ‰ŽÎ×‡™»NäzërµAƒ¦Ó|ÌŽÆc½IÜX‚÷âA¯êJ„“íåN¾E¼ƒù‡¾bÝzýpÞoÔ~Ùnú\Â”Ê[‡èÄ…²Éò‹˜ºœÝÜf¸Z”xåeäSûzß_‡}í^Ž§ÙJñ~½H“ú†Îà“ÛÄ‘‘„ÕQ—š—Ùy°Dº„®”“õühÊŽ™n×•´XÒd“vu¶\Œ§±I cŸôà‡ç‹”³œìßf¾†¼eÔgÖB½Ó]çCîüc‰|ëŠŽpâš°dážÕ{ã“õ Õ™¯Bölá”í”åVÓ†äbGäA–|„Ó—ƒö–ù…¸] Ùªš×xÙ€åƒž^™³ ©ºVütå‘”à¾„»fƒ¶ê Œ¦‘»ç…‡îDâgŸõÜOŠZ‰™èIùZî~ÓžºðIÖ@ˆ×é‘Ü—ä~åŠù˜î€î…÷{ÕOƒº –ðDÙEßƒãsøõb°lÁPéy¬mµ\âCŸ©ØœïˆÔL¼â[ô™ïwÕuUÙM¾pçšöE¼Š‰žŠ^‘¼SƒfØS—÷ähïL¯‚ñT¿pÖSøPž–ÄwÝ—“áÝoÙxÍØ“Ó‡‹D¿`øDñ€¼›½EÙŽûŸõVöváÔ“â}ÉwÙW—UÚs¶’ÚMŒÀ“{½CŒù„‚ä“¾V‘ßæ€²GÕa¿cä†”Røéwãt‚€¼væk}½oƒÙs½Žõ†ýŒmì–Ø•ã^œÏÆˆ˜‹Ù‰òÔ¾—ÓMÐMî™ÔbÝžâ’ådøù]úX„Ž’ìøŽ“êPÓ^ð^‘TØžÔŸ“¥ûXöŠV«EÒŽšwý”é|Ü‰ÔŽÙF„£…Q„¥‹‚™uõq÷ZÝLÐ–¾iõ…å‡øß^ˆå†JŽ½˜¡ÏXãxñ”ínhêR½WîRÌ–ž®î—éuúQÙRÔXêHÏ ™MÞZø™¼tüZÓÈ‡éb÷c‰Ø×oœû‘ôGù–‡WÈA®‹„Ô’ò‘˜åçf‘Ñ‰Äšg­hß€¾“Q†¾¯ˆŸ¨œoŠJÀQæDõŒüSÖeöm“]Ýxš§ÙV·x•þ Z…RÖMÕdÀLÔœËC‡‚ÒÀD¬q•ŸÈœ†ÕŸðQé’«@Ø›µœâ€èZ“ô™C·eð‡ÛE×Iëu¿ƒ¾ƒ˜OÝ‹¼‰”DŽ×ËE„©úÓ‹Ó›ëHÀ^¼oÓ“Ô‘Ëj‡\‡óK­^ÓJýW´‰ÁbÏŠÜQìV÷qöaŠAÇvîaÙZâ›ƒrñ{àP›Ñäeæ‰Ïušž±OˆÔ¹{égÆD¾}ÀO™z‰Aû|’þ“ìº†ƒ€œpË]™‘èbÛ`ÙvÒŠæIÅž„¦ðTužR¾ÖG¿V‘â‘ì²€úY¹aöžídŒ¢{ÊY˜ªª„Öváu½{í\Äz²òœ‹É”‡ãq³CƒeÄ_ïœÀU½gÞIÝ^“×þú„õoëA¹½YÕ]ŒÃ°XîMõ^¾oå\ƒHÖ”ßM•x a±M„ÅÇGÇoŽ„Ë|ð~¿NÚBÓPöLó@½›îiìoçR½¯d¸‚ƒô„q›ÜÞŸ†Ã„ìn¼mŽýÅfôbøFúñxÅe“þä‘Ö„¡ÔnŒÕ™ÎïZâ ä|¸MýeùN½äŸçëhÓX›Q½^×H«kâxÜŠòE°—é_„P„’‰N÷ðæzå|ýé`â‚äDîwš¤ÕnòS¾~ÝVâŽä˜îh‰¨‘©ýlçH“¸ŽìÑ‡¿‰Kƒ~à”‡ˆÄ’Œ’ªœóyµV•ç›rÕEÕNà—‰¿ÀkÙLÌŽh¸Qð¢…TÊ‰‘|Â˜ºˆé€åKöH”UéŸÏ“ÏžÅDÈRíÙ‡ˆÆœZž|Ùl²Aån°]»[Ë{™Ú”r»@ê@Ìmž‘×Ž”ˆÓ[‘ÐÀ| €žE¹™ì”Ìè|Òh¬˜éäZ“Æ„Ú³‡Z÷ã™ç„°A˜·ö˜èD‰¾îœIÕC¿w»hØ‚ëxõŽ¶Yû…–„îµ[švžrë`ƒ«áB‰ÈËžÉWÌy‡³ßŠóP¿r™À™µÞ]µZä‡ûZ°O¼cÜVìZ÷~÷k‚zÂ“ÉßBç ‘ziºŸ”¿Ä˜æœ‘ÙŸ’¾šÌ`ŠYž‡­IššÑžÒcö–¼Z›öƒÉÝvÕôu¯Ÿß|ç‚¿á‘ú«CÅRà÷[„CÙUÌA[™_ÞOÜkýgâì`ŽXîI¾c™ôÏ|öNðs„¢žgòt¾^æyúwýˆÃ@‡µ»\‰Å”në]Ìdž{­‡™É–Vµa˜ÇŠä“§ºtƒEÊV‡DâçU¯›ÂeÏNótÌJ±RïB] t“ïûuÌ”ô”ÙTµ“ä›ê‘‰À”]‡£é‚žoœO™¾™©Þ_Ý`ÞAšÚÅFûRú˜ÆA÷|Žn”Œ\ž´yÅLŒD™èû[èŽ’àÝ†‚öœS¾]Õ“‡÷Ì}Á_ß‰èŒ»jò…ñ˜½j Î«MžT™åÄTæ óH…ÎäX‚HŒÒ¿|‘]žV¾G™°Ò@äs‡`‹Œ¬”´aÎ›ñRÁR†á‡O‹ß˜qÙIûœÙuß~Ã}„ê²mðzÐUMÖ™¿zçNî‹ö Øˆå^ãTÙQüN›]æVéTž‚ƒ’Ð F‘¿å{åi‰ô²[Öi›Ò’ƒçÁdÖk«J¶[¾d¾’ÆìtüwR¾˜¿Šœç‘‘é}éh¾‡øQã‘Ö‡Öƒò‡ðxš{æŸÖ\®€ãf…Èâc¼{ëy“ÏÄXÀô[çtÔGðHƒÈ”MÄâ‰öF”fÝ‚öTá„øBÊ\ÑUÂ™‡§è‡æ‡êŸÌY‡Ëî”Üb™ŽªŸŒŽ”QôÆr‡“Âœâo¼~Ä“âÞrƒz‡ñwâSÖZƒ®¯‘šWútšª‡IaÖŽ‘Y®T±PÛ˜ý‹’°’ÙrÞ\‡Šùi¼„Á`â”ò_Õ›ñ‰ïh¿~îlØš‹åÌO‘{ÔuŠîHá•“ää˜ã×Vçhç’—«ÄšýRòTØM†¢šâ—‰Ó™ÌIòU¾_˜´ƒí î@ö’ ¿âFãUßwºžÖtåXãQ“œ\×l‰qƒLÊn‘aòqÀ`˜ âj˜Œ†Ü‰¦ËNŠ“Œ‹Ô™{‘êŸÍäçIçjÁuÛ„æ@˜ò†ÌƒSÂN¸[ÕV×SÊwÀR´“ÜE¸`Üå›ºDšJÓHŒ‹äuÝpšäƒAí•Õˆ‘c“åõ›­‚¸FŸ¦ÍŽ€ÙgÏlöqÚ……^Ü|òŒýxÔxçé˜ÓUøzïE™à„ñÔ¾JÝbãŒ…sùo´_é êIâ×Œðˆ”_À@Ê‹Æ˜ïŸáígÕJ¼xïƒÜ˜s½qŽVÏ”¿dãœïAÜ›äJÍ˜éc™ž¢Ë_ïSöwÙ‚ãšÐ¼R†Êò}’ß¿‰­†ÝäC·wš¢„x¼†æ|õºY•ñá‡„héWêƒÙ ¿˜Ó˜Š™ò~áŸ÷X‰„‚ûÙpˆsš‘ÓxŸý½BÙd”z‘ØÔO…‡ž—®Œ¼Œ‹ðÄIBÔ–Õ”žcÂ•ÀK„ÙŽŸª{ñÔŠ•rÎgŒ×Rñ‚„ÝßmáŒï—Ò•Ô‡Öu‰PÉPsÝYÙBâ‹öˆ‰Û«F¾R˜ÐÝ”•øÚHŒÙÐg˜äØQ”µ”d¼‚Ž›éVëpÕl¶í˜Õf´T qèp½zï•Pñ†¾ŒæJúƒÂ–‘ZížÔAÕb”\Ë’ðtï`æ}ÌKÔVÃCÖq·dëmëS½—šqÕrŒO“p¹SÉpªs¿s¬æi†îÃ«H“éêYãBöÅ_‘BâõT”‚Ø°cž©‰¯×TÕ„‡@•ÒãgåUí™œ« Cƒ¯ðhç|çMý½{Ó‘íwäˆòvÖ`äRî}ówŒÏ¾ŸùYêD—l¼gýföœÙNèFdÂ ŸNã~½y‘Qî^â^¶dˆDâQˆF“»îjÍ‘ï‚Ã“ørñWñ„™E»XüƒÒm‹zÄež³îBÈf¼w¾U¾WÝyífß`‡úžéžH¾SÈ”‚¥‚Î¾•Ö^ÐlÕ†Ž®éœ¿¬¬|ítŸ˜õnœØÂ„¼y·€†–é”®Y“ëÎœu¸CÅPÈný}†èæužõÕ_ŸoÊ…Ç‰]ìF„ÕÕ`àwT‘“‹³ò\ù^úFåa ÞÒuÁ•ãŠ‘ò¼šðqô]­tÒ ÎrÝ {‚bªMB‡˜³ˆõrÀwÙtã•éeï@ëU¬F«I¿hðWÁw‘—¾€Ç{ËWÌ\sª‹¹ú’°BÏ–¶iÜ]Žûè‚àlÔ”í‘í—ËGðAóJ¾|ð‹Ê’‡ÌäN•Ô‡[‡^žtò”½‹—nº…f’¶”yÃ{ÖCŒ‘žaÖxÒC”X¼œÀiä\á…Ådê€œîƒ´›°äPÀCð}ø Ì“‡uíšÔS”¢¾wÀmÔ‚íœÜŽ‘Òßx°_½kÖXãCæ›ŒWÖoÍ÷L„×ÔƒŒ¤ñZÓ–Óßd‰_¡÷\‰ºøfø††¡†Ó ˆº‹I—¿šåéŽŸŸû}‡ÀŽrîéÆG…’³Ž©ÖVòž…˜ÚIƒ°ƒ¼×—‘ÃéZá‰ô|ðýBø„—î“P¯ƒê–°WðB˜ÓŸ¬¬Ž“uˆòßb¸GÖ{ËŽÝUú_öŽ ”í“˜IÈ~ìvÖ]à’•ÏŸîátãžîUßzƒxÏË‡ƒ|‘›ÁxÔ„×hÕx×g®À[Ôr‡ÒŽFï‘«óA¿OÝWÙOáæ„èO¯ŽÅœÊaêŽãyï‹ë[ãŸ°a™Ñ‹ëú—‘ªÀt¬“Îž IŸÉÏ‰ÚA·f‰LúL¿Mæv”t‡Âž]žu­‹ûW°`îWÀ›†Ñ“í‚ò°bÛxÔçOƒž‘nà]â™ªqÕTÊ~äBôœÝ›ô~OŠÊÅcŽZÕZªz×uîAñS‚ø‚RÕ˜ÖIÊš£ï„é“‹ž¼uÓDšeâ•ùOú–ýrøxœYÞ@ˆ@†TˆA¾‰ßh™´øSüx¼sÜSè€»›‚é†ãXày„òëEß\ÌNáj•žíàiÊ|Á‘C¼‹íyšŒšèësžÄÝd”€•ºÙ­‘ÚŽçYÚEÅKñzè——ØŸ“ñ„tÉÙ‘‡KŽ¾ºjÙ\×PÙ›¾C¿•ÜˆåŽél–ÅÔpýS‚ùšÖ±K”ØÝšä—£‘ð¾`×dˆqŽ¤Ù~Ã›ÚwÔtá“ÏUÞHæNß@Ö†ÝmúpØ‘á˜‚ÉÔ\æ‚ê‡œ¿b˜EÝFÙcµøc’ê± ªb ŽŽ¬°Yà×CÕŠ˜ã`åP¹~¿—ÂšˆÌ¼ˆ“´”SŽÃÙ|œþòs™±—dÝTÝeÙ—úvÎ‡¿{ÜWÜUÓzçŠ½K·NÄ[±ŠæRÖaÝS°™•ƒóE¼q¿UØiÖTÕD T²š‡ÚÙAèTñvÐ™½ãŒ£´uÞDÙ‡Êð‚ïD˜¶ÇfÑbŠy‰Ñ îåFÙ˜‰‹¾YòK¿PÕœÊÖøáÕŽèCÆÙYnÖJ¾lÝwÙD±{åOýbõ™Û™¿‚¿v‚ôàuÕŒò|öOÔ{½Mæ—ã@ÀyÜg÷VÂOKÊNÉòáhÕµþôY¹ ŽÖÅVÎù™™ááâ··M‚ÜÔEÕFÑYœR÷áüq“ÓœD’LÂ}ŒÆ”E‰T¸Dðj›@åvûyÏ¤œ¥ß[»n¶RîŠŽ[ë…¸^¼™„žºBì¶ÕIÔ]µñÓ…×vàSÃÍšëÚæ‰Åˆßˆ‰|™”Ê{È’É‰ÉO¹½éÂ“«ßå†w†U‡z‡j¾ïÅüÖoÒLŽS¼¹·ÂƒeªwûƒðNðlð€ð–Àãâð‘¬ãÝsž¹»ìžEžzµ­ŒŽôé½f¾y¬z—g—¨°¸™R™ÁÝMÜ Ù}ÄdÄLïlºýŸÁïœ¡Ãì´^L²gâbãOäyäHä{äç˜ç™åŸåuåxäžæXæ[æ“çèuè‰èd·„ù‘úBûI°[åí¯{ÄŸÒM¿‹ÂgîžÏ\üDõEõGõRõœöAöX÷aöcö…öš÷IíXíxýO";

            char[] sArray = s.ToCharArray();

            foreach (char c in sArray)
            {
                if (trad.IndexOf(c) >= 0)
                    sb.Append(simp.Substring(trad.IndexOf(c), 1));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /*======= ICTCLAS ÖÐÓÐ£¬µ«´ÓÎ´ÓÃµ½µÄ·½·¨£¬Ä¿Ç°ÉÐ´ýÊµÏÖ ============
      
        //Get the max Prefix string made up of Chinese Char
        public static uint GetCCPrefix(string sSentence)
        {
           return 0;
        }

        //Judge the string is all made up of non-Chinese Char
        public static bool IsAllNonChinese(string sString)
        {
           return true;
        }

        //Judge the string is all made up of Single Byte Char
        public static bool IsAllSingleByte(string sString)
        {
           return true;
        }



        //Judge the string is all made up of Index Num Char
        public static bool IsAllIndex(string sString)
        {
           return true;
        }

        //Judge the string is all made up of Delimiter
        public static bool IsAllDelimiter(string sString)
        {
           return true;
        }

        //sWord maybe is a foreign translation
        public static bool IsForeign(string sWord)
        {
           return true;
        }

        //Decide whether the word is all  foreign translation
        public static bool IsAllForeign(string sWord)
        {
           return true;
        }

        //Return the foreign type 
        public static int GetForeignType(string sWord)
        {
           return 0;
        }

        //Get the postfix
        public static bool PostfixSplit(string sWord, string sWordRet, string sPostfix)
        {
           return true;
        }

        //Judge whether it's a num
        public static bool IsSingleByteDelimiter(char cByteChar)
        {
           return true;
        }

        ================================================================*/

    }
}