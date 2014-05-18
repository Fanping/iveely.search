using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Iveely.TagTools
{
    public partial class QuestionInfo : UserControl
    {
        public QuestionInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 传入的文具
        /// </summary>
        private string _question;

        /// <summary>
        /// 编号
        /// </summary>
        private int _id;

        private List<string> _semantics;

        public void SetQuestion(int id, string question)
        {
            this._question = question;
            this._id = id;
        }

        /// <summary>
        /// 初始化控件显示
        /// </summary>
        private void Init()
        {
            //取出需要标记的问题
            this._semantics = new List<string>();
            string[] text = this._question.Split(new[] { "  ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            string[] vals =
            text[0].Split(new[] { "/", " " }, StringSplitOptions.RemoveEmptyEntries);

            //绘制控件
            int x = this.Parent.Location.X;
            for (int i = 0; i < vals.Length; i++)
            {
                if (i % 2 == 0)
                {
                    Label label = new Label
                    {
                        Name = "lable" + i,
                        Text = vals[i],
                        Tag = i,
                        Width = vals[i].Length * 20,
                        Location = new Point(x, this.Parent.Location.Y)

                    };

                    //if (i == 0)
                    //{
                    //    label.Text = this._id + ".:" + label.Text;
                    //}

                    this.Controls.Add(label);

                    ComboBox box = new ComboBox();
                    box.Name = "box" + i;
                    box.Tag = i;
                    box.Location = new Point(x + label.Text.Length * 20, this.Parent.Location.Y);
                    box.Items.Add("实体1");
                    box.Items.Add("实体2");
                    box.Items.Add("关系");
                    box.Width = 60;
                    box.Text = "忽略";
                    this.Controls.Add(box);

                    x = x + label.Text.Length * 20 + 60;
                }
                else
                {
                    this._semantics.Add(vals[i]);
                }

            }



        }

        private void QuestionInfo_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void TagButton_Click(object sender, EventArgs e)
        {

        }
    }
}
