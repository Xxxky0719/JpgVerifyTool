using JpgVerifyTool.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JpgVerifyTool.Core
{
    /// <summary>
    /// 负责从 CSV 文件读取 PID 记录
    /// CSV 格式要求：第一行为表头，第一列 PID，第二列 Step
    /// 支持 UTF-8、UTF-8 BOM、GBK 等常见编码
    /// </summary>
    public class ExcelReader
    {
        private readonly int _pidColumn;
        private readonly int _stepColumn;
        private readonly int _startRow;

        public ExcelReader(int pidColumn, int stepColumn, int startRow)
        {
            _pidColumn = pidColumn;
            _stepColumn = stepColumn;
            _startRow = startRow;
        }

        /// <summary>
        /// 读取 CSV 文件，返回原始记录列表（未去重）
        /// </summary>
        public List<PidRecord> Read(string csvPath)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV 文件不存在", csvPath);

            var records = new List<PidRecord>();

            // 自动检测编码，兼容 UTF-8 BOM 和 GBK（中文 Windows 默认编码）
            var encoding = DetectEncoding(csvPath);
            var lines = File.ReadAllLines(csvPath, encoding);

            // _startRow 从 1 开始计数，转换为数组下标需要 -1
            // 例：DataStartRow=2 表示跳过第 1 行表头，从第 2 行开始读
            for (int i = _startRow - 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 跳过空行
                if (string.IsNullOrEmpty(line)) continue;

                // 解析 CSV 行（正确处理带引号的字段）
                var columns = ParseCsvLine(line);

                // 列数不足，跳过
                if (columns.Count < 2) continue;

                // 列索引从 1 开始，转换为数组下标需要 -1
                var pid = columns[_pidColumn - 1].Trim();
                var step = columns[_stepColumn - 1].Trim();

                // 任意一列为空，跳过
                if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(step))
                    continue;

                records.Add(new PidRecord
                {
                    Pid = pid,
                    Step = step,
                    RowNumber = i + 1  // 记录实际行号（从 1 开始）
                });
            }

            return records;
        }

        /// <summary>
        /// 去重，返回去重后的列表和重复数量
        /// </summary>
        public static List<PidRecord> Deduplicate(
            List<PidRecord> records, out int duplicateCount)
        {
            var seen = new HashSet<string>();
            var result = new List<PidRecord>();

            foreach (var r in records)
            {
                // HashSet.Add 返回 true 表示新值，false 表示重复
                if (seen.Add(r.Key))
                    result.Add(r);
            }

            duplicateCount = records.Count - result.Count;
            return result;
        }

        /// <summary>
        /// 自动检测文件编码
        /// 优先检测 UTF-8 BOM，否则使用系统默认编码（中文 Windows 为 GBK）
        /// </summary>
        private static Encoding DetectEncoding(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var bom = new byte[3];
                fs.Read(bom, 0, 3);

                // UTF-8 BOM 标识：EF BB BF
                if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    return new UTF8Encoding(true);
            }

            // 没有 BOM，使用系统默认编码（兼容中文 Windows 的 GBK）
            return Encoding.Default;
        }

        /// <summary>
        /// 解析 CSV 行，正确处理带双引号的字段
        /// 例："hello, world","test" → ["hello, world", "test"]
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 连续两个引号 "" 表示转义的引号字符
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // 跳过下一个引号
                        }
                        else
                        {
                            inQuotes = false; // 引号结束
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true; // 引号开始
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            // 添加最后一个字段
            fields.Add(current.ToString());
            return fields;
        }
    }
}