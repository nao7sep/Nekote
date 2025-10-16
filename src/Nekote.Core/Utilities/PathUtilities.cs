using System.IO;

namespace Nekote.Core.Utilities
{
    /// <summary>
    /// パス操作に関する静的ユーティリティメソッドを提供します。
    /// </summary>
    public static class PathUtilities
    {
        /// <summary>
        /// パス文字列内のパス区切り文字を、現在の環境のプライマリ区切り文字に正規化します。
        /// </summary>
        /// <param name="path">正規化するパス文字列。</param>
        /// <returns>正規化されたパス文字列。入力が null または空の場合は、入力をそのまま返します。</returns>
        /// <example>
        /// Windows 環境では `some/path` は `some\\path` になります。
        /// Linux 環境では `some\\path` は `some/path` になります。
        /// </example>
        public static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
