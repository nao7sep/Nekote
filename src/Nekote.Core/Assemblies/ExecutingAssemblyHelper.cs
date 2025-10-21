using System;
using System.Reflection;

namespace Nekote.Core.Assemblies
{
    /// <summary>
    /// 実行中アセンブリへの静的アクセスを提供します。
    /// </summary>
    public static class ExecutingAssemblyHelper
    {
        private static readonly Lazy<AssemblyWrapper> WrapperInstance =
            new Lazy<AssemblyWrapper>(() => new AssemblyWrapper(Assembly.GetExecutingAssembly()));

        /// <summary>
        /// 現在のコードを実行しているアセンブリの完全な AssemblyWrapper インスタンスを取得します。
        /// </summary>
        public static AssemblyWrapper Info => WrapperInstance.Value;

        /// <summary>
        /// 実行中アセンブリが null でないかどうかを示す値を取得します。
        /// </summary>
        public static bool Exists => Info.Exists;

        /// <summary>
        /// 実行中アセンブリがロードされたファイルの完全なパスまたは UNC の場所を取得します。
        /// </summary>
        public static string? Location => Info.Location;

        /// <summary>
        /// 実行中アセンブリが存在するディレクトリのパスを取得します。
        /// </summary>
        public static string? DirectoryPath => Info.DirectoryPath;

        /// <summary>
        /// 実行中アセンブリのディレクトリを基準にして、相対パスを絶対パスに変換します。
        /// </summary>
        /// <param name="relativePath">変換する相対パス。</param>
        /// <returns>解決された絶対パス。</returns>
        /// <exception cref="InvalidOperationException">アセンブリが存在しないか、ディレクトリパスを決定できない場合にスローされます。</exception>
        /// <exception cref="ArgumentNullException">`relativePath` が null または空の場合にスローされます。</exception>
        /// <exception cref="ArgumentException">`relativePath` が絶対パスの場合にスローされます。</exception>
        public static string GetAbsolutePath(string relativePath) => Info.GetAbsolutePath(relativePath);

        /// <summary>
        /// 指定された書式に基づいて、実行中アセンブリの文字列表現を取得します。
        /// </summary>
        /// <param name="format">使用する表示書式。</param>
        /// <returns>書式設定されたアセンブリの文字列表現。</returns>
        /// <exception cref="ArgumentOutOfRangeException">format で指定された表示書式が定義されていません。</exception>
        public static string? ToString(AssemblyDisplayFormat format) => Info.ToString(format);
    }
}
