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

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="text">需要分词的文本</param>
        /// <param name="delimeter">分词后的分隔符</param>
        /// <returns></returns>
        public string Split(string text, string delimeter = "/")
        {
            string result = model.Split(text, delimeter);
            return result;
        }

        /// <summary>
        /// 带词性的分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
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
