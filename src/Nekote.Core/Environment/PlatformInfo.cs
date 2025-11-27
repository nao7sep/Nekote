using System;
using System.Runtime.InteropServices;

namespace Nekote.Core.Environment
{
    /// <summary>
    /// プラットフォーム固有の情報と動作を提供します。
    /// </summary>
    public static class PlatformInfo
    {
        // ========================================
        // オペレーティングシステムの検出
        // ========================================

        /// <summary>
        /// 現在のオペレーティングシステムが Windows かどうかを示す値を取得します。
        /// </summary>
        public static bool IsWindows { get; } = OperatingSystem.IsWindows();

        /// <summary>
        /// 現在のオペレーティングシステムが Linux かどうかを示す値を取得します。
        /// </summary>
        public static bool IsLinux { get; } = OperatingSystem.IsLinux();

        /// <summary>
        /// 現在のオペレーティングシステムが macOS かどうかを示す値を取得します。
        /// </summary>
        public static bool IsMacOS { get; } = OperatingSystem.IsMacOS();

        /// <summary>
        /// 現在のオペレーティングシステムが FreeBSD かどうかを示す値を取得します。
        /// </summary>
        public static bool IsFreeBSD { get; } = OperatingSystem.IsFreeBSD();

        /// <summary>
        /// 現在のオペレーティングシステムが Android かどうかを示す値を取得します。
        /// </summary>
        public static bool IsAndroid { get; } = OperatingSystem.IsAndroid();

        /// <summary>
        /// 現在のオペレーティングシステムが iOS または iPadOS かどうかを示す値を取得します。
        /// </summary>
        public static bool IsIOS { get; } = OperatingSystem.IsIOS();

        /// <summary>
        /// 現在のオペレーティングシステムが Unix 系（Windows 以外）かどうかを示す値を取得します。
        /// </summary>
        /// <remarks>
        /// これには Linux、macOS、FreeBSD、Android、iOS などが含まれます。
        /// </remarks>
        public static bool IsUnixLike { get; } = !IsWindows;

        // ========================================
        // ファイルシステムの特性
        // ========================================

        /// <summary>
        /// 現在のプラットフォームのファイルシステムが大文字と小文字を区別するかどうかを示す値を取得します。
        /// </summary>
        /// <remarks>
        /// Windows では false、Unix 系システムでは true を返します。
        /// macOS は既定では大文字と小文字を区別しませんが（APFS または HFS+ の既定設定）、
        /// ボリュームを大文字と小文字を区別する形式でフォーマットすることも可能です。
        /// ただし、.NET フレームワーク自体が macOS を Unix 系として扱い、大文字と小文字を区別する前提で動作するため、
        /// このプロパティも同様の動作を採用しています。
        /// </remarks>
        public static bool IsFileSystemCaseSensitive { get; } = IsUnixLike;

        /// <summary>
        /// 現在のプラットフォームのパス比較に使用するべき <see cref="StringComparison"/> を取得します。
        /// </summary>
        public static StringComparison PathComparison { get; } =
            IsFileSystemCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// 現在のプラットフォームの改行シーケンスを取得します。
        /// </summary>
        /// <remarks>
        /// Windows では "\r\n" (CRLF)、Unix 系システムでは "\n" (LF) を返します。
        /// このプロパティは <see cref="System.Environment.NewLine"/> と同じ値を返しますが、
        /// プラットフォーム固有の情報を一元管理するために提供されています。
        /// </remarks>
        public static string NewLine { get; } = System.Environment.NewLine;

        // ========================================
        // パス区切り文字
        // ========================================

        /// <summary>
        /// Windows のパス区切り文字 (バックスラッシュ) を取得します。
        /// </summary>
        public static char WindowsDirectorySeparator { get; } = '\\';

        /// <summary>
        /// Unix 系システムのパス区切り文字 (スラッシュ) を取得します。
        /// </summary>
        public static char UnixDirectorySeparator { get; } = '/';

        /// <summary>
        /// 現在のプラットフォームのパス区切り文字を取得します。
        /// </summary>
        /// <remarks>
        /// Windows では '\\'、Unix 系システムでは '/' を返します。
        /// </remarks>
        public static char DirectorySeparator { get; } =
            IsWindows ? WindowsDirectorySeparator : UnixDirectorySeparator;

        /// <summary>
        /// 現在のプラットフォームの代替パス区切り文字を取得します。
        /// </summary>
        /// <remarks>
        /// Windows では '/'、Unix 系システムでは '\\' を返します。
        /// 注: <see cref="Path.AltDirectorySeparatorChar"/> は全てのプラットフォームで '/' を返すため、
        /// プラットフォーム間でパス文字列を正しく正規化するには、このプロパティを使用する必要があります。
        /// </remarks>
        public static char AltDirectorySeparator { get; } =
            IsWindows ? UnixDirectorySeparator : WindowsDirectorySeparator;

        // ========================================
        // アーキテクチャ
        // ========================================

        /// <summary>
        /// 現在のプロセスが 64 ビットかどうかを示す値を取得します。
        /// </summary>
        public static bool Is64BitProcess { get; } = System.Environment.Is64BitProcess;

        /// <summary>
        /// 現在のオペレーティングシステムが 64 ビットかどうかを示す値を取得します。
        /// </summary>
        public static bool Is64BitOperatingSystem { get; } = System.Environment.Is64BitOperatingSystem;

        /// <summary>
        /// 現在のプロセスのアーキテクチャを取得します。
        /// </summary>
        public static Architecture ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture;

        /// <summary>
        /// 現在のオペレーティングシステムのアーキテクチャを取得します。
        /// </summary>
        public static Architecture OSArchitecture { get; } = RuntimeInformation.OSArchitecture;

        // ========================================
        // ランタイム情報
        // ========================================

        /// <summary>
        /// オペレーティングシステムの説明を取得します。
        /// </summary>
        public static string OSDescription { get; } = RuntimeInformation.OSDescription;

        /// <summary>
        /// ランタイム識別子 (RID) を取得します。
        /// </summary>
        public static string RuntimeIdentifier { get; } = RuntimeInformation.RuntimeIdentifier;

        /// <summary>
        /// .NET ランタイムのフレームワーク名を取得します。
        /// </summary>
        public static string FrameworkDescription { get; } = RuntimeInformation.FrameworkDescription;
    }
}
