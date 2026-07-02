using System;
using System.Configuration;

namespace JpgVerifyTool
{
    /// <summary>
    /// 统一配置访问类
    /// 开发调试时：UseEmbeddedConfig = false，从 App.config 读取
    /// 发布时：    UseEmbeddedConfig = true， 使用内嵌默认值，无需 .config 文件
    /// </summary>
    public static class AppConfig
    {
        // ============================================================
        // ⭐ 发布前改成 true，重新编译；开发调试时保持 false
        // ============================================================
        private const bool UseEmbeddedConfig = true;

        // ============================================================
        // 内嵌配置（发布时的默认值，修改后需重新编译）
        // ============================================================
        private static class Embedded
        {
            public const string AppName = "JPG 文件校验工具";
            public const string AppVersion = "1.0.1";
            public const string Author = "Automation Engineering";
            public const string LogRootFolder = "Log";
            public const int PidColumnIndex = 1;
            public const int StepColumnIndex = 2;
            public const int DataStartRow = 2;
            public const string Developer = "Michael Xiang";
            public const string Company = "Aligntech@Automation";
            public const string Copyright = "2026";
            public const string FooterTemplate =
                "© {Copyright} {Company}  |  Developed by {Developer}  |  v{Version}";
        }

        // ============================================================
        // 公开属性（代码里统一从这里读配置，不直接用 ConfigurationManager）
        // ============================================================
        public static string AppName => Get("AppName", Embedded.AppName);
        public static string AppVersion => Get("AppVersion", Embedded.AppVersion);
        public static string Author => Get("Author", Embedded.Author);
        public static string LogRootFolder => Get("LogRootFolder", Embedded.LogRootFolder);
        public static int PidColumnIndex => GetInt("PidColumnIndex", Embedded.PidColumnIndex);
        public static int StepColumnIndex => GetInt("StepColumnIndex", Embedded.StepColumnIndex);
        public static int DataStartRow => GetInt("DataStartRow", Embedded.DataStartRow);
        public static string Developer => Get("Developer", Embedded.Developer);
        public static string Company => Get("Company", Embedded.Company);
        public static string Copyright => Get("Copyright", Embedded.Copyright);
        public static string FooterTemplate => Get("FooterTemplate", Embedded.FooterTemplate);

        // ============================================================
        // 内部辅助方法
        // ============================================================
        private static string Get(string key, string defaultValue)
        {
            if (UseEmbeddedConfig) return defaultValue;
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private static int GetInt(string key, int defaultValue)
        {
            if (UseEmbeddedConfig) return defaultValue;
            var value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
    }
}