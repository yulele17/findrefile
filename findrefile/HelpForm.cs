using System;
using System.IO;
using System.Windows.Forms;

namespace findrefile
{
    public partial class HelpForm : Form
    {
        private bool _dragging = false;
        private System.Drawing.Point _dragOffset = System.Drawing.Point.Empty;

        public HelpForm()
        {
            InitializeComponent();
            try { this.Icon = new System.Drawing.Icon("app.ico"); }
            catch { /* 图标缺失时保持默认 */ }

            // 无边框窗口拖动
            headerPanel.MouseDown += Title_MouseDown;
            headerPanel.MouseMove += Title_MouseMove;
            headerPanel.MouseUp += Title_MouseUp;
            lblTitle.MouseDown += Title_MouseDown;
            lblTitle.MouseMove += Title_MouseMove;
            lblTitle.MouseUp += Title_MouseUp;

            // 关闭按钮悬停效果
            btnClose.MouseEnter += (s, ev) => btnClose.BackColor = System.Drawing.Color.FromArgb(232, 17, 35);
            btnClose.MouseLeave += (s, ev) => btnClose.BackColor = System.Drawing.Color.FromArgb(45, 125, 249);
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            string qrPath = Path.Combine(Application.StartupPath, "Resources", "wechat_qrcode.jpg");
            if (File.Exists(qrPath))
            {
                picQR.Image = System.Drawing.Image.FromFile(qrPath);
            }
            else
            {
                lblHint.Text = "二维码文件缺失：" + qrPath;
            }
        }

        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragOffset = e.Location;
                _dragging = true;
            }
        }

        private void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                System.Drawing.Point p = this.PointToScreen(e.Location);
                this.Location = new System.Drawing.Point(p.X - _dragOffset.X, p.Y - _dragOffset.Y);
            }
        }

        private void Title_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
