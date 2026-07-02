using JpgVerifyTool.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace JpgVerifyTool.Core
{
    /// <summary>
    /// 负责将 PID 记录与 JPG 文件进行匹配
    ///
    /// 匹配规则：_{PID}[UL]0*{Step}N
    ///   例：PID=80471796, Step=3 → 正则 _80471796[UL]0*3N
    ///   - PID 前必须是下划线，防止误匹配更长的 PID
    ///   - PID 后必须是 U 或 L，防止 PID 后面还有数字
    ///   - Step 前允许任意个前导零（0*），所以 Excel 里的 3 能匹配文件里的 03
    ///   - Step 后必须是 N，防止 3 匹配到 33、30、13 等
    /// </summary>
    public class Matcher
    {
        public List<MatchResult> Match(
            List<PidRecord> records, List<string> jpgFiles)
        {
            var results = new List<MatchResult>();

            // 一次性提取所有文件名（不含路径），避免重复调用 Path.GetFileName
            var fileNames = new List<(string FullPath, string FileName)>();
            foreach (var path in jpgFiles)
                fileNames.Add((path, Path.GetFileName(path)));

            foreach (var record in records)
            {
                var result = new MatchResult
                {
                    Pid = record.Pid,
                    Step = record.Step
                };

                // Regex.Escape 防止 PID/Step 里含正则特殊字符（如 . + * 等）
                string pattern = $"_{Regex.Escape(record.Pid)}[UL]0*{Regex.Escape(record.Step)}N";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);

                foreach (var f in fileNames)
                {
                    if (regex.IsMatch(f.FileName))
                        result.MatchedFiles.Add(f.FileName);
                }

                results.Add(result);
            }

            return results;
        }
    }
}