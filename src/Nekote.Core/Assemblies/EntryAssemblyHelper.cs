using System;
using System.Reflection;

namespace Nekote.Core.Assemblies
{
    /// <summary>
    /// エントリアセンブリへの静的アクセスを提供します。
    /// </summary>
    public static class EntryAssemblyHelper
    {
        private static readonly Lazy<AssemblyWrapper> WrapperInstance =
            new Lazy<AssemblyWrapper>(() => new AssemblyWrapper(Assembly.GetEntryAssembly()));

        /// <summary>
        /// エントリアセンブリの完全な AssemblyWrapper インスタンスを取得します。
        /// </summary>
        public static AssemblyWrapper Info => WrapperInstance.Value;

        /// <summary>
        /// エントリアセンブリが null でないかどうかを示す値を取得します。
        /// </summary>
        public static bool Exists => Info.Exists;

        /// <summary>
        /// エントリアセンブリがロードされたファイルの完全なパスまたは UNC の場所を取得します。
        /// </summary>
        public static string? Location => Info.Location;

        /// <summary>
        /// エントリアセンブリが存在するディレクトリのパスを取得します。
        /// </summary>
        public static string? DirectoryPath => Info.DirectoryPath;

        /// <summary>
        /// エントリアセンブリのディレクトリを基準にして、相対パスを絶対パスに変換します。
        /// </summary>
        /// <param name="relativePath">変換する相対パス。</param>
        /// <returns>解決された絶対パス。</returns>
        /// <exception cref="InvalidOperationException">アセンブリが存在しないか、ディレクトリパスを決定できない場合にスローされます。</exception>
        /// <exception cref="ArgumentNullException">`relativePath` が null または空の場合にスローされます。</exception>
        /// <exception cref="ArgumentException">`relativePath` が絶対パスの場合にスローされます。</exception>
        public static string GetAbsolutePath(string relativePath) => Info.GetAbsolutePath(relativePath);

        /// <summary>
        /// 指定された書式に基づいて、エントリアセンブリの文字列表現を取得します。
        /// </summary>
        /// <param name="format">使用する表示書式。</param>
        /// <returns>書式設定されたアセンブリの文字列表現。</returns>
        /// <exception cref="ArgumentOutOfRangeException">format で指定された表示書式が定義されていません。</exception>
        public static string? ToString(AssemblyDisplayFormat format) => Info.ToString(format);
    }
}
