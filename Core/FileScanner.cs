using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JpgVerifyTool.Core
{
    /// <summary>
    /// 负责扫描文件夹中的 JPG 文件
    /// </summary>
    public class FileScanner
    {
        /// <summary>
        /// 扫描文件夹，返回所有 JPG 文件的完整路径列表
        /// </summary>
        /// <param name="folderPath">目标文件夹路径</param>
        /// <param name="recursive">是否递归扫描子文件夹</param>
        public List<string> Scan(string folderPath, bool recursive)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"文件夹不存在: {folderPath}");

            var option = recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            // 筛选 .jpg 和 .jpeg，不区分大小写
            return Directory.EnumerateFiles(folderPath, "*.*", option)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToLower();
                    return ext == ".jpg" || ext == ".jpeg";
                })
                .ToList();
        }
    }
}