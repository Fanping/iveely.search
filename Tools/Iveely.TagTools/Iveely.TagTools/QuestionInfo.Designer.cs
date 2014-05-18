namespace Iveely.TagTools
{
    partial class QuestionInfo
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.TagButton = new System.Windows.Forms.Button();
            this.CanCelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TagButton
            // 
            this.TagButton.Location = new System.Drawing.Point(684, 3);
            this.TagButton.Name = "TagButton";
            this.TagButton.Size = new System.Drawing.Size(75, 23);
            this.TagButton.TabIndex = 0;
            this.TagButton.Text = "确  认";
            this.TagButton.UseVisualStyleBackColor = true;
            this.TagButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // CanCelButton
            // 
            this.CanCelButton.Location = new System.Drawing.Point(765, 3);
            this.CanCelButton.Name = "CanCelButton";
            this.CanCelButton.Size = new System.Drawing.Size(75, 23);
            this.CanCelButton.TabIndex = 1;
            this.CanCelButton.Text = "重标此条";
            this.CanCelButton.UseVisualStyleBackColor = true;
            // 
            // QuestionInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CanCelButton);
            this.Controls.Add(this.TagButton);
            this.Name = "QuestionInfo";
            this.Size = new System.Drawing.Size(853, 43);
            this.Load += new System.EventHandler(this.QuestionInfo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button TagButton;
        private System.Windows.Forms.Button CanCelButton;
    }
}
