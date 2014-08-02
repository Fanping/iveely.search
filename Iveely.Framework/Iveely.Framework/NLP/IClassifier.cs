using System.Collections.Generic;

namespace Iveely.Framework.NLP
{
    #region Interfaces
    /// <summary>
    /// Classifier methods.
    /// </summary>
    public interface IClassifier
    {
        void TeachPhrases(string cat, string[] phrases);

        void TeachCategory(string cat, System.IO.TextReader tr);

        Dictionary<string, double> Classify(string text);
    }

    interface ICategory
    {
        string Name { get; }

        void Reset();

        int GetPhraseCount(string phrase);

        void TeachCategory(System.IO.TextReader reader);

        void TeachPhrase(string rawPhrase);

        void TeachPhrases(string[] words);

        int TotalWords { get; }
    }
    #endregion
}
