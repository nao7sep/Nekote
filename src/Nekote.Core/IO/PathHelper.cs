using System;
using System.IO;
using Nekote.Core.Environment;

namespace Nekote.Core.IO
{
    /// <summary>
    /// パス操作に関する静的ユーティリティメソッドを提供します。
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 指定されたパスがベースディレクトリ内に収まることを確認します。
        /// </summary>
        /// <param name="basePath">ベースディレクトリのパス。</param>
        /// <param name="targetPath">検証する対象のパス。</param>
        /// <returns>対象のパスがベースディレクトリ内にある場合は true、それ以外は false。</returns>
        /// <remarks>
        /// このメソッドは、両方のパスを絶対パスに正規化してから比較します。
        /// </remarks>
        public static bool IsPathWithinBase(string basePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Base path cannot be null or whitespace.", nameof(basePath));
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException("Target path cannot be null or whitespace.", nameof(targetPath));
            }

            if (!Path.IsPathFullyQualified(basePath))
            {
                throw new ArgumentException("Base path must be absolute, not relative.", nameof(basePath));
            }

            if (!Path.IsPathFullyQualified(targetPath))
            {
                throw new ArgumentException("Target path must be absolute, not relative.", nameof(targetPath));
            }

            var fullBasePath = Path.GetFullPath(basePath);
            var fullTargetPath = Path.GetFullPath(targetPath);

            return fullTargetPath.StartsWith(fullBasePath, PlatformInfo.PathComparison);
        }

        /// <summary>
        /// 指定されたパスがベースディレクトリ内に収まることを確認し、
        /// 収まらない場合は例外をスローします。
        /// </summary>
        /// <param name="basePath">ベースディレクトリのパス。</param>
        /// <param name="targetPath">検証する対象のパス。</param>
        /// <exception cref="ArgumentException">パスがベースディレクトリ外に出る場合にスローされます。</exception>
        public static void EnsurePathWithinBase(string basePath, string targetPath)
        {
            if (!IsPathWithinBase(basePath, targetPath))
            {
                throw new ArgumentException($"The path '{targetPath}' is outside the base directory '{basePath}'.", nameof(targetPath));
            }
        }

        /// <summary>
        /// パス文字列内のパス区切り文字を、現在の環境のプライマリ区切り文字に正規化します。
        /// </summary>
        /// <param name="path">正規化するパス文字列。</param>
        /// <returns>正規化されたパス文字列。</returns>
        /// <exception cref="ArgumentException">path が null、空、または空白文字のみの場合にスローされます。</exception>
        /// <remarks>
        /// 注: <see cref="Path.GetFullPath"/> を使用する場合、このメソッドを呼び出す必要はありません。
        /// <see cref="Path.GetFullPath"/> は自動的にパス区切り文字を正規化します。
        /// </remarks>
        /// <example>
        /// Windows 環境では `some/path` は `some\\path` になります。
        /// Linux 環境では `some\\path` は `some/path` になります。
        /// </example>
        public static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            return path.Replace(PlatformInfo.AltDirectorySeparator, PlatformInfo.DirectorySeparator);
        }

        /// <summary>
        /// パス文字列内のパス区切り文字を、指定されたプラットフォームの区切り文字に正規化します。
        /// </summary>
        /// <param name="path">正規化するパス文字列。</param>
        /// <param name="useWindowsSeparator">true の場合は Windows 形式（\\）、false の場合は Unix 形式（/）に正規化します。</param>
        /// <returns>正規化されたパス文字列。</returns>
        /// <exception cref="ArgumentException">path が null、空、または空白文字のみの場合にスローされます。</exception>
        /// <remarks>
        /// 注: <see cref="Path.GetFullPath"/> を使用する場合、現在のプラットフォームの形式に正規化するだけであれば、このメソッドを呼び出す必要はありません。
        /// このメソッドは、特定のプラットフォーム形式を強制する必要がある場合（例: URL や設定ファイル用に Unix 形式を使用）に使用してください。
        /// </remarks>
        public static string NormalizeDirectorySeparators(string path, bool useWindowsSeparator)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            char targetSeparator = useWindowsSeparator
                ? PlatformInfo.WindowsDirectorySeparator
                : PlatformInfo.UnixDirectorySeparator;

            char sourceSeparator = useWindowsSeparator
                ? PlatformInfo.UnixDirectorySeparator
                : PlatformInfo.WindowsDirectorySeparator;

            return path.Replace(sourceSeparator, targetSeparator);
        }

        /// <summary>
        /// 複数のパス文字列を1つのパスに結合します。
        /// </summary>
        /// <param name="basePath">ベースとなるパス。</param>
        /// <param name="relativePath">結合する相対パス。</param>
        /// <param name="ensureWithinBase">結合されたパスがベースパス内に収まることを検証するかどうか。</param>
        /// <returns>結合された絶対パス。</returns>
        /// <exception cref="ArgumentException">パスがベースディレクトリ外に出る場合（ensureWithinBase が true の場合）にスローされます。</exception>
        /// <remarks>
        /// このメソッドは内部で <see cref="Path.GetFullPath"/> を呼び出すため、呼び出し元で再度正規化する必要はありません。
        /// </remarks>
        public static string Combine(string basePath, string relativePath, bool ensureWithinBase = false)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Base path cannot be null or whitespace.", nameof(basePath));
            }

            if (!Path.IsPathFullyQualified(basePath))
            {
                throw new ArgumentException("Base path must be absolute, not relative.", nameof(basePath));
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or whitespace.", nameof(relativePath));
            }

            if (Path.IsPathFullyQualified(relativePath))
            {
                throw new ArgumentException("Relative path must be relative, not absolute.", nameof(relativePath));
            }

            var absolutePath = Path.GetFullPath(Path.Combine(basePath, relativePath));

            if (ensureWithinBase)
            {
                EnsurePathWithinBase(basePath, absolutePath);
            }

            return absolutePath;
        }

        /// <summary>
        /// 複数のパス文字列を1つのパスに結合します。
        /// </summary>
        /// <param name="basePath">ベースとなるパス。</param>
        /// <param name="path1">結合する最初の相対パス。</param>
        /// <param name="path2">結合する2番目の相対パス。</param>
        /// <param name="ensureWithinBase">結合されたパスがベースパス内に収まることを検証するかどうか。</param>
        /// <returns>結合された絶対パス。</returns>
        /// <exception cref="ArgumentException">パスがベースディレクトリ外に出る場合（ensureWithinBase が true の場合）にスローされます。</exception>
        /// <remarks>
        /// このメソッドは内部で <see cref="Path.GetFullPath"/> を呼び出すため、呼び出し元で再度正規化する必要はありません。
        /// </remarks>
        public static string Combine(string basePath, string path1, string path2, bool ensureWithinBase = false)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Base path cannot be null or whitespace.", nameof(basePath));
            }

            if (!Path.IsPathFullyQualified(basePath))
            {
                throw new ArgumentException("Base path must be absolute, not relative.", nameof(basePath));
            }

            if (string.IsNullOrWhiteSpace(path1) && string.IsNullOrWhiteSpace(path2))
            {
                throw new ArgumentException("At least one relative path must have a meaningful value.");
            }

            if (!string.IsNullOrWhiteSpace(path1) && Path.IsPathFullyQualified(path1))
            {
                throw new ArgumentException("Path1 must be relative, not absolute.", nameof(path1));
            }

            if (!string.IsNullOrWhiteSpace(path2) && Path.IsPathFullyQualified(path2))
            {
                throw new ArgumentException("Path2 must be relative, not absolute.", nameof(path2));
            }

            var absolutePath = Path.GetFullPath(Path.Combine(basePath, path1, path2));

            if (ensureWithinBase)
            {
                EnsurePathWithinBase(basePath, absolutePath);
            }

            return absolutePath;
        }

        /// <summary>
        /// 複数のパス文字列を1つのパスに結合します。
        /// </summary>
        /// <param name="basePath">ベースとなるパス。</param>
        /// <param name="path1">結合する最初の相対パス。</param>
        /// <param name="path2">結合する2番目の相対パス。</param>
        /// <param name="path3">結合する3番目の相対パス。</param>
        /// <param name="ensureWithinBase">結合されたパスがベースパス内に収まることを検証するかどうか。</param>
        /// <returns>結合された絶対パス。</returns>
        /// <exception cref="ArgumentException">パスがベースディレクトリ外に出る場合（ensureWithinBase が true の場合）にスローされます。</exception>
        /// <remarks>
        /// このメソッドは内部で <see cref="Path.GetFullPath"/> を呼び出すため、呼び出し元で再度正規化する必要はありません。
        /// </remarks>
        public static string Combine(string basePath, string path1, string path2, string path3, bool ensureWithinBase = false)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Base path cannot be null or whitespace.", nameof(basePath));
            }

            if (!Path.IsPathFullyQualified(basePath))
            {
                throw new ArgumentException("Base path must be absolute, not relative.", nameof(basePath));
            }

            if (string.IsNullOrWhiteSpace(path1) && string.IsNullOrWhiteSpace(path2) && string.IsNullOrWhiteSpace(path3))
            {
                throw new ArgumentException("At least one relative path must have a meaningful value.");
            }

            if (!string.IsNullOrWhiteSpace(path1) && Path.IsPathFullyQualified(path1))
            {
                throw new ArgumentException("Path1 must be relative, not absolute.", nameof(path1));
            }

            if (!string.IsNullOrWhiteSpace(path2) && Path.IsPathFullyQualified(path2))
            {
                throw new ArgumentException("Path2 must be relative, not absolute.", nameof(path2));
            }

            if (!string.IsNullOrWhiteSpace(path3) && Path.IsPathFullyQualified(path3))
            {
                throw new ArgumentException("Path3 must be relative, not absolute.", nameof(path3));
            }

            var absolutePath = Path.GetFullPath(Path.Combine(basePath, path1, path2, path3));

            if (ensureWithinBase)
            {
                EnsurePathWithinBase(basePath, absolutePath);
            }

            return absolutePath;
        }

        /// <summary>
        /// 複数のパス文字列を1つのパスに結合します。
        /// </summary>
        /// <param name="basePath">ベースとなるパス。</param>
        /// <param name="paths">結合する相対パスの配列。</param>
        /// <param name="ensureWithinBase">結合されたパスがベースパス内に収まることを検証するかどうか。</param>
        /// <returns>結合された絶対パス。</returns>
        /// <exception cref="ArgumentException">パスがベースディレクトリ外に出る場合（ensureWithinBase が true の場合）にスローされます。</exception>
        /// <remarks>
        /// このメソッドは内部で <see cref="Path.GetFullPath"/> を呼び出すため、呼び出し元で再度正規化する必要はありません。
        /// </remarks>
        public static string Combine(string basePath, string[] paths, bool ensureWithinBase = false)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Base path cannot be null or whitespace.", nameof(basePath));
            }

            if (!Path.IsPathFullyQualified(basePath))
            {
                throw new ArgumentException("Base path must be absolute, not relative.", nameof(basePath));
            }

            if (paths == null || paths.Length == 0)
            {
                throw new ArgumentException("Paths array cannot be null or empty.", nameof(paths));
            }

            bool hasMeaningfulPath = false;
            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    hasMeaningfulPath = true;

                    if (Path.IsPathFullyQualified(path))
                    {
                        throw new ArgumentException($"Path '{path}' must be relative, not absolute.", nameof(paths));
                    }
                }
            }

            if (!hasMeaningfulPath)
            {
                throw new ArgumentException("At least one relative path must have a meaningful value.", nameof(paths));
            }

            var allPaths = new string[paths.Length + 1];
            allPaths[0] = basePath;
            Array.Copy(paths, 0, allPaths, 1, paths.Length);

            var absolutePath = Path.GetFullPath(Path.Combine(allPaths));

            if (ensureWithinBase)
            {
                EnsurePathWithinBase(basePath, absolutePath);
            }

            return absolutePath;
        }

        /// <summary>
        /// アプリケーションのベースディレクトリを基準にして、相対パスを絶対パスに解決します。
        /// </summary>
        /// <param name="relativePath">変換する相対パス。</param>
        /// <param name="ensureWithinBase">パスがベースディレクトリ内に収まることを検証するかどうか。</param>
        /// <returns>解決された絶対パス。</returns>
        /// <exception cref="ArgumentException">relativePath が null、空、または絶対パスの場合、
        /// またはパスがベースディレクトリ外に出る場合（ensureWithinBase が true の場合）にスローされます。</exception>
        public static string MapPath(string relativePath, bool ensureWithinBase = false)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or whitespace.", nameof(relativePath));
            }

            if (Path.IsPathFullyQualified(relativePath))
            {
                throw new ArgumentException("Path must be relative, not absolute.", nameof(relativePath));
            }

            var absolutePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));

            if (ensureWithinBase)
            {
                EnsurePathWithinBase(AppContext.BaseDirectory, absolutePath);
            }

            return absolutePath;
        }
    }
}
