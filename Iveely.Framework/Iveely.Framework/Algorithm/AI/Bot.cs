/////////////////////////////////////////////////
//文件名:Bot
//描  述:
//创建者:刘凡平(Iveely Liu)
//邮  箱:liufanping@iveely.com
//组  织:Iveely
//年  份:2012/3/27 20:51:37
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Iveely.Framework.Algorithm.AI.Library;
using Iveely.Framework.DataStructure;

namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 应答机器人
    /// </summary>
    public class Bot
    {
        /// <summary>
        /// 知识目录
        /// </summary>
        public SortedList<Category> Categorys = new SortedList<Category>();

        /// <summary>
        /// 正常启动
        /// </summary>
        public bool SetUp;

        /// <summary>
        /// 唯一实例
        /// </summary>
        private static Bot _bot;// = new Bot();

        /// <summary>
        /// 获取实例
        /// </summary>
        public static Bot GetInstance(string resourceFolder = "")
        {
            return _bot ?? (_bot = new Bot(resourceFolder));
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        private Bot(string resourceFolder)
        {
            SetUp = false;
            //初始化
            Init(resourceFolder);
            BuildRealCategory();
            //正常启动
            SetUp = true;
        }

        /// <summary>
        /// 应答机器人的初始化工作
        /// </summary>
        public void Init(string resourceFolder = "")
        {
            if (!SetUp)
            {
                //xml文档对象
                var xmlDoc = new XmlDocument();
                //加载对应的智能文件
                if (resourceFolder == "")
                {
                    xmlDoc.Load(resourceFolder + "Iveely.AI.aiml");
                }
                else
                {
                    xmlDoc.Load(resourceFolder + "\\Iveely.AI.aiml");
                }
                //子节点集合
                var selectSingleNode = xmlDoc.SelectSingleNode("aiml");
                if (selectSingleNode != null)
                {
                    XmlNodeList nodeList = selectSingleNode.ChildNodes;
                    //当前结点
                    //遍历每一个子节点
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        foreach (var categoryNode in nodeList[i].ChildNodes)
                        {
                            //将当前结点赋值
                            XmlNode currentNode = (XmlNode)categoryNode;
                            //如果是类别结点
                            if (currentNode.Name.ToLower() == "category" && currentNode.HasChildNodes)
                            {
                                // 新建类别
                                var category = new Category();
                                //说明存在模式
                                var childList = currentNode.ChildNodes;
                                for (int j = 0; j < childList.Count; j++)
                                {
                                    //子节点赋值
                                    XmlNode childNode = childList[j];
                                    //如果是模式
                                    if (childNode.Name.ToLower() == "pattern")
                                    {
                                        //新建模式
                                        var pattern = new Pattern { Value = childNode.InnerXml.Trim() };
                                        //模式复制
                                        //下一个一定是模板
                                        j++;
                                        //新建模板
                                        var template = new Template();
                                        //如果没有子结点
                                        if (!childList[j].HasChildNodes)
                                        {
                                            //模板赋值
                                            template.Value = childList[j].InnerXml.Trim();
                                        }
                                        else
                                        {
                                            //获取当前所有的子节点
                                            XmlNodeList innerList = childList[j].ChildNodes;
                                            //定义子节点值
                                            string tempValue = "";
                                            //遍历循环
                                            for (int m = 0; m < innerList.Count; m++)
                                            {
                                                //如果是随机
                                                if (innerList[m].Name.ToLower() == "random")
                                                {
                                                    //说明存在随机组
                                                    var rand = new Rand();
                                                    //那么获取出随机值
                                                    for (var n = 0; n < innerList[m].ChildNodes.Count; n++)
                                                    {
                                                        //增加进去
                                                        rand.List.Add(innerList[m].ChildNodes[n].InnerXml);
                                                    }
                                                    //模板随机值
                                                    template.Rand = rand;
                                                }
                                                //如果是普通值
                                                else if (innerList[m].Name.ToLower() == "#text")
                                                {
                                                    tempValue += innerList[m].Value.Trim();
                                                }
                                                //如果是*号标记
                                                else if (innerList[m].Name.ToLower() == "star")
                                                {
                                                    //获取star属性index，标识位置
                                                    int index = int.Parse(innerList[m].Attributes["index"].Value);
                                                    //设定答值
                                                    template.Star.Add(index);
                                                }
                                                //如果是input号标记
                                                else if (innerList[m].Name.ToLower() == "input")
                                                {
                                                    //获取input属性index，标识位置
                                                    int index = int.Parse(innerList[m].Attributes["index"].Value);
                                                    //设定答值
                                                    template.Input = index;
                                                }
                                                //如果是Set标记
                                                else if (innerList[m].Name.ToLower() == "set")
                                                {
                                                    //获取Set属性name，标识位置
                                                    string setName = innerList[m].Attributes["name"].Value;
                                                    //索引号
                                                    string index = innerList[m].Attributes["index"].Value;
                                                    //设定变量
                                                    template.SetVariable.Name = setName;
                                                    //设定取值编号
                                                    template.SetVariable.Value = index;
                                                    //利用Star来记录
                                                    template.Star.Add(int.Parse(index));
                                                }
                                                //如果是Get标记
                                                else if (innerList[m].Name.ToLower() == "get")
                                                {
                                                    //获取get的变量
                                                    string getName = innerList[m].Attributes["name"].Value;
                                                    //变量名
                                                    template.GetVariable.Name = getName;
                                                }
                                                //如果是函数功能标记
                                                else if (innerList[m].Name.ToLower() == "function")
                                                {
                                                    //获取函数名称
                                                    template.Function.Name = innerList[m].Attributes["name"].Value;
                                                    //如果存在参数
                                                    if (innerList[m].Attributes["para"] != null)
                                                    {
                                                        //首先获取参数中的值
                                                        string[] par = innerList[m].Attributes["para"].Value.Split(',');
                                                        //遍历每一个参数值
                                                        foreach (string p in par)
                                                        {
                                                            //首先记录下是哪些参数
                                                            template.Star.Add(int.Parse(p));
                                                            //在函数体中也记录
                                                            template.Function.Parameters.Add(p);
                                                        }
                                                    }
                                                }
                                                //如果是疑问
                                                else if (innerList[m].Name.ToLower() == "question")
                                                {
                                                    string[] questionStrings = innerList[m].InnerText.Split(
                                                        new[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
                                                    foreach (var questionString in questionStrings)
                                                    {
                                                        //[*.0]和谁离婚了?[*.1]
                                                        string[] text = questionString.Split(new[] { '?', '？' },
                                                            StringSplitOptions.RemoveEmptyEntries);
                                                        if (text.Length == 2)
                                                        {
                                                            Template.Question question = new Template.Question
                                                            {
                                                                Description = text[0],
                                                                Answer = text[1]
                                                            };
                                                            template.AddQuestion(question);
                                                        }
                                                    }
                                                }
                                            }
                                            //将临时值放回去
                                            template.Value = tempValue.Trim();
                                        }
                                        //将模板赋值给模式
                                        pattern.Template = template;
                                        //将模式添加到模式集合
                                        category.Patterns.Add(pattern);
                                    }
                                }
                                //将类别添加到类别集合
                                Categorys.Add(category);
                            }
                        }
                    }
                }
            }

        }

        //<pattern>*[地名:北京|上海|天津|-ns]*将*[行为:开盘|发布会]*</pattern>
        //    <template>
        //        <function name="Normal" para="1,2,3"></function>
        //        <question>[0]什么地方[1]将[2][行为][3]？[地名]</question>
        //    </template>

        private void BuildRealCategory()
        {
            for (int i = 0; i < Categorys.Count; i++)
            {
                SortedList<Pattern> realPatterns = new SortedList<Pattern>();
                SortedList<Pattern> patterns = Categorys[i].Patterns;
                foreach (Pattern t in patterns)
                {
                    if (t.Value.Contains("["))
                    {
                        string patternValue = t.Value;
                        List<Pattern> myPatterns = new List<Pattern>();
                        int leftIndex = patternValue.IndexOf('[');
                        int rightIndex = patternValue.IndexOf(']');
                        // string middleValue = string.Empty;
                        while (leftIndex > -1)
                        {
                            string patternString = patternValue.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
                            string[] valueTemplate = patternString.Split(new[] { '|', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (myPatterns.Count == 0)
                            {
                                Pattern[] pts = new Pattern[valueTemplate.Count() - 1];
                                string header = patternValue.Substring(0, leftIndex);
                                for (int k = 0; k < pts.Count(); k++)
                                {
                                    pts[k] = new Pattern();
                                    pts[k].Value = header + valueTemplate[k + 1];
                                    Template template = new Template();

                                    Template.Question[] questions =
                                        new Template.Question[t.Template.Questions.Count];
                                    for (int n = 0; n < questions.Count(); n++)
                                    {
                                        questions[n] = new Template.Question();
                                        questions[n].Answer =
                                            t.Template.Questions[n].Answer.Replace(
                                                "[" + valueTemplate[0] + "]", valueTemplate[k + 1]);
                                        questions[n].Description = t.Template.Questions[n].Description.Replace(
                                            "[" + valueTemplate[0] + "]", valueTemplate[k + 1]);
                                    }
                                    template.Questions.AddRange(questions);
                                    template.Function = t.Template.Function;
                                    template.Star = t.Template.Star;
                                    pts[k].Template = template;
                                }
                                myPatterns.AddRange(pts);
                            }
                            else
                            {
                                Pattern[,] pts = new Pattern[myPatterns.Count, valueTemplate.Count() - 1];
                                for (int k = 0; k < myPatterns.Count; k++)
                                {

                                    for (int n = 0; n < valueTemplate.Count() - 1; n++)
                                    {
                                        pts[k, n] = new Pattern();
                                        pts[k, n].Value = myPatterns[k].Value + patternValue.Substring(0, leftIndex) + valueTemplate[n + 1];
                                        List<Template.Question> questions = myPatterns[k].Template.Questions;
                                        Template template = new Template();
                                        for (int m = 0; m < questions.Count; m++)
                                        {
                                            Template.Question myQuestion = new Template.Question();
                                            myQuestion.Description = questions[m].Description.Replace(
                                                "[" + valueTemplate[0] + "]", valueTemplate[n + 1]);
                                            myQuestion.Answer = questions[m].Answer.Replace(
                                                "[" + valueTemplate[0] + "]", valueTemplate[n + 1]);
                                            template.AddQuestion(myQuestion);

                                        }
                                        template.Function = myPatterns[k].Template.Function;
                                        template.Star = myPatterns[k].Template.Star;
                                        pts[k, n].Template = template;
                                    }
                                }

                                myPatterns.Clear();
                                foreach (var p in pts)
                                {
                                    myPatterns.Add(p);
                                }
                            }


                            patternValue = patternValue.Substring(rightIndex + 1, patternValue.Length - rightIndex - 1);
                            //  int lastRightIndex = rightIndex;
                            leftIndex = patternValue.IndexOf('[');
                            rightIndex = patternValue.IndexOf(']');
                            if (rightIndex == -1)
                            {
                                foreach (Pattern t1 in myPatterns)
                                {
                                    t1.Value += patternValue;
                                }
                            }
                        }
                        realPatterns.AddRange(myPatterns);
                    }
                    else
                    {
                        //Pattern pattern = new Pattern();
                        realPatterns.Add(t);
                    }
                }
                Categorys[i].Patterns = realPatterns;
            }
        }

        public string Answer(string input)
        {
            //记录用户的输入
            Input.List.Add(input);
            //遍历类别
            foreach (Category cate in Categorys)
            {
                foreach (Pattern pat in cate.Patterns)
                {
                    if (Analyse.Match(pat.Value, input))
                    {
                        string replyInfo = pat.Template.Reply();
                        return replyInfo;
                    }
                }
            }
            return string.Empty;
        }

        public List<Template.Question> BuildQuestion(string input, params string[] references)
        {
            Interrogative interrogative = new Interrogative();
            List<Template.Question> questions = new List<Template.Question>();
            if (input.Length < 10 || input.Length > 100)
            {
                return questions;
            }
            List<Tuple<string, string>> list = interrogative.GetQuestions(input);
            if (list != null && list.Count > 0)
            {
                foreach (Tuple<string, string> tuple in list)
                {
                    Template.Question question = new Template.Question();
                    question.Description = tuple.Item1;
                    question.Answer = tuple.Item2;
                    question.Reference = references[0];
                    question.FromTitle = references[1];
                    questions.Add(question);
                }
            }
            return questions;
        }
    }
}
