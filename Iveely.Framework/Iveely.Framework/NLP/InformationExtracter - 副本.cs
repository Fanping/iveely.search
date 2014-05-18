using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Algorithm;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 信息抽取
    /// </summary>
    public class InformationExtracter
    {
        /// <summary>
        /// 替代词检测器
        /// </summary>
        internal class PronounsChecker
        {
            /// <summary>
            /// 类型编号
            /// </summary>
            public int TypeId { get; internal set; }

            /// <summary>
            /// 规则类型
            /// </summary>
            public string TypeName { get; internal set; }

            /// <summary>
            /// 类型标记
            /// </summary>
            public string TypeFlag { get; internal set; }

            /// <summary>
            /// 时候在最近查找（有最近和最前两者方式）
            /// </summary>
            public bool IsNear { get; internal set; }

            /// <summary>
            /// 类型检测规则
            /// </summary>
            public string Rule { get; internal set; }

            public void Replace(Tuple<string[], string[]> lastTuple, ref Tuple<string[], string[]> tuple,
                Hashtable remember)
            {
                for (int i = 0; i < tuple.Item2.Length; i++)
                {
                    if (tuple.Item2[i] == this.TypeFlag)
                    {
                        string flg = TypeFlag + " " + Rule;
                        if (!remember.ContainsKey(flg) || remember[flg].ToString() == string.Empty)
                        {
                            bool isHere = false;
                            List<int> tmp = GetEntity(lastTuple.Item2, tuple.Item2, i, ref isHere);
                            if (tmp.Count > 0)
                            {
                                tuple.Item1[i] = string.Empty;
                                if (isHere)
                                {
                                    foreach (int t in tmp)
                                    {
                                        tuple.Item1[i] += tuple.Item1[t];
                                        tuple.Item2[i] = Rule;
                                    }
                                    if (remember.ContainsKey(flg))
                                    {
                                        remember[flg] = tuple.Item1[i];
                                    }
                                }
                                else
                                {
                                    foreach (int t in tmp)
                                    {
                                        tuple.Item1[i] += lastTuple.Item1[t];
                                        tuple.Item2[i] = Rule;
                                    }
                                    if (remember.ContainsKey(flg) && i > 0)
                                    {
                                        remember[flg] = tuple.Item1[i];
                                    }
                                }
                            }
                        }
                        else
                        {
                            tuple.Item1[i] = remember[flg].ToString();
                        }
                    }
                }
            }

            private List<int> GetEntity(string[] lastSemantics, string[] semantics, int endiIndex,
                ref bool isInThisSentence)
            {
                List<int> tmp = new List<int>();

                //如果寻找最近的
                if (IsNear)
                {
                    bool isFind = false;
                    //本句话中查找
                    for (int i = endiIndex; i > -1; i--)
                    {
                        if (semantics[i] == this.Rule && (tmp.Count == 0 || tmp[tmp.Count - 1] == i - 1))
                        {
                            tmp.Add(i);
                            isFind = true;
                        }
                    }
                    //如果在本句子中没有找到
                    if (!isFind)
                    {
                        //上句话中查找
                        for (int i = lastSemantics.Length - 1; i > -1; i--)
                        {
                            if (lastSemantics[i] == this.Rule && (tmp.Count == 0 || tmp[tmp.Count - 1] == i - 1))
                            {
                                tmp.Add(i);
                            }
                        }
                    }
                    isInThisSentence = isFind;
                }

                else
                {
                    bool isFind = false;
                    //寻找最远的
                    for (int i = 0; i < endiIndex; i++)
                    {
                        if (semantics[i] == this.Rule && (tmp.Count == 0 || tmp[tmp.Count - 1] == i - 1))
                        {
                            tmp.Add(i);
                            isFind = true;
                        }
                    }

                    //如果本句中没有找到
                    if (!isFind)
                    {
                        //上句话中查找
                        for (int i = lastSemantics.Length - 1; i > -1; i--)
                        {
                            if (lastSemantics[i] == this.Rule && (tmp.Count == 0 || tmp[tmp.Count - 1] == i - 1))
                            {
                                tmp.Add(i);
                            }
                        }
                    }

                    isInThisSentence = isFind;
                }
                return tmp;
            }
        }

        /// <summary>
        /// 提取模式
        /// </summary>
        internal class ExtractPattern
        {
            /// <summary>
            /// 模式类型
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// 匹配模式
            /// </summary>
            private readonly List<string> _pattern;

            /// <summary>
            /// 与模式匹配后的值信息
            /// </summary>
            private readonly List<string> _values;

            /// <summary>
            /// 实体关系发出者
            /// </summary>
            private readonly List<int> _entityA;

            /// <summary>
            /// 实体关系接受者
            /// </summary>
            private readonly List<int> _entityB;

            /// <summary>
            /// 实体之间的关系
            /// </summary>
            private readonly List<object> _relationship;

            /// <summary>
            /// 上一个词性
            /// </summary>
            private string _lastSemantic;

            /// <summary>
            /// 是否在句子结束的时候才允许提取数据
            /// </summary>
            private bool _shouldExtractInEnd;

            /// <summary>
            /// 当前索引位
            /// </summary>
            private int _currentIndex = 0;

            /// <summary>
            /// 构造提取模式
            /// </summary>
            /// <param name="strPattern"></param>
            /// <param name="descInfos"></param>
            public ExtractPattern(string strPattern, string descInfos)
            {
                if (strPattern.Trim().Length < 1)
                {
                    throw new ArgumentNullException();
                }

                //变量初始化
                _values = new List<string>();
                _entityA = new List<int>();
                _entityB = new List<int>();
                _relationship = new List<object>();
                _pattern = new List<string>();

                //初始化模式
                string[] tempPatterns = strPattern.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tempPattern in tempPatterns)
                {
                    if (_pattern.Count > 0 && _pattern[_pattern.Count - 1] == tempPattern)
                    {
                        continue;
                    }
                    else
                    {
                        _pattern.Add(tempPattern);
                    }
                }

                //初始化描述
                string[] infors = descInfos.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < infors.Length; i++)
                {
                    string[] vals = infors[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var val in vals)
                    {
                        if (i == 0)
                        {
                            _entityA.Add(int.Parse(val));
                        }

                        else if (i == 1 && infors.Length == 3)
                        {
                            int tmp = 0;
                            if (int.TryParse(val, out tmp))
                            {
                                _relationship.Add(int.Parse(val));
                            }
                            else
                            {
                                _relationship.Add(val);
                            }
                        }
                        else
                        {
                            if (val == ".+")
                            {
                                _entityB.Add(-1);
                                _shouldExtractInEnd = true;
                            }
                            else
                            {
                                _entityB.Add(int.Parse(val));
                                _shouldExtractInEnd = false;
                            }
                        }
                    }
                }
            }

            public string GetPatternString()
            {
                return string.Join(" ", this._pattern);
            }

            /// <summary>
            /// 添加用于检查是否满足提取模式
            /// </summary>
            /// <param name="semantic"></param>
            /// <param name="val"></param>
            public void Check(string semantic, string val)
            {
                //如果本条词性与上一条相同，则累加
                if (_currentIndex < this._pattern.Count && _currentIndex > 0 && _pattern[_currentIndex - 1] == semantic)
                {
                    int valIndex = _values.Count - 1;
                    if (_lastSemantic == semantic)
                    {
                        _values[valIndex] += val;
                    }
                    else
                    {
                        _values[valIndex] = val;
                    }
                    return;
                }

                //如果当前匹配到
                if (_currentIndex < this._pattern.Count && _pattern[_currentIndex] == semantic)
                {
                    _currentIndex++;
                    _lastSemantic = semantic;
                    _values.Add(val);
                    return;
                }

                //如果是任意忽略匹配符
                if (_currentIndex < this._pattern.Count && _pattern[_currentIndex] == ".*")
                {
                    _currentIndex++;
                    _lastSemantic = ".*";
                    Check(semantic, val);
                }

                //如果是任意通配符
                if (_currentIndex >= this._pattern.Count || _pattern[_currentIndex] == ".+")
                {
                    _currentIndex++;
                    _values.Add(val);
                }

                _lastSemantic = semantic;
            }

            /// <summary>
            /// 获取匹配后的结果集(是否强制取出)
            /// </summary>
            /// <returns></returns>
            public string[] GetResult(bool force = false)
            {
                try
                {
                    if ((IsMatch() || force) && _values.Count > 0)
                    {
                        string[] results = new string[3];
                        int valIndex = 0;

                        //实体A
                        for (int i = 0; i < _entityA.Count; i++)
                        {
                            results[0] += _values[_entityA[i]];
                            valIndex = _entityA[i];
                        }

                        //关系
                        for (int i = 0; i < _relationship.Count; i++)
                        {
                            int tmp = 0;
                            if (int.TryParse(_relationship[i].ToString(), out tmp))
                            {
                                results[1] += _values[tmp];
                                valIndex = tmp;
                            }
                            else
                            {
                                results[1] += _relationship[i];
                            }
                        }

                        //实体B
                        for (int i = 0; i < _entityB.Count; i++)
                        {
                            if (_entityB[i] == -1)
                            {
                                for (int j = valIndex + 1; j < _values.Count; j++)
                                {
                                    results[2] += _values[j];
                                }
                            }
                            else
                            {
                                results[2] += _values[_entityB[i]];
                            }
                        }
                        if (results[0] != null && results[1] != null && results[2] != null)
                            return results;
                        return null;
                    }
                }
                catch
                {
                }
                return null;
            }

            public string GetResultToXml()
            {
                return string.Empty;
            }

            /// <summary>
            /// 是否满足本条提取规则
            /// </summary>
            /// <returns></returns>
            public bool IsMatch()
            {
                return _currentIndex >= _pattern.Count && !_shouldExtractInEnd;
            }

            /// <summary>
            /// 还原规则
            /// </summary>
            public void Recover()
            {
                this._currentIndex = 0;
                _lastSemantic = string.Empty;
                _values.Clear();
            }

        }

        /// <summary>
        /// 实体关系信息
        /// </summary>
        internal class EntityRelation
        {
            /// <summary>
            /// 实体A
            /// </summary>
            public string A { get; internal set; }

            /// <summary>
            /// 子关系集合
            /// </summary>
            private readonly HashSet<EntityRelation> _children;

            /// <summary>
            /// 与父实体关系
            /// </summary>
            public string Relationship { get; internal set; }

            /// <summary>
            /// 关系权重（如果大家都认同这种关系，权值就越到）
            /// </summary>
            public double Weight { get; internal set; }

            /// <summary>
            /// 关系识别期
            /// </summary>
            public DateTime DateTime { get; private set; }

            public EntityRelation()
            {
                _children = new HashSet<EntityRelation>();
            }

            public void AddChild(EntityRelation relation)
            {
                //BUG:如果存在则需要修改权值
                if (!_children.Contains(relation))
                {
                    _children.Add(relation);
                    return;
                }
            }

            /// <summary>
            /// 生成思维导图
            /// </summary>
            /// <returns></returns>
            public string ToMindMap(int deepLevel = 0, int maxDeep = 5, double minRank = 0.2)
            {
                string str = "<node>" +
                             "<X>100</X>" +
                             "<Y>100</Y>" +
                             "<width>100</width>" +
                             "<height>50</height>" +
                             "<r_start_x>100</r_start_x>" +
                             "<r_start_y>100</r_start_y>" +
                             "<r_text>" + Relationship + "</r_text>" +
                             "<r_weight>" + Weight + "</r_weight>" +
                             "<stil>BBT.Style</stil>" +
                             "<text>" + A + "</text>" +
                             "<form>Rechteck</form>" +
                             "<style>" +
                             "<farbe>#FF000000</farbe>" +
                             "<fill>false</fill>" +
                             "<font>12</font>" +
                             "<icon />" +
                             "</style>" +
                             "<childs>";
                if (deepLevel <= maxDeep)
                {
                    foreach (var entityRelation in _children)
                    {
                        if (entityRelation.Weight >= minRank)
                        {
                            str += entityRelation.ToMindMap(deepLevel++, maxDeep);
                        }
                    }
                }
                str += "</childs>";
                str += "</node>";
                return str;
            }
        }

        /// <summary>
        /// 实体关系管理
        /// </summary>
        internal class EntityManager
        {
            /// <summary>
            /// 实体与实体的倒排索引
            /// </summary>
            private static InvertFragment eeFragment;

            /// <summary>
            /// A实体与关系的倒排索引
            /// </summary>
            private static InvertFragment arFragment;

            /// <summary>
            /// B实体与关系的倒排
            /// </summary>
            private static InvertFragment brFragment;

            /// <summary>
            /// 实体表索引
            /// </summary>
            private static Hashtable _entityIndex;


            public EntityManager()
            {
                _entityIndex = new Hashtable();
                if (eeFragment == null)
                {
                    eeFragment = new InvertFragment();
                    arFragment = new InvertFragment();
                    brFragment = new InvertFragment();
                }
            }

            /// <summary>
            /// 获取两个实体之间的关系
            /// </summary>
            /// <param name="entityA"></param>
            /// <param name="relation"></param>
            /// <param name="entityB"></param>
            /// <returns></returns>
            public List<string> GetRelation(string entityA, string relation, string entityB)
            {
                //找实体与实体之间的关系
                List<string> list = GetRelationList(eeFragment, entityA, entityB, relation);

                //实体A与关系之间的实体B
                if (list == null)
                    list = GetRelationList(arFragment, entityA, entityB, relation);

                //实体B与关系之间的实体B
                if (list == null)
                    list = GetRelationList(brFragment, entityA, entityB, relation);

                return list;
            }

            /// <summary>
            /// 添加实体关系
            /// </summary>
            public void AddEntityRelation(string entityA, string relationship, string entityB)
            {
                if (entityA == entityB)
                {
                    return;
                }
                EntityRelation entityRelationA = FindParent(entityA);
                if (entityRelationA == null)
                {
                    entityRelationA = new EntityRelation();
                    entityRelationA.A = entityA;
                    entityRelationA.Weight = 0.1;
                    _entityIndex.Add(entityA, entityRelationA);
                }

                EntityRelation entityRelationB = FindParent(entityB);
                if (entityRelationB == null)
                {
                    EntityRelation childEntity = new EntityRelation();
                    childEntity.Weight = 0.1;
                    childEntity.Relationship = relationship;
                    childEntity.A = entityB;
                    entityRelationA.AddChild(childEntity);
                    _entityIndex.Add(entityB, childEntity);
                }
                else
                {
                    //entityRelationB.AddChild(childEntity);
                    entityRelationB.Weight += 0.1;
                }

                eeFragment.AddDocument(relationship, new[] { entityA, entityB });
                arFragment.AddDocument(entityB, new[] { entityA, relationship });
                brFragment.AddDocument(entityA, new[] { entityB, relationship });
            }

            public string RelationToXml(double minRank, string entity = "")
            {
                string result = "<mindmap>";
                if (entity != "")
                {
                    if (_entityIndex.ContainsKey(entity))
                    {
                        result += ((EntityRelation)_entityIndex[entity]).ToMindMap(0, 5, minRank);
                    }

                }
                else
                {
                    foreach (DictionaryEntry de in _entityIndex)
                    {
                        result += ((EntityRelation)de.Value).ToMindMap(0, 5, minRank);
                        break;
                    }
                }
                result += "</mindmap>";
                return result;
            }

            /// <summary>
            /// 清除所有实体
            /// </summary>
            public void Clear()
            {
                _entityIndex.Clear();
            }

            private List<string> GetRelationList(InvertFragment fragment, string a, string b, string c)
            {
                List<string> list = fragment.FindCommonDocumentByKeys(new string[] { a, b }, 10);
                if (list != null && list.Count > 0)
                {
                    return list;
                }

                list = fragment.FindCommonDocumentByKeys(new string[] { b, c }, 10);
                if (list != null && list.Count > 0)
                {
                    return list;
                }

                list = fragment.FindCommonDocumentByKeys(new string[] { c, a }, 10);
                if (list != null && list.Count > 0)
                {
                    return list;
                }
                return list;
            }

            /// <summary>
            /// 查找父节点
            /// </summary>
            /// <param name="entityA"></param>
            /// <returns></returns>
            private EntityRelation FindParent(string entityA)
            {
                if (_entityIndex.ContainsKey(entityA))
                {
                    return (EntityRelation)_entityIndex[entityA];
                }
                return null;
            }


        }

        /// <summary>
        /// 单例
        /// </summary>
        private static InformationExtracter _extracter;

        /// <summary>
        /// 代词检测器
        /// </summary>
        private static List<PronounsChecker> _pronounsPatterns;

        /// <summary>
        /// 提取匹配模式
        /// </summary>
        private static List<ExtractPattern> _extractPatterns;

        /// <summary>
        /// 实体管理器
        /// </summary>
        private EntityManager _entityManager;

        /// <summary>
        /// 代词记忆
        /// </summary>
        private static Hashtable _pronounsRemember;

        public static InformationExtracter GetInstance()
        {
            if (_extracter == null)
            {
                _extracter = new InformationExtracter();
            }
            return _extracter;
        }

        private InformationExtracter()
        {
            _pronounsRemember = new Hashtable();
            _pronounsPatterns = new List<PronounsChecker>();
            _extractPatterns = new List<ExtractPattern>();
            _entityManager = new EntityManager();
            this.LoadPronounsPatterns("Init\\Corpus_Pronouns_Rule.txt");
            this.LoadExtractPatterns("Init\\Corpus_Entity_Extract.txt");
        }

        /// <summary>
        /// 读取内容
        /// </summary>
        /// <param name="content"></param>
        public void ReadContent(string content)
        {
            Framework.Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();

            //第一步：替换代词
            string[] lines = content.Split(new[] { "\r\n", "\n", ".", "。" }, StringSplitOptions.RemoveEmptyEntries);
            lines = ReplacePronouns(lines);

            //第二步：信息抽取
            ExtractByPattern(lines);
        }

        /// <summary>
        /// 代词替换
        /// </summary>
        /// <param name="content"></param>
        /// <param name="buildWithLast"></param>
        /// <returns></returns>
        public string[] ReplacePronouns(string[] content, bool buildWithLast = true)
        {
            List<string> lines = new List<string>();
            Framework.Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();
            Tuple<string[], string[]> lastTuple = new Tuple<string[], string[]>(new string[0], new string[0]);
            for (int i = 0; i < content.Length; i++)
            {
                Tuple<string[], string[]> currentTuple = segment.SplitToArray(content[i]);
                lines.Add(ReplacePronouns(ref currentTuple, lastTuple));
                if (buildWithLast)
                    lastTuple = currentTuple;
            }
            return lines.ToArray();
        }

        /// <summary>
        /// 代词替换
        /// </summary>
        /// <param name="text"></param>
        /// <param name="currentyTuple"></param>
        /// <returns></returns>
        private string ReplacePronouns(ref Tuple<string[], string[]> currentyTuple, Tuple<string[], string[]> lastTuple)
        {
            foreach (PronounsChecker pronounsChecker in _pronounsPatterns)
            {
                pronounsChecker.Replace(lastTuple, ref currentyTuple, _pronounsRemember);
            }
            return string.Join("", currentyTuple.Item1);
        }

        /// <summary>
        /// 加载代词匹配模式
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadPronounsPatterns(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] text = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length == 5)
                {
                    PronounsChecker checker = new PronounsChecker();
                    checker.Rule = text[0];
                    checker.TypeId = int.Parse(text[1]);
                    checker.TypeFlag = text[2];
                    checker.IsNear = text[3] == "near";
                    checker.TypeName = text[4];
                    _pronounsPatterns.Add(checker);

                    if (checker.IsNear == false)
                    {
                        _pronounsRemember.Add(checker.TypeFlag + " " + checker.Rule, "");
                    }
                }
            }
        }

        /// <summary>
        /// 加载提取模式
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadExtractPatterns(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (line.Length > 0)
                {
                    string[] t = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    ExtractPattern pattern =
                        new ExtractPattern(t[0], t[1]);
                    _extractPatterns.Add(pattern);
                }
            }
        }

        private void ExtractByPattern(string[] lines)
        {

#if DEBUG
            if (File.Exists("_InforExtracter_result.txt"))
            {
                File.Delete("_InforExtracter_result.txt");
            }
#endif

            List<string> result = new List<string>();
            foreach (var line in lines)
            {
                GetInforByPattern(line, false);
            }

        }

        /// <summary>
        /// 对一句话获取信息
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInChecking"></param>
        /// <returns></returns>
        public List<string[]> GetInforByPattern(string line, bool isInChecking)
        {
            List<string[]> allResults = new List<string[]>();
            Iveely.Framework.Text.Segment.IctclasSegment ictclasSegment = Text.Segment.IctclasSegment.GetInstance();
            Tuple<string[], string[]> tuple = ictclasSegment.SplitToArray(line);
            string nextSemantic = string.Empty;
            for (int i = 0; i < tuple.Item2.Length; i++)
            {
                if (i < tuple.Item2.Length - 1)
                {
                    nextSemantic = tuple.Item2[i + 1];
                }
                else
                {
                    nextSemantic = string.Empty;
                }
                foreach (var extractPattern in _extractPatterns)
                {
                    extractPattern.Check(tuple.Item2[i], tuple.Item1[i]);
                    if (nextSemantic != tuple.Item2[i] && extractPattern.IsMatch())
                    {
                        string[] results = extractPattern.GetResult();
                        if (!isInChecking)
                        {
#if DEBUG
                            File.AppendAllText("_InforExtracter_result.txt",
                                string.Join("->", results) + "     " + extractPattern.GetPatternString() + "\r\n");
#endif
                            _entityManager.AddEntityRelation(results[0], results[1], results[2]);
                        }
                        extractPattern.Recover();
                        allResults.Add(results);
                    }
                }
            }

            foreach (var extractPattern in _extractPatterns)
            {
                string[] results = extractPattern.GetResult(true);
                if (results != null)
                {
                    _entityManager.AddEntityRelation(results[0], results[1], results[2]);
                    allResults.Add(results);
                }
                extractPattern.Recover();
            }
            return allResults;
        }

        /// <summary>
        /// 获取两个实体之间的对应关系
        /// </summary>
        /// <param name="entityA"></param>
        /// <param name="entityB"></param>
        /// <returns></returns>
        public List<string> GetRelation(string entityA, string relation, string entityB)
        {
            return _entityManager.GetRelation(entityA, relation, entityB);
        }

        public string InforToMindMap(double minRank, string entity = "")
        {
            return _entityManager.RelationToXml(minRank, entity);
        }

        public void Clear()
        {
            _entityManager.Clear();
        }

#if DEBUG

        public void Debug_TestReplace()
        {
            string context = File.ReadAllText("代词替换测试.txt", Encoding.UTF8);
            Framework.Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();
            string[] content = context.Split(new[] { "\r\n", "\n", ",", ".", "。", "，" }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder builder = new StringBuilder();
            Tuple<string[], string[]> lastTuple = new Tuple<string[], string[]>(new string[0], new string[0]);
            for (int i = 0; i < content.Length; i++)
            {
                Tuple<string[], string[]> currentTuple = segment.SplitToArray(content[i]);
                builder.AppendLine(ReplacePronouns(ref currentTuple, lastTuple));
                lastTuple = currentTuple;
            }
            File.WriteAllText("代词替换测试.txt", string.Join("\r\n", content));
            File.WriteAllText("代词替换测试_result.txt", builder.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 获取含有代词的句子
        /// </summary>
        /// <param name="fileName"></param>

        public void Debug_GetAllPronouns(string fileName)
        {
            Framework.Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();
            string[] allLines = File.ReadAllText(fileName, Encoding.UTF8).Split(new string[] { "\r\n", "\r", "\n", ".", "。", "!", "！" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> lines = new List<string>();
            foreach (string line in allLines)
            {
                if (line.Trim().Length > 1)
                {
                    Tuple<string[], string[]> tuple = segment.SplitToArray(line.Trim());
                    for (int i = 0; i < tuple.Item2.Length; i++)
                    {
                        if (tuple.Item2[i] == "rr" || tuple.Item2[i] == "rz"
                            || tuple.Item2[i] == "rzt" || tuple.Item2[i] == "rzs" || tuple.Item2[i] == "rzv")
                        {
                            lines.Add(line);
                            break;
                        }
                    }
                }
            }

            StringBuilder builder = new StringBuilder();
            foreach (string line in lines)
            {
                //Tuple<string[], string[]> tuple = segment.SplitToArray(line);
                //for (int i = 0; i < tuple.Item2.Length; i++)
                //{
                //    builder.Append(tuple.Item1[i] + "/" + tuple.Item2[i] + " ");
                //}
                //builder.AppendLine("\r\n");
                builder.AppendLine(line);
            }

            File.WriteAllText("代词替换测试_Pronouns.txt", builder.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 获取实体关系
        /// </summary>
        /// <param name="fileName"></param>

        public void Debug_GetAllEntityRelationship(string fileName)
        {
            string allText = File.ReadAllText(fileName, Encoding.UTF8);
            Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();
            string[] text = allText.Split(new[] { ",", "，", ".", "。", "!", "！" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < text.Length; i++)
            {
                Tuple<string[], string[]> tuple = segment.SplitToArray(text[0]);
            }
            Console.WriteLine(segment.splitWithSemantic(allText));
        }



#endif
    }
}
