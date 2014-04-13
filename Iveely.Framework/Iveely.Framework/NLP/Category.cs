using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Iveely.Framework.NLP
{

    [Serializable]
    class EnumerableCategory : Category, IEnumerable<KeyValuePair<string, PhraseCount>>
    {
        public EnumerableCategory(string cat, ExcludedWords excluded)
            : base(cat, excluded)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<KeyValuePair<string, PhraseCount>> GetEnumerator()
        {
            return MPhrases.GetEnumerator();
        }

    }

    [Serializable]
    class Category : ICategory
    {
        protected System.Collections.Generic.SortedDictionary<string, PhraseCount> MPhrases;
        int _totalWords;
        readonly string _mName;
        readonly ExcludedWords _mExcluded;

        public string Name
        {
            get { return _mName; }
        }
      
        public int TotalWords
        {
            get { return _totalWords; }
        }

        public Category(string cat, ExcludedWords excluded)
        {
            MPhrases = new SortedDictionary<string, PhraseCount>();
            _mExcluded = excluded;
            _mName = cat;
        }

        public int GetPhraseCount(string phrase)
        {
            PhraseCount pc;
            if (MPhrases.TryGetValue(phrase, out pc))
                return pc.Count;
            else
                return 0;
        }

        public void Reset()
        {
            _totalWords = 0;
            MPhrases.Clear();
        }

        System.Collections.Generic.SortedDictionary<string, PhraseCount> Phrases
        {
            get { return MPhrases; }
        }

        public void TeachCategory(System.IO.TextReader reader)
        {
            Regex re = new Regex(@"(\w+)\W*", RegexOptions.Compiled);
            string line;
            while (null != (line = reader.ReadLine()))
            {
                Match m = re.Match(line);
                while (m.Success)
                {
                    string word = m.Groups[1].Value;
                    //ÖÐÎÄ
                    char[] cws = word.ToCharArray();
                    foreach (char c in cws)
                    {
                        TeachPhrase(c.ToString());
                    }
                    m = m.NextMatch();
                }
            }
            reader.Close();
        }

        public void TeachPhrases(string[] words)
        {
            foreach (string word in words)
            {
                TeachPhrase(word);
            }
        }

        public void TeachPhrase(string rawPhrase)
        {
            if ((null != _mExcluded) && (_mExcluded.IsExcluded(rawPhrase)))
                return;

            PhraseCount pc;
            string Phrase = DePhrase(rawPhrase);
            if (!MPhrases.TryGetValue(Phrase, out pc))
            {
                pc = new PhraseCount(rawPhrase);
                MPhrases.Add(Phrase, pc);
            }
            pc.Count++;
            _totalWords++;
        }

        static Regex ms_PhraseRegEx = new Regex(@"\W", RegexOptions.Compiled);

        public static bool CheckIsPhrase(string s)
        {
            return ms_PhraseRegEx.IsMatch(s);
        }


        public static string DePhrase(string s)
        {
            return ms_PhraseRegEx.Replace(s, @"");
        }

    }
}
