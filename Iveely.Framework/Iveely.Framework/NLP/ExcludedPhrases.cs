

using System;
using System.Collections.Generic;

namespace Iveely.Framework.NLP
{
    [Serializable]
	class ExcludedWords  
	{
		static readonly string[] EnuMostCommon =
		{
			 "the", 
			 "to", 
			 "and", 
			 "a", 
			 "an", 
			 "in", 
			 "is", 
			 "it", 
			 "you", 
			 "that", 
			 "was", 
			 "for", 
			 "on", 
			 "are", 
			 "with", 
			 "as", 
			 "be", 
			 "been", 
			 "at", 
			 "one", 
			 "have", 
			 "this", 
			 "what", 
			 "which", 
		};

	   private readonly Dictionary<string, int> _mDict;

		public ExcludedWords()
		{
			_mDict = new Dictionary<string, int>();
		}

		public void InitDefault()
		{
			Init(EnuMostCommon);
		}
		public void Init(string[] excluded)
		{
			_mDict.Clear();
			for (int i = 0; i < excluded.Length; i++)
			{
				_mDict.Add(excluded[i], i);
			}
		}

		public bool IsExcluded(string word)
		{
			return _mDict.ContainsKey(word);
		}

	}
}
