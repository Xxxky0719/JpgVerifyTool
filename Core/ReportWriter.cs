using JpgVerifyTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JpgVerifyTool.Core
{
    /// <summary>
    /// 负责生成 Summary.csv、Details.csv 和 run.log
    /// </summary>
    public class ReportWriter
    {
        // UTF-8 with BOM，保证 CSV 用 Excel 打开时中文不乱码
        private static readonly Encoding Utf8Bom = new UTF8Encoding(true);

        /// <summary>
        /// 生成本次运行的完整报告，返回报告文件夹路径
        /// </summary>
        public string WriteReports(
            string logRootFolder,
            string excelFilePath,
            string jpgFolderPath,
            bool recursive,
            int totalExcelRows,
            int duplicateCount,
            int totalJpgFiles,
            List<MatchResult> results,
            TimeSpan elapsed)
        {
            // 创建本次运行的子文件夹：{Excel文件名}_{yyyyMMdd_HHmmss}
            string excelName = Path.GetFileNameWithoutExtension(excelFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string runFolder = Path.Combine(logRootFolder, $"{excelName}_{timestamp}");
            Directory.CreateDirectory(runFolder);

            WriteSummary(Path.Combine(runFolder, "Summary.csv"), results);
            WriteDetails(Path.Combine(runFolder, "Details.csv"), results);
            WriteRunLog(Path.Combine(runFolder, "run.log"),
                          excelFilePath, jpgFolderPath, recursive,
                          totalExcelRows, duplicateCount, totalJpgFiles,
                          results, elapsed);

            return runFolder;
        }

        // ── Summary.csv：每条 PID 记录一行 ──────────────────────────
        private void WriteSummary(string path, List<MatchResult> results)
        {
            using (var sw = new StreamWriter(path, false, Utf8Bom))
            {
                sw.WriteLine("PID,Step,Exists,Count");
                foreach (var r in results)
                    sw.WriteLine($"{Esc(r.Pid)},{Esc(r.Step)},{(r.Exists ? "Y" : "N")},{r.Count}");
            }
        }

        // ── Details.csv：每个匹配文件一行 ───────────────────────────
        private void WriteDetails(string path, List<MatchResult> results)
        {
            using (var sw = new StreamWriter(path, false, Utf8Bom))
            {
                sw.WriteLine("PID,Step,FileName");
                foreach (var r in results)
                {
                    if (r.MatchedFiles.Count == 0)
                    {
                        // 缺失的记录也写一行，方便查找
                        sw.WriteLine($"{Esc(r.Pid)},{Esc(r.Step)},(NOT FOUND)");
                    }
                    else
                    {
                        foreach (var file in r.MatchedFiles)
                            sw.WriteLine($"{Esc(r.Pid)},{Esc(r.Step)},{Esc(file)}");
                    }
                }
            }
        }

        // ── run.log：本次运行的元信息 ────────────────────────────────
        private void WriteRunLog(
            string path,
            string excelFilePath, string jpgFolderPath, bool recursive,
            int totalExcelRows, int duplicateCount, int totalJpgFiles,
            List<MatchResult> results, TimeSpan elapsed)
        {
            int existsCount = 0, missingCount = 0, totalMatched = 0;
            foreach (var r in results)
            {
                if (r.Exists) existsCount++; else missingCount++;
                totalMatched += r.Count;
            }

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"执行时间    : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Excel 文件  : {excelFilePath}");
            sb.AppendLine($"图片文件夹  : {jpgFolderPath}");
            sb.AppendLine($"递归扫描    : {(recursive ? "是" : "否")}");
            sb.AppendLine("========================================");
            sb.AppendLine($"Excel 记录总数    : {totalExcelRows}");
            sb.AppendLine($"重复记录数        : {duplicateCount}（已自动去重）");
            sb.AppendLine($"去重后有效记录    : {totalExcelRows - duplicateCount}");
            sb.AppendLine($"扫描到 JPG 文件数 : {totalJpgFiles}");
            sb.AppendLine("========================================");
            sb.AppendLine($"匹配存在  : {existsCount} 条");
            sb.AppendLine($"匹配缺失  : {missingCount} 条");
            sb.AppendLine($"匹配文件总数 : {totalMatched} 个");
            sb.AppendLine("========================================");
            sb.AppendLine($"耗时 : {elapsed.TotalSeconds:F2} 秒");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Utf8Bom);
        }

        // ── CSV 字段转义 ─────────────────────────────────────────────
        private string Esc(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            return field;
        }
    }
}