using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Text.Segment
{
    public class MetastasisSegment
    {
        private static MetastasisModel model;

        public MetastasisSegment()
        {
            string serializedFile = "Init\\MetastasisModel.ser";
            if (model == null && File.Exists(serializedFile))
            {
                model = Serializer.DeserializeFromFile<MetastasisModel>(serializedFile);
            }
            else if (model == null)
            {
                model = new MetastasisModel();
                model.Learn("Init\\2014_Segment_Corpus\\");
                Serializer.SerializeToFile(model, serializedFile);
            }
        }

        public string Split(string text, string delimeter = "/")
        {
            string result = model.Split(text, delimeter);
            return result;
        }

        public Tuple<string[], string[]> SplitToArray(string sentence)
        {
            return model.SplitToArray(sentence);
        }

        public string SplitWithSemantic(string sentence, string delimeter = " ")
        {
            Tuple<string[], string[]> tuple = model.SplitToArray(sentence);
            string str = "";
            for (int i = 0; i < tuple.Item1.Length; i++)
            {
                str += tuple.Item1[i] + "/" + tuple.Item2[i] + delimeter;
            }
            return str.TrimEnd();
        }

    }
}
