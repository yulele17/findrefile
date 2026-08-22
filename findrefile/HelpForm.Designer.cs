namespace findrefile
{
    partial class HelpForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.headerPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblIntro = new System.Windows.Forms.Label();
            this.picQR = new System.Windows.Forms.PictureBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblUsage = new System.Windows.Forms.Label();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).BeginInit();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(125)))), ((int)(((byte)(249)))));
            this.headerPanel.Controls.Add(this.btnClose);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(520, 46);
            this.headerPanel.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(125)))), ((int)(((byte)(249)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(478, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(42, 46);
            this.btnClose.TabIndex = 1;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(14, 11);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(120, 24);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "帮助与关于";
            // 
            // lblIntro
            // 
            this.lblIntro.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.lblIntro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(80)))));
            this.lblIntro.Location = new System.Drawing.Point(28, 64);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new System.Drawing.Size(464, 46);
            this.lblIntro.TabIndex = 1;
            this.lblIntro.Text = "findrefile 是一款免费的 Windows 重复文件查找工具，基于 MD5 精准比对，帮助您快速清理磁盘冗余文件。";
            // 
            // picQR
            // 
            this.picQR.BackColor = System.Drawing.Color.White;
            this.picQR.Location = new System.Drawing.Point(131, 124);
            this.picQR.Name = "picQR";
            this.picQR.Size = new System.Drawing.Size(258, 258);
            this.picQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picQR.TabIndex = 2;
            this.picQR.TabStop = false;
            // 
            // lblHint
            // 
            this.lblHint.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(125)))), ((int)(((byte)(249)))));
            this.lblHint.Location = new System.Drawing.Point(28, 394);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(464, 24);
            this.lblHint.TabIndex = 3;
            this.lblHint.Text = "扫码关注微信公众号";
            this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(125)))), ((int)(((byte)(249)))));
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(196, 596);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(128, 38);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "关  闭";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblUsage
            // 
            this.lblUsage.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F);
            this.lblUsage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(115)))), ((int)(((byte)(130)))));
            this.lblUsage.Location = new System.Drawing.Point(28, 432);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(464, 150);
            this.lblUsage.TabIndex = 4;
            this.lblUsage.Text = "使用步骤：\n1. 点击「扫描」选择要检查的文件夹；\n2. 扫描完成后，表格会列出重复文件，每组默认保留第一个、其余勾选；\n3. 确认无误后点击「删除选中」，文件将移入回收站（可勾选「永久删除」）；\n4. 删除进度会显示在底部状态栏。\n\n开源地址：github.com/yulele17/findrefile";
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(520, 660);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblUsage);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.picQR);
            this.Controls.Add(this.lblIntro);
            this.Controls.Add(this.headerPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HelpForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "帮助与关于";
            this.Load += new System.EventHandler(this.HelpForm_Load);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblIntro;
        private System.Windows.Forms.PictureBox picQR;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblUsage;
        private System.Windows.Forms.Button btnOK;
    }
}
