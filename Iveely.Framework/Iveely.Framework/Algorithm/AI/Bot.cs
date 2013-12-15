/////////////////////////////////////////////////
///文件名:Bot
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/27 20:51:37
///////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
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
        private static Bot bot;// = new Bot();

        /// <summary>
        /// 获取实例
        /// </summary>
        public static Bot GetInstance()
        {
            if (bot == null)
            {
                bot = new Bot();

            }
            return bot;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        private Bot()
        {
            SetUp = false;
            //初始化
            Init();
            BuildRealCategory();
            //正常启动
            SetUp = true;
        }

        /// <summary>
        /// 应答机器人的初始化工作
        /// </summary>
        public void Init()
        {
            if (!SetUp)
            {
                //xml文档对象
                var xmlDoc = new XmlDocument();
                //加载对应的智能文件
                xmlDoc.Load("strai.aiml");
                //子节点集合
                XmlNodeList nodeList = xmlDoc.SelectSingleNode("aiml").ChildNodes;
                //当前结点
                XmlNode currentNode;
                //遍历每一个子节点
                for (int i = 0; i < nodeList.Count; i++)
                {
                    foreach (var categoryNode in nodeList[i].ChildNodes)
                    {

                        ///将当前结点赋值
                        currentNode = (XmlNode)categoryNode;
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
                                                        Template.Question question = new Template.Question();
                                                        question.Doubt = text[0];
                                                        question.Answer = text[1];
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
                            this.Categorys.Add(category);
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
            for (int i = 0; i < this.Categorys.Count; i++)
            {
                SortedList<Pattern> realPatterns = new SortedList<Pattern>();
                SortedList<Pattern> patterns = Categorys[i].Patterns;
                for (int j = 0; j < patterns.Count; j++)
                {
                    if (patterns[j].Value.Contains("["))
                    {
                        string patternValue = patterns[j].Value;
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
                                        new Template.Question[patterns[j].Template.Questions.Count];
                                    for (int n = 0; n < questions.Count(); n++)
                                    {
                                        questions[n] = new Template.Question();
                                        questions[n].Answer =
                                            patterns[j].Template.Questions[n].Answer.Replace(
                                                "[" + valueTemplate[0] + "]", valueTemplate[k + 1]);
                                        questions[n].Doubt = patterns[j].Template.Questions[n].Doubt.Replace(
                                                "[" + valueTemplate[0] + "]", valueTemplate[k + 1]);
                                    }
                                    template.Questions.AddRange(questions);
                                    template.Function = patterns[j].Template.Function;
                                    template.Star = patterns[j].Template.Star;
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
                                            myQuestion.Doubt = questions[m].Doubt.Replace(
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
                                for (int p = 0; p < myPatterns.Count; p++)
                                {
                                    myPatterns[p].Value += patternValue;
                                }
                            }
                        }
                        realPatterns.AddRange(myPatterns);
                    }
                    else
                    {
                        //Pattern pattern = new Pattern();
                        realPatterns.Add(patterns[j]);
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
            foreach (Category cate in this.Categorys)
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

        public string BuildQuestion(string input)
        {
            foreach (Category cate in this.Categorys)
            {
                foreach (Pattern pat in cate.Patterns)
                {
                    if (Analyse.Match(pat.Value, input))
                    {
                        string replyInfo = pat.Template.BuildQuestion();
                        return replyInfo;
                    }
                }
            }
            return string.Empty;
        }
    }
}
