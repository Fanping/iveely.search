/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpICTCLAS;

namespace Iveely.Framework.Algorithm.AI.Library
{
    /// <summary>
    /// 疑问句
    /// </summary>
    public class Interrogative
    {
        private Hashtable table = new Hashtable();

        public Interrogative()
        {
            table.Add("v-t", "什么时候");
            table.Add("-tg", "什么时候");
            table.Add("-ns", "什么地方");
            table.Add("-nsf", "什么地方");
            table.Add("ns-n", "谁");
            table.Add("nsf-n", "谁");
            table.Add("v-n", "干什么");
            table.Add("-nr", "谁");
            table.Add("-nr1", "谁");
            table.Add("-nr2", "谁");
            table.Add("-nrj", "谁");
            table.Add("-nrf", "谁");
            table.Add("-nt", "谁");
            table.Add("-nz", "谁");
        }

        /// <summary>
        /// 获取陈述句变成的疑问句
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> GetQuestions(string sentence)
        {
            List<WordResult[]> result = Text.Segment.IctclasSegment.GetInstance().SplitToArray(sentence);
            if (IntegrityCheck(result))
            {
                List<Tuple<string, string>> list = new List<Tuple<string, string>>();

                //时间问题
                Tuple<string, string> timeQuestion = Time(result);
                if (timeQuestion != null)
                {
                    list.Add(timeQuestion);
                }

                //地点问题
                Tuple<string, string> locationQuestion = Location(result);
                if (locationQuestion != null)
                {
                    list.Add(locationQuestion);
                }

                //人物问题
                Tuple<string, string> whomQuestion = Whom(result);
                if (whomQuestion != null)
                {
                    list.Add(whomQuestion);
                }

                //事件问题
                Tuple<string, string> eventQuestion = Event(result);
                if (eventQuestion != null)
                {
                    list.Add(eventQuestion);
                }

                return list;

            }
            return null;
        }

        /// <summary>
        /// 时间疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Tuple<string, string> Time(List<WordResult[]> result)
        {
            HashSet<string> signs = new HashSet<string>();
            signs.Add("t");
            signs.Add("tg");
            HashSet<string> exceptionSigns = new HashSet<string>();
            HashSet<string> continueSigns = new HashSet<string>();
            HashSet<string> beforeExceptionSigns = new HashSet<string>();
            return CommonRegex(result, signs, beforeExceptionSigns, exceptionSigns, continueSigns);
        }

        /// <summary>
        /// 共同规则
        /// </summary>
        /// <param name="result"></param>
        /// <param name="signature">特征标识符</param>
        /// <param name="replaceWords">替换字符串</param>
        /// <returns></returns>
        private Tuple<string, string> CommonRegex(List<WordResult[]> result, HashSet<string> signatures, HashSet<string> beforeExceptionSigns, HashSet<string> nextExceptionSigns, HashSet<string> continueSigns)
        {
            bool isMatch = false;
            string question = string.Empty;
            string answer = string.Empty;
            bool shoudContine = false;
            string lastType = string.Empty;
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 1; j < result[i].Length - 1; j++)
                {
                    string type = Utility.GetPOSString(result[i][j].nPOS).Trim();
                    if ((!isMatch && signatures.Contains(type) && !beforeExceptionSigns.Contains(lastType)) || shoudContine)
                    {
                        if (j < result[i].Length - 2)
                        {
                            string nextType = Utility.GetPOSString(result[i][j + 1].nPOS);
                            if (nextExceptionSigns.Contains(nextType))
                            {
                                break;
                            }
                            if (continueSigns.Contains(nextType))
                            {
                                if (!shoudContine)
                                {
                                    lastType = type;
                                }
                                answer += result[i][j].sWord;
                                shoudContine = true;
                                continue;
                            }
                        }
                        isMatch = true;
                        question += table[lastType + "-" + type].ToString();
                        answer += result[i][j].sWord;
                        shoudContine = false;
                    }
                    else
                    {
                        question += result[i][j].sWord;
                        lastType = type;
                    }

                }
            }
            if (isMatch)
            {
                Tuple<string, string> tuple = new Tuple<string, string>(question, answer);
                return tuple;
            }
            return null;
        }

        /// <summary>
        /// 事件疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Tuple<string, string> Event(List<WordResult[]> result)
        {
            HashSet<string> signs = new HashSet<string>();
            signs.Add("n");
            HashSet<string> nextExceptionSigns = new HashSet<string>();
            HashSet<string> beforeExceptionSigns = new HashSet<string>();
            beforeExceptionSigns.Add("ns");
            beforeExceptionSigns.Add("n");
            beforeExceptionSigns.Add("f");
            beforeExceptionSigns.Add("vn");
            HashSet<string> continueSigns = new HashSet<string>();
            return CommonRegex(result, signs, beforeExceptionSigns, nextExceptionSigns, continueSigns);
        }

        /// <summary>
        /// 地点疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Tuple<string, string> Location(List<WordResult[]> result)
        {
            HashSet<string> signs = new HashSet<string>();
            signs.Add("ns");
            signs.Add("nsf");
            HashSet<string> exceptionSigns = new HashSet<string>();
            HashSet<string> continueSigns = new HashSet<string>();
            continueSigns.Add("n");
            HashSet<string> beforeExceptionSigns = new HashSet<string>();
            return CommonRegex(result, signs, beforeExceptionSigns, exceptionSigns, continueSigns);
        }

        /// <summary>
        /// 人物疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Tuple<string, string> Whom(List<WordResult[]> result)
        {
            HashSet<string> signs = new HashSet<string>();
            signs.Add("nr");
            signs.Add("nr1");
            signs.Add("nr2");
            signs.Add("nrj");
            signs.Add("nrf");
            signs.Add("nt");
            signs.Add("nz");
            HashSet<string> exceptionSigns = new HashSet<string>();
            exceptionSigns.Add("n");
            HashSet<string> continueSigns = new HashSet<string>();
            HashSet<string> beforeExceptionSigns = new HashSet<string>();
            return CommonRegex(result, signs, beforeExceptionSigns, exceptionSigns, continueSigns);
        }

        /// <summary>
        /// 是否有条件构建疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool IntegrityCheck(List<WordResult[]> result)
        {
            List<string> flags = new List<string>();
            //必须有动词
            bool hasV = false;
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 1; j < result[i].Length - 1; j++)
                {
                    string str = Utility.GetPOSString(result[i][j].nPOS);
                    if (!flags.Contains(str))
                        flags.Add(str);
                    if (str.Contains("v"))
                    {
                        hasV = true;
                    }
                    if (str.Contains("r"))
                    {
                        //不能有代词
                        return false;
                    }
                }
            }
            return flags.Count > 2 && hasV;
        }
    }
}
