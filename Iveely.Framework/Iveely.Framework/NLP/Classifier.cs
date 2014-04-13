using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iveely.Framework.NLP
{

    /// <summary>
    /// ±¥“∂Àπ∑÷¿‡∆˜
    /// </summary>
    [Serializable]
    public class Classifier : IClassifier
    {
        private readonly SortedDictionary<string, ICategory> _categories;

        private readonly ExcludedWords _excludedWords;

        private static Classifier classifier;

        private bool isInit;

        private Classifier(bool isInit)
        {
            this.isInit = isInit;
            _categories = new SortedDictionary<string, ICategory>();
            _excludedWords = new ExcludedWords();
            _excludedWords.InitDefault();
            classifier = null;
        }

        public static Classifier GetInstance(string corpusFolder)
        {
            string serFile = "Init\\Classifier.ser";
            if (classifier == null && File.Exists(serFile))
            {
                classifier = Text.Serializer.DeserializeFromFile<Classifier>(serFile);
                if (classifier != null)
                {
                    classifier.isInit = true;
                }
            }
            else if (classifier == null)
            {
                classifier = new Classifier(false);
                classifier.Learn(corpusFolder);
                Text.Serializer.SerializeToFile(classifier, serFile);
                classifier.isInit = true;
            }
            return classifier;
        }

        public void Learn(string corpusFolder)
        {
            if (classifier.isInit)
            {
                return;
            }
            string[] dirs = Directory.GetDirectories(corpusFolder);
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                string catName = dir.Substring(dir.LastIndexOf("\\", System.StringComparison.Ordinal) + 1, dir.Length - dir.LastIndexOf("\\", System.StringComparison.Ordinal) - 1);
                foreach (string file in files)
                {
                    this.TeachCategory(catName, new System.IO.StreamReader(file, Encoding.Default));
                }
            }
        }

        private int CountTotalWordsInCategories()
        {
            int total = 0;
            foreach (var category in _categories.Values)
            {
                var cat = (Category)category;
                total += cat.TotalWords;
            }
            return total;
        }

        private ICategory GetOrCreateCategory(string cat)
        {
            ICategory c;
            if (!_categories.TryGetValue(cat, out c))
            {
                c = new Category(cat, _excludedWords);
                _categories.Add(cat, c);
            }
            return c;
        }

        public void TeachPhrases(string cat, string[] phrases)
        {
            GetOrCreateCategory(cat).TeachPhrases(phrases);
        }

        public void TeachCategory(string cat, System.IO.TextReader tr)
        {
            GetOrCreateCategory(cat).TeachCategory(tr);
        }

        public Dictionary<string, double> Classify(string text)
        {
            Dictionary<string, double> score = new Dictionary<string, double>();
            foreach (KeyValuePair<string, ICategory> cat in _categories)
            {
                score.Add(cat.Value.Name, 0.0);
            }

            EnumerableCategory wordsInFile = new EnumerableCategory("", _excludedWords);
            char[] words = text.ToCharArray();
            foreach (char word in words)
            {
                if (!string.IsNullOrWhiteSpace(word.ToString()))
                    wordsInFile.TeachPhrase(word.ToString());
            }


            double maxScore = 0;
            foreach (KeyValuePair<string, PhraseCount> kvp1 in wordsInFile)
            {
                PhraseCount pcInFile = kvp1.Value;
                foreach (KeyValuePair<string, ICategory> kvp in _categories)
                {
                    ICategory cat = kvp.Value;
                    int count = cat.GetPhraseCount(pcInFile.RawPhrase);
                    if (count > 0)
                    {
                        score[cat.Name] += (double)count / (double)cat.TotalWords;
                        if (score[cat.Name] > maxScore)
                        {
                            maxScore = score[cat.Name];
                        }
                    }
                    System.Diagnostics.Trace.WriteLine(pcInFile.RawPhrase.ToString() + "(" +
                        cat.Name + ")" + score[cat.Name]);
                }


            }

            if (maxScore > 0)
            {
                Dictionary<string, double> finaScore = new Dictionary<string, double>();
                foreach (KeyValuePair<string, double> kv in score)
                {
                    finaScore.Add(kv.Key, kv.Value / maxScore);
                }
                return finaScore;
            }
            //foreach (KeyValuePair<string, ICategory> kvp in m_Categories)
            //{
            //    ICategory cat = kvp.Value;
            //    score[cat.Name] += (double)cat.TotalWords / (double)this.CountTotalWordsInCategories();
            //}
            return score;
        }

        public string GetPossibleClassify(string text)
        {
            Dictionary<string, double> score = new Dictionary<string, double>();
            foreach (KeyValuePair<string, ICategory> cat in _categories)
            {
                score.Add(cat.Value.Name, 0.0);
            }

            EnumerableCategory wordsInFile = new EnumerableCategory("", _excludedWords);
            char[] words = text.ToCharArray();
            foreach (char word in words)
            {
                if (!string.IsNullOrWhiteSpace(word.ToString()))
                    wordsInFile.TeachPhrase(word.ToString());
            }


            double maxScore = 0;
            string classifyType = "";
            foreach (KeyValuePair<string, PhraseCount> kvp1 in wordsInFile)
            {
                PhraseCount pcInFile = kvp1.Value;
                foreach (KeyValuePair<string, ICategory> kvp in _categories)
                {
                    ICategory cat = kvp.Value;
                    int count = cat.GetPhraseCount(pcInFile.RawPhrase);
                    if (count > 0)
                    {
                        score[cat.Name] += (double)count / (double)cat.TotalWords;
                        if (score[cat.Name] > maxScore)
                        {
                            maxScore = score[cat.Name];
                            classifyType = cat.Name;
                        }
                    }
                    System.Diagnostics.Trace.WriteLine(pcInFile.RawPhrase.ToString() + "(" +
                        cat.Name + ")" + score[cat.Name]);
                }


            }
            return classifyType;
        }
    }
}
