using Nekote.Core.IO;
using Nekote.Core.Text;
using Nekote.Core.Versioning;
using System;
using System.IO;
using System.Reflection;

namespace Nekote.Core.Assemblies
{
    /// <summary>
    /// アセンブリ情報をラップし、メタデータへのアクセスとパス操作を容易にします。
    /// </summary>
    public class AssemblyWrapper
    {
        private readonly Assembly? _assembly;

        // よく使われるパス関連のプロパティをキャッシュします。
        private readonly Lazy<string?> _location;
        private readonly Lazy<string?> _directoryPath;

        /// <summary>
        /// 指定されたアセンブリを使用して、AssemblyWrapper の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="assembly">ラップするアセンブリ。</param>
        public AssemblyWrapper(Assembly? assembly)
        {
            _assembly = assembly;

            _location = new Lazy<string?>(() =>
            {
                // バイト配列からロードされたアセンブリの場合、Locationは空文字列を返すことがあります。
                // これをnullに正規化して、呼び出し元がnullチェックだけで済むようにします。
                return StringHelper.NullIfWhiteSpace(_assembly?.Location);
            });

            _directoryPath = new Lazy<string?>(() =>
            {
                // Locationがnullまたは空の場合、ディレクトリパスもnullです。
                if (string.IsNullOrEmpty(Location))
                {
                    return null;
                }
                // Path.GetDirectoryNameは、パスにディレクトリ情報が含まれていない場合に空の文字列を返すことがあります。
                // これをnullに正規化して、呼び出し元がnullチェックだけで済むようにします。
                return StringHelper.NullIfWhiteSpace(Path.GetDirectoryName(Location));
            });
        }

        /// <summary>
        /// アセンブリが null でないかどうかを示す値を取得します。
        /// </summary>
        public bool Exists => _assembly != null;

        /// <summary>
        /// アセンブリの名前を取得します。
        /// </summary>
        public string? Name => _assembly?.GetName().Name;

        /// <summary>
        /// アセンブリのバージョンを取得します。
        /// </summary>
        public Version? Version => _assembly?.GetName().Version;

        /// <summary>
        /// アセンブリの完全名を取得します。
        /// </summary>
        public string? FullName => _assembly?.FullName;

        /// <summary>
        /// アセンブリがロードされたファイルの完全なパスまたは UNC の場所を取得します。（キャッシュされます）
        /// </summary>
        public string? Location => _location.Value;

        /// <summary>
        /// アセンブリが存在するディレクトリのパスを取得します。（キャッシュされます）
        /// </summary>
        public string? DirectoryPath => _directoryPath.Value;

        /// <summary>
        /// アセンブリの会社名を取得します。
        /// </summary>
        public string? Company => GetAssemblyAttribute<AssemblyCompanyAttribute>()?.Company;

        /// <summary>
        /// アセンブリの製品名を取得します。
        /// </summary>
        public string? Product => GetAssemblyAttribute<AssemblyProductAttribute>()?.Product;

        /// <summary>
        /// アセンブリのタイトルを取得します。
        /// </summary>
        public string? Title => GetAssemblyAttribute<AssemblyTitleAttribute>()?.Title;

        /// <summary>
        /// アセンブリの著作権情報を取得します。
        /// </summary>
        public string? Copyright => GetAssemblyAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        /// <summary>
        /// アセンブリの説明を取得します。
        /// </summary>
        public string? Description => GetAssemblyAttribute<AssemblyDescriptionAttribute>()?.Description;

        /// <summary>
        /// アセンブリのディレクトリを基準にして、相対パスを絶対パスに変換します。
        /// 注: ほとんどの場合、<see cref="PathHelper.MapPath"/> を使用することを検討してください。
        /// このメソッドは、特定のアセンブリの場所を基準にする必要がある場合にのみ使用してください。
        /// </summary>
        /// <param name="relativePath">変換する相対パス。</param>
        /// <returns>解決された絶対パス。</returns>
        /// <exception cref="InvalidOperationException">アセンブリが存在しないか、ディレクトリパスを決定できない場合にスローされます。</exception>
        /// <exception cref="ArgumentNullException">`relativePath` が null または空の場合にスローされます。</exception>
        /// <exception cref="ArgumentException">`relativePath` が絶対パスの場合にスローされます。</exception>
        public string GetAbsolutePath(string relativePath)
        {
            if (!Exists)
            {
                throw new InvalidOperationException("Assembly does not exist.");
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath), "Relative path cannot be null or whitespace.");
            }

            if (Path.IsPathFullyQualified(relativePath))
            {
                throw new ArgumentException("Input path must be relative, not absolute.", nameof(relativePath));
            }

            if (DirectoryPath == null)
            {
                throw new InvalidOperationException("Could not determine the directory path for the assembly.");
            }

            var normalizedRelativePath = PathHelper.NormalizeDirectorySeparators(relativePath);
            return Path.GetFullPath(Path.Combine(DirectoryPath, normalizedRelativePath));
        }

        /// <summary>
        /// 指定された書式に基づいて、アセンブリの文字列表現を取得します。
        /// </summary>
        /// <param name="format">使用する表示書式。</param>
        /// <returns>書式設定されたアセンブリの文字列表現。</returns>
        /// <exception cref="ArgumentOutOfRangeException">format で指定された表示書式が定義されていません。</exception>
        public string? ToString(AssemblyDisplayFormat format)
        {
            switch (format)
            {
                case AssemblyDisplayFormat.TitleAndVersion:
                    if (string.IsNullOrWhiteSpace(Title) || Version is null)
                    {
                        return null;
                    }
                    var versionString = VersionHelper.ToString(Version);
                    return $"{Title} v{versionString}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), "The specified display format is not defined.");
            }
        }

        /// <summary>
        /// 指定された型のカスタム属性を取得します。
        /// </summary>
        /// <typeparam name="T">取得する属性の型。</typeparam>
        /// <returns>見つかった属性。それ以外の場合は null。</returns>
        private T? GetAssemblyAttribute<T>() where T : Attribute
        {
            if (!Exists) return null;
            // Exists プロパティで null チェックが行われているため、この時点で _assembly が null でないことは保証されています。
            return _assembly!.GetCustomAttribute<T>();
        }
    }
}
