using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.NLP
{
    public class Feeling
    {
        private static Hashtable _corpus;

        public Feeling(string filePath="")
        {
            string serFile = "Init\\Feeling.ser";
            if (_corpus == null && !File.Exists(serFile))
            {
                _corpus = new Hashtable();
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] keywords = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (keywords.Length == 2)
                    {
                        if (!_corpus.Contains(keywords[0].Trim()))
                        {
                            _corpus[keywords[0].Trim()] = double.Parse(keywords[1].Trim());
                        }
                    }
                }
                Text.Serializer.SerializeToFile(_corpus, serFile);
            }
            else if (_corpus == null && File.Exists(serFile))
            {
                _corpus = Text.Serializer.DeserializeFromFile<Hashtable>(serFile);
            }

        }

        public double GetScore(string[] keywords)
        {
            double score = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (_corpus.Contains(keywords[i]))
                {
                    score += (double)_corpus[keywords[i]];
                }
            }
            return score;
        }
    }
}
