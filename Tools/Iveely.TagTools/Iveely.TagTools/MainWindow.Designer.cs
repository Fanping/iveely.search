namespace Iveely.TagTools
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.人工标记ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.疑问句标记ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.疑问句成分标记ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.疑问句准确度分析ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.信息抽取标记ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.基本信息抽取ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.抽取率分析ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Content = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.人工标记ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(617, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.文件ToolStripMenuItem.Text = "文件";
            // 
            // 人工标记ToolStripMenuItem
            // 
            this.人工标记ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.疑问句标记ToolStripMenuItem,
            this.信息抽取标记ToolStripMenuItem});
            this.人工标记ToolStripMenuItem.Name = "人工标记ToolStripMenuItem";
            this.人工标记ToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.人工标记ToolStripMenuItem.Text = "人工标记";
            // 
            // 疑问句标记ToolStripMenuItem
            // 
            this.疑问句标记ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.疑问句成分标记ToolStripMenuItem,
            this.疑问句准确度分析ToolStripMenuItem});
            this.疑问句标记ToolStripMenuItem.Name = "疑问句标记ToolStripMenuItem";
            this.疑问句标记ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.疑问句标记ToolStripMenuItem.Text = "疑问句标记";
            // 
            // 疑问句成分标记ToolStripMenuItem
            // 
            this.疑问句成分标记ToolStripMenuItem.Name = "疑问句成分标记ToolStripMenuItem";
            this.疑问句成分标记ToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.疑问句成分标记ToolStripMenuItem.Text = "疑问句成分标记";
            this.疑问句成分标记ToolStripMenuItem.Click += new System.EventHandler(this.疑问句成分标记ToolStripMenuItem_Click);
            // 
            // 疑问句准确度分析ToolStripMenuItem
            // 
            this.疑问句准确度分析ToolStripMenuItem.Name = "疑问句准确度分析ToolStripMenuItem";
            this.疑问句准确度分析ToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.疑问句准确度分析ToolStripMenuItem.Text = "疑问句准确度分析";
            // 
            // 信息抽取标记ToolStripMenuItem
            // 
            this.信息抽取标记ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.基本信息抽取ToolStripMenuItem,
            this.抽取率分析ToolStripMenuItem});
            this.信息抽取标记ToolStripMenuItem.Name = "信息抽取标记ToolStripMenuItem";
            this.信息抽取标记ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.信息抽取标记ToolStripMenuItem.Text = "信息抽取标记";
            // 
            // 基本信息抽取ToolStripMenuItem
            // 
            this.基本信息抽取ToolStripMenuItem.Name = "基本信息抽取ToolStripMenuItem";
            this.基本信息抽取ToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.基本信息抽取ToolStripMenuItem.Text = "基本信息抽取";
            // 
            // 抽取率分析ToolStripMenuItem
            // 
            this.抽取率分析ToolStripMenuItem.Name = "抽取率分析ToolStripMenuItem";
            this.抽取率分析ToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.抽取率分析ToolStripMenuItem.Text = "抽取率分析";
            // 
            // Content
            // 
            this.Content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Content.Location = new System.Drawing.Point(0, 24);
            this.Content.Name = "Content";
            this.Content.Size = new System.Drawing.Size(617, 306);
            this.Content.TabIndex = 1;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 330);
            this.Controls.Add(this.Content);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "Iveely - 人工标记工具";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 人工标记ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 疑问句标记ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 疑问句成分标记ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 疑问句准确度分析ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 信息抽取标记ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 基本信息抽取ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 抽取率分析ToolStripMenuItem;
        private System.Windows.Forms.Panel Content;
    }
}

