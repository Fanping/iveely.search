using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Iveely.TagTools
{
    public partial class QuestionTag : UserControl
    {
        public QuestionTag()
        {
            InitializeComponent();
        }

        private List<string> GetQuestionStyle(string fileName)
        {
            List<string> result = new List<string>();
            string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] text = line.Split(new[] { "  ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length != 3)
                {
                    result.Add(line);
                    if (result.Count == 10)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        private void QuestionTag_Load(object sender, EventArgs e)
        {
            List<string> questiones = GetQuestionStyle("Corpus_Question_Style.txt");
            for (int i = 0; i < questiones.Count; i++)
            {
                QuestionInfo questionInfo = new QuestionInfo();
                questionInfo.Location = new Point(10, 70 * i);
                questionInfo.SetQuestion((i + 1), questiones[i]);
                this.Controls.Add(questionInfo);
            }
        }
    }
}
