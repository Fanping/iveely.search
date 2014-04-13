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
            string result = model.Split(text);
            return result;
        }

        public Tuple<string[], string[]> SplitToArray(string sentence)
        {
            return null;
        }

    }
}
