using System.Collections.Generic;

namespace JpgVerifyTool.Models
{
    /// <summary>
    /// 表示某条 PID 记录的匹配结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>PID 号</summary>
        public string Pid { get; set; }

        /// <summary>步骤号</summary>
        public string Step { get; set; }

        /// <summary>匹配到的文件名列表（仅文件名，不含路径）</summary>
        public List<string> MatchedFiles { get; set; } = new List<string>();

        /// <summary>是否存在（至少匹配到一个文件）</summary>
        public bool Exists => MatchedFiles.Count > 0;

        /// <summary>匹配数量</summary>
        public int Count => MatchedFiles.Count;
    }
}