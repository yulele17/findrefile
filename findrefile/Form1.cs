using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace findrefile
{
    public partial class Form1 : Form
    {
        // 主数据：所有“大小有相同兄弟”的文件
        private readonly List<HashFile> _allFiles = new List<HashFile>();
        // 网格视图绑定的行集合
        private readonly BindingList<DuplicateRow> _rows = new BindingList<DuplicateRow>();
        // 后台扫描线程，避免界面卡死
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        // 后台删除线程，删除时显示进度、避免界面卡死
        private readonly BackgroundWorker _deleteWorker = new BackgroundWorker();
        // 在 UI 线程读取到的“永久删除”选项，避免后台线程跨线程读控件
        private bool _deletePermanent = false;
        // 当前排序列与方向（点列头排序用）
        private string _sortColumn = null;
        private bool _sortAscending = true;
        private bool _dragging = false;
        private System.Drawing.Point _dragOffset = System.Drawing.Point.Empty;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.DataSource = _rows;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            _deleteWorker.WorkerReportsProgress = true;
            _deleteWorker.DoWork += DeleteWorker_DoWork;
            _deleteWorker.ProgressChanged += DeleteWorker_ProgressChanged;
            _deleteWorker.RunWorkerCompleted += DeleteWorker_RunWorkerCompleted;

            // 数据绑定异常兜底：格式化/取值出错时不再抛出到 UI 线程导致崩溃。
            dataGridView1.DataError += (s, ev) => { ev.Cancel = true; };

            // —— 无边框窗口：自定义标题栏拖动与窗口按钮 ——
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            headerPanel.MouseDown += Title_MouseDown;
            headerPanel.MouseMove += Title_MouseMove;
            headerPanel.MouseUp += Title_MouseUp;
            lblTitle.MouseDown += Title_MouseDown;
            lblTitle.MouseMove += Title_MouseMove;
            lblTitle.MouseUp += Title_MouseUp;
            btnMin.Click += btnMin_Click;
            btnMax.Click += btnMax_Click;
            btnClose.Click += btnClose_Click;
            btnMin.MouseEnter += WinBtn_Enter; btnMin.MouseLeave += WinBtn_Leave;
            btnMax.MouseEnter += WinBtn_Enter; btnMax.MouseLeave += WinBtn_Leave;
            btnClose.MouseEnter += WinBtn_Enter; btnClose.MouseLeave += WinBtn_Leave;

            // 窗口图标（任务栏、标题栏、Alt+Tab 都用这个）
            try { this.Icon = new System.Drawing.Icon("app.ico"); }
            catch { /* 图标缺失时保持默认，不影响程序运行 */ }

            // 扁平现代工具栏
            toolStrip1.Renderer = new FlatToolStripRenderer();

            // 表格精致化（覆盖设计器默认，统一为微软雅黑 + 现代配色）
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(214, 228, 255);
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(20, 20, 20);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(244, 247, 251);
            dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridView1.GridColor = System.Drawing.Color.FromArgb(230, 234, 240);
            dataGridView1.RowTemplate.Height = 26;
            // 表头样式（关闭默认视觉样式后需手动设置，否则不生效）
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 125, 210);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
            // 点击列头排序
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(200, 225, 245);
        }

        // ===================== 扫描 =====================
        private void buttonScan_Click(object sender, EventArgs e)
        {
            if (_worker.IsBusy)
            {
                MessageBox.Show("正在扫描中，请稍候。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;

            string root = folderBrowserDialog1.SelectedPath;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "正在扫描：" + root;

            // 扫描期间：允许取消、禁用开始
            toolCancel.Enabled = true;
            toolScan.Enabled = false;
            _worker.RunWorkerAsync(root);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
                lblStatus.Text = "正在取消…";
                toolCancel.Enabled = false;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string root = (string)e.Argument;
            var detector = new DuplicateDetector(
                status: msg => _worker.ReportProgress(0, msg));
            var result = detector.Scan(root, () => _worker.CancellationPending);

            // 用户取消则标记，避免把半成品结果当正常完成
            if (_worker.CancellationPending)
                e.Cancel = true;
            else
                e.Result = result;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string msg)
                lblStatus.Text = msg;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;
            toolCancel.Enabled = false;
            toolScan.Enabled = true;

            if (e.Cancelled)
            {
                lblStatus.Text = "已取消扫描。";
                return;
            }

            if (e.Error != null)
            {
                lblStatus.Text = "扫描出错：" + e.Error.Message;
                MessageBox.Show("扫描过程中发生错误：\n" + e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scanned = (List<HashFile>)e.Result;
            // 按路径去重合并，方便多次扫描不同目录
            var existing = new HashSet<string>(_allFiles.Select(f => f.Path), StringComparer.OrdinalIgnoreCase);
            int added = 0;
            foreach (var f in scanned)
            {
                if (!existing.Contains(f.Path))
                {
                    _allFiles.Add(f);
                    existing.Add(f.Path);
                    added++;
                }
            }
            lblStatus.Text = $"扫描完成：本批新增 {added} 个疑似重复文件，累计 {_allFiles.Count} 个。";
            BuildDuplicateRows();
        }

        // ===================== 计算并展示重复分组 =====================
        private void BuildDuplicateRows()
        {
            _sortColumn = null;
            dataGridView1.SuspendLayout();
            try
            {
                _rows.RaiseListChangedEvents = false;
                _rows.Clear();

                var groups = _allFiles
                    .GroupBy(f => f.Hash, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1);

                int groupCount = 0;
                long waste = 0;
                foreach (var g in groups)
                {
                    groupCount++;
                    var list = g.ToList();
                    // 每组保留第一个（默认不勾选），其余默认勾选“删除”
                    for (int i = 0; i < list.Count; i++)
                    {
                        _rows.Add(new DuplicateRow
                        {
                            MarkedForDelete = i > 0,
                            Path = list[i].Path,
                            Size = list[i].Size,
                            Hash = list[i].Hash
                        });
                    }
                    // 可释放空间 = (组内数量 - 1) * 单文件大小
                    waste += (list.Count - 1) * list[0].Size;
                }

                _rows.RaiseListChangedEvents = true;
                _rows.ResetBindings();

                int marked = _rows.Count(r => r.MarkedForDelete);
                lblStatus.Text = string.Format("重复分组：{0} 组，已勾选删除 {1} 个，预计可释放 {2}。",
                    groupCount, marked, FormatSize(waste));
            }
            finally
            {
                foreach (System.Windows.Forms.DataGridViewColumn c in dataGridView1.Columns)
                    c.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
                dataGridView1.ResumeLayout(true);
            }
        }

        // ===================== 列头点击排序 =====================
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var col = dataGridView1.Columns[e.ColumnIndex];
            string prop = col.DataPropertyName;
            if (string.IsNullOrEmpty(prop)) return;

            // 同一列再次点击则切换升/降序，否则按该列升序开始
            if (_sortColumn == prop)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = prop;
                _sortAscending = true;
            }

            // BindingList 不支持内置排序，这里手动排序后重建
            List<DuplicateRow> list = _rows.ToList();
            if (prop == "Size")
                list.Sort((a, b) => _sortAscending ? a.Size.CompareTo(b.Size) : b.Size.CompareTo(a.Size));
            else if (prop == "Path")
                list.Sort((a, b) => _sortAscending
                    ? string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase)
                    : string.Compare(b.Path, a.Path, StringComparison.OrdinalIgnoreCase));
            else if (prop == "MarkedForDelete")
                list.Sort((a, b) => _sortAscending ? a.MarkedForDelete.CompareTo(b.MarkedForDelete) : b.MarkedForDelete.CompareTo(a.MarkedForDelete));
            else
                return; // 隐藏列（如 Hash）不参与排序

            dataGridView1.SuspendLayout();
            _rows.RaiseListChangedEvents = false;
            _rows.Clear();
            foreach (var r in list) _rows.Add(r);
            _rows.RaiseListChangedEvents = true;
            _rows.ResetBindings();
            dataGridView1.ResumeLayout(true);

            // 在列头显示排序箭头
            foreach (System.Windows.Forms.DataGridViewColumn c in dataGridView1.Columns)
                c.HeaderCell.SortGlyphDirection = (c.DataPropertyName == _sortColumn)
                    ? (_sortAscending ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.Descending)
                    : System.Windows.Forms.SortOrder.None;
        }

        // ===================== 删除 =====================
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (_deleteWorker.IsBusy)
            {
                MessageBox.Show("正在删除中，请稍候。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var toDelete = _rows.Where(r => r.MarkedForDelete && File.Exists(r.Path)).ToList();
            if (toDelete.Count == 0)
            {
                MessageBox.Show("没有勾选要删除的文件（或文件已不存在）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            long totalSize = toDelete.Sum(r => r.Size);
            string tip = toolPermanent.Checked
                ? "将【永久删除】，不可恢复！"
                : "将移入回收站，可从回收站恢复。";
            string msg = $"即将删除 {toDelete.Count} 个文件（共 {FormatSize(totalSize)}）。\n{tip}\n\n确定继续？";
            if (MessageBox.Show(msg, "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            // 把待删除列表交给后台线程执行，显示进度
            _deletePermanent = toolPermanent.Checked; // 先在 UI 线程读好
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            toolDelete.Enabled = false;
            _deleteWorker.RunWorkerAsync(toDelete);
        }

        private void DeleteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var toDelete = (List<DuplicateRow>)e.Argument;
            bool permanent = _deletePermanent;
            int total = toDelete.Count;
            int ok = 0, fail = 0;
            var failed = new List<string>();
            var deletedPaths = new List<string>();

            for (int i = 0; i < total; i++)
            {
                var row = toDelete[i];
                // 实时汇报进度：第几个、共几个、当前文件名
                _deleteWorker.ReportProgress(
                    (int)((i * 100.0) / total),
                    string.Format("正在删除 {0}/{1}：{2}", i + 1, total,
                        System.IO.Path.GetFileName(row.Path)));
                try
                {
                    if (permanent)
                        File.Delete(row.Path);
                    else
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                            row.Path,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                    deletedPaths.Add(row.Path);
                    ok++;
                }
                catch (Exception ex)
                {
                    fail++;
                    failed.Add(row.Path + "  （" + ex.Message + "）");
                }
            }

            _deleteWorker.ReportProgress(100, "正在整理结果…");
            e.Result = new DeleteResult { Ok = ok, Fail = fail, Failed = failed, DeletedPaths = deletedPaths };
        }

        private void DeleteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
                progressBar.Value = Math.Min(e.ProgressPercentage, 100);
            if (e.UserState is string text)
                lblStatus.Text = text;
        }

        private void DeleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolDelete.Enabled = true;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Blocks;

            if (e.Error != null)
            {
                lblStatus.Text = "删除出错：" + e.Error.Message;
                MessageBox.Show("删除过程中发生错误：\n" + e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var r = (DeleteResult)e.Result;
            // 同步主数据，避免出现“幽灵”记录
            foreach (var p in r.DeletedPaths)
                _allFiles.RemoveAll(f => f.Path.Equals(p, StringComparison.OrdinalIgnoreCase));

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("成功删除 {0} 个文件。", r.Ok));
            if (r.Fail > 0)
            {
                sb.AppendLine(string.Format("{0} 个文件删除失败：", r.Fail));
                sb.AppendLine(string.Join("\n", r.Failed));
            }
            MessageBox.Show(sb.ToString(), "删除结果");

            // 重新计算剩余重复（有些组删除后可能已不再重复）
            BuildDuplicateRows();
        }

        private class DeleteResult
        {
            public int Ok;
            public int Fail;
            public List<string> Failed;
            public List<string> DeletedPaths;
        }

        // ===================== 辅助 =====================
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // 缩放/重绘时可能会对表头（RowIndex < 0）或无效列（ColumnIndex < 0）触发格式化，必须跳过。
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null)
                return;

            if (dataGridView1.Columns[e.ColumnIndex] != null &&
                dataGridView1.Columns[e.ColumnIndex].Name == "colSize" &&
                e.Value is long size)
            {
                e.Value = FormatSize(size);
                e.FormattingApplied = true;
            }
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            double v = bytes;
            string[] units = { "KB", "MB", "GB", "TB" };
            int i = -1;
            do { v /= 1024; i++; } while (v >= 1024 && i < units.Length - 1);
            return $"{v:0.##} {units[i]}";
        }

        // ===================== 窗口控制（无边框自定义标题栏） =====================
        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    // 从最大化状态拖出：先还原，并以鼠标为顶边中心定位
                    this.WindowState = FormWindowState.Normal;
                    _dragOffset = new System.Drawing.Point(this.Width / 2, 0);
                }
                else
                {
                    _dragOffset = e.Location;
                }
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

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnMax.Text = "▢";
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                btnMax.Text = "▣";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void WinBtn_Enter(object sender, EventArgs e)
        {
            var b = (System.Windows.Forms.Button)sender;
            b.BackColor = (b == btnClose)
                ? System.Drawing.Color.FromArgb(232, 17, 35)   // 关闭按钮悬停变红
                : System.Drawing.Color.FromArgb(26, 95, 208);  // 其余悬停变浅蓝
        }

        private void WinBtn_Leave(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Button)sender).BackColor = System.Drawing.Color.FromArgb(45, 125, 249);
        }

        // 扁平工具栏渲染：去掉老气渐变，改为白底 + 悬停浅色
        private class FlatToolStripRenderer : System.Windows.Forms.ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e)
            {
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
            protected override void OnRenderButtonBackground(System.Windows.Forms.ToolStripItemRenderEventArgs e)
            {
                System.Drawing.Rectangle r = new System.Drawing.Rectangle(0, 0, e.Item.Width, e.Item.Height);
                System.Drawing.Color c = System.Drawing.Color.White;
                if (e.Item.Pressed) c = System.Drawing.Color.FromArgb(224, 234, 250);
                else if (e.Item.Selected) c = System.Drawing.Color.FromArgb(240, 245, 252);
                using (var brush = new System.Drawing.SolidBrush(c))
                    e.Graphics.FillRectangle(brush, r);
            }
            protected override void OnRenderSeparator(System.Windows.Forms.ToolStripSeparatorRenderEventArgs e)
            {
                int x = e.Item.Bounds.Width / 2;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(225, 225, 225)))
                    e.Graphics.DrawLine(pen, x, 8, x, e.Item.Bounds.Height - 8);
            }
        }

        // 无边框窗口添加系统阴影
        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }
    }

    /// <summary>
    /// 网格中的一行：对应一个重复文件，含“是否删除”勾选。
    /// </summary>
    public class DuplicateRow
    {
        public bool MarkedForDelete { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
    }
}
