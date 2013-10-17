using System;
using System.Collections.Generic;


namespace Iveely.CloudComputting.Client
{
    public sealed class Example : Application
    {
        public override void Run(object[] args)
        {
            string content = ReadText("news.txt", args);
            string[] words = content.Split(new[] { ' ', '"', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
            DiagnosticsWrite("local words count:" + words.Length, args);
            int globalWordCount = Mathematics.Sum<int>(words.Length, args);
            DiagnosticsWrite("global words count:" + globalWordCount, args);

        }
    }
}
