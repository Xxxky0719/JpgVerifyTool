namespace JpgVerifyTool.Models
{
    /// <summary>
    /// 表示 Excel 中的一条 PID 记录
    /// </summary>
    public class PidRecord
    {
        /// <summary>PID 号</summary>
        public string Pid { get; set; }

        /// <summary>步骤号</summary>
        public string Step { get; set; }

        /// <summary>原始行号（用于错误提示）</summary>
        public int RowNumber { get; set; }

        /// <summary>去重用的唯一键</summary>
        public string Key => $"{Pid}|{Step}";

        public override string ToString() => $"PID={Pid}, Step={Step}";
    }
}