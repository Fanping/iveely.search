using System;

namespace Iveely.Framework.NLP
{
    [Serializable]
    class PhraseCount
    {

        public PhraseCount(string rawPhrase)
        {
            this.Count = 0;
            this.RawPhrase = rawPhrase;
        }

        public string RawPhrase { get; private set; }

        public int Count
        {
            get; set;
        }
    }
}
