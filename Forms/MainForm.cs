using JpgVerifyTool.Core;
using JpgVerifyTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JpgVerifyTool.Forms
{
    public partial class MainForm : Form
    {
        private readonly string _appName;
        private readonly string _appVersion;
        private readonly string _logRootFolder;
        private readonly int _pidColumn;
        private readonly int _stepColumn;
        private readonly int _dataStartRow;
        private string _lastLogRoot = "";  // 记录上次实际生成报告的路径

        public MainForm()
        {
            InitializeComponent();

            // 从统一配置类读取所有配置
            _appName = AppConfig.AppName;
            _appVersion = AppConfig.AppVersion;
            _logRootFolder = AppConfig.LogRootFolder;
            _pidColumn = AppConfig.PidColumnIndex;
            _stepColumn = AppConfig.StepColumnIndex;
            _dataStartRow = AppConfig.DataStartRow;

            // 窗口标题
            this.Text = $"{_appName}  v{_appVersion}";

            // 渲染底部开发者信息
            lblFooter.Text = AppConfig.FooterTemplate
                .Replace("{Developer}", AppConfig.Developer)
                .Replace("{Company}", AppConfig.Company)
                .Replace("{Copyright}", AppConfig.Copyright)
                .Replace("{Version}", _appVersion);
        }

        // ── 选择 Excel 文件 ──────────────────────────────────────────
        private void btnSelectExcel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                dlg.Title = "选择 CSV 文件";
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtExcelPath.Text = dlg.FileName;
            }
        }

        // ── 选择 JPG 文件夹 ──────────────────────────────────────────
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "选择 JPG 文件夹（选择文件夹内任意一个文件即可）";
                dlg.Filter = "所有文件 (*.*)|*.*";
                dlg.CheckFileExists = false;  // 不检查文件是否存在
                dlg.FileName = "选择此文件夹";  // 提示用户操作方式

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // 取选中文件所在的文件夹路径
                    txtFolderPath.Text = Path.GetDirectoryName(dlg.FileName);
                }
            }
        }

        // ── 开始校验 ─────────────────────────────────────────────────
        private async void btnRun_Click(object sender, EventArgs e)
        {
            // 输入验证
            //if (string.IsNullOrWhiteSpace(txtExcelPath.Text) ||
            //    !File.Exists(txtExcelPath.Text))
            //{
            //    MessageBox.Show("请选择有效的 Excel 文件", "提示",
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            //if (string.IsNullOrWhiteSpace(txtFolderPath.Text) ||
            //    !Directory.Exists(txtFolderPath.Text))
            //{
            //    MessageBox.Show("请选择有效的 JPG 文件夹", "提示",
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            // 去掉首尾空格，兼容用户手动输入时不小心多打的空格
            string excelInput = txtExcelPath.Text.Trim();
            string folderInput = txtFolderPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(excelInput))
            {
                MessageBox.Show("请输入或选择 Excel 文件路径", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(excelInput))
            {
                MessageBox.Show($"Excel 文件不存在，请确认路径是否正确：\n{excelInput}",
                    "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(folderInput))
            {
                MessageBox.Show("请输入或选择 JPG 文件夹路径", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(folderInput))
            {
                MessageBox.Show($"文件夹不存在，请确认路径是否正确：\n{folderInput}",
                    "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 把处理后的路径写回文本框（去掉多余空格）
            txtExcelPath.Text = excelInput;
            txtFolderPath.Text = folderInput;

            btnRun.Enabled = false;
            txtLog.Clear();

            try
            {
                // Task.Run 把耗时操作放到后台线程，防止 UI 卡死
                await Task.Run(() => RunVerification());
            }
            catch (Exception ex)
            {
                AppendLog($"[错误] {ex.Message}");
                MessageBox.Show("运行出错：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRun.Enabled = true;
            }
        }

        // ── 核心校验流程（后台线程执行）─────────────────────────────
        private void RunVerification()
        {
            var sw = Stopwatch.StartNew();

            // 使用 Trim 后的路径，防止手动输入时首尾有空格导致路径识别失败
            string excelPath = txtExcelPath.Text.Trim();
            string folderPath = txtFolderPath.Text.Trim();
            bool recursive = chkRecursive.Checked;

            AppendLog("========== 开始校验 ==========");
            AppendLog($"Excel  : {excelPath}");
            AppendLog($"文件夹 : {folderPath}");
            AppendLog($"递归   : {(recursive ? "是" : "否")}");
            AppendLog("");

            // 1. 读取 Excel
            AppendLog("[1/4] 读取 Excel...");
            var reader = new ExcelReader(_pidColumn, _stepColumn, _dataStartRow);
            var rawRecords = reader.Read(excelPath);
            AppendLog($"  原始记录: {rawRecords.Count} 条");

            var records = ExcelReader.Deduplicate(rawRecords, out int dupCount);
            AppendLog($"  重复记录: {dupCount} 条（已去重）");
            AppendLog($"  有效记录: {records.Count} 条");

            if (dupCount > 0)
            {
                Invoke((Action)(() =>
                    MessageBox.Show(
                        $"检测到 {dupCount} 条重复记录，已自动去重。\n请检查 Excel 数据。",
                        "重复记录提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)));
            }

            // 2. 扫描 JPG
            AppendLog("");
            AppendLog("[2/4] 扫描 JPG 文件...");
            var scanner = new FileScanner();
            var jpgFiles = scanner.Scan(folderPath, recursive);
            AppendLog($"  扫描到: {jpgFiles.Count} 个 JPG 文件");

            // 3. 匹配
            AppendLog("");
            AppendLog("[3/4] 匹配中...");
            var matcher = new Matcher();
            var results = matcher.Match(records, jpgFiles);

            int exists = 0, missing = 0;
            foreach (var r in results)
            {
                if (r.Exists) exists++; else missing++;
            }
            AppendLog($"  存在: {exists} 条");
            AppendLog($"  缺失: {missing} 条");

            // 4. 生成报告
            AppendLog("");
            AppendLog("[4/4] 生成报告...");
            sw.Stop();

            string logRoot = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, _logRootFolder);

            // 保存本次实际使用的 Log 根目录，供按钮使用
            _lastLogRoot = logRoot;

            var writer = new ReportWriter();
            string reportFolder = writer.WriteReports(
                logRoot, excelPath, folderPath, recursive,
                rawRecords.Count, dupCount, jpgFiles.Count,
                results, sw.Elapsed);

            // ⭐ 直接用报告文件夹的上级目录作为 Log 根目录
            _lastLogRoot = Path.GetDirectoryName(reportFolder);

            AppendLog($"  报告路径: {reportFolder}");
            AppendLog("");
            AppendLog("========== 校验完成 ==========");

            // 完成弹窗
            Invoke((Action)(() =>
            {
                MessageBox.Show(
                $"校验完成！\n\n✔ 存在: {exists} 条\n✘ 缺失: {missing} 条",
                "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        // ── 打开 Log 文件夹 ──────────────────────────────────────────
        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            // 跑过校验就打开上次的 Log 根目录
            // 没跑过就用 exe 所在目录下的 Log 文件夹
            string exeDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            string logRoot = !string.IsNullOrEmpty(_lastLogRoot)
                ? _lastLogRoot
                : Path.Combine(exeDir, _logRootFolder);

            if (!Directory.Exists(logRoot))
                Directory.CreateDirectory(logRoot);

            Process.Start("explorer.exe", logRoot);
        }

        // ── 线程安全地追加日志 ───────────────────────────────────────
        private void AppendLog(string message)
        {
            // InvokeRequired：判断当前是否在非 UI 线程
            // 如果是后台线程调用，需要通过 Invoke 切换到 UI 线程再操作控件
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((Action)(() => AppendLog(message)));
                return;
            }
            txtLog.AppendText(message + Environment.NewLine);
        }

        private void lblExcel_Click(object sender, EventArgs e)
        {

        }
    }
}