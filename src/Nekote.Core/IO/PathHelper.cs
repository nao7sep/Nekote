using System;
using System.IO;

namespace Nekote.Core.IO
{
    /// <summary>
    /// パス操作に関する静的ユーティリティメソッドを提供します。
    /// </summary>
    public static class PathHelper
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

        /// <summary>
        /// アプリケーションのベースディレクトリを基準にして、相対パスを絶対パスに解決します。
        /// </summary>
        /// <param name="relativePath">変換する相対パス。</param>
        /// <returns>解決された絶対パス。</returns>
        /// <exception cref="ArgumentException">relativePath が null、空、または絶対パスの場合にスローされます。</exception>
        public static string MapPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or whitespace.", nameof(relativePath));
            }

            if (Path.IsPathFullyQualified(relativePath))
            {
                throw new ArgumentException("Path must be relative, not absolute.", nameof(relativePath));
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        }
    }
}
