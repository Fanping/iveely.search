using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Iveely.TagTools
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void 疑问句成分标记ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QuestionTag tag = new QuestionTag();
            tag.Dock = DockStyle.Fill;
            this.Content.Controls.Add(tag);

            //QuestionInfo questionInfo = new QuestionInfo();
            //questionInfo.SetQuestion("国民党/n 主席/n 是/vshi 谁/ry	0,1,2	Who");
            //this.Content.Controls.Add(questionInfo);
          
        }
    }
}
