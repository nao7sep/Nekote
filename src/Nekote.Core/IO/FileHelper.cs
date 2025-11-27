using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nekote.Core.IO
{
    /// <summary>
    /// ファイル操作に関する静的ユーティリティメソッドを提供します。
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// ストリームのコピー操作で使用する既定のバッファサイズ。（80KB）
        /// https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.copytoasync?view=net-9.0
        /// もっと複雑な処理に変更されたようだが、フォールバック先的な値は変わっていない。
        /// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs,a232bc2650f49aa3
        /// </summary>
        private const int DefaultBufferSize = 81920;

        /// <summary>
        /// 指定されたパスの親ディレクトリが存在することを確認します。
        /// </summary>
        /// <param name="path">ファイルパス。</param>
        /// <remarks>
        /// 親ディレクトリが存在しない場合は作成します。
        /// 親ディレクトリが取得できない場合は何もしません。
        /// </remarks>
        public static void EnsureParentDirectoryExists(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// ファイルを非同期にコピーします。
        /// </summary>
        /// <param name="sourcePath">コピー元のファイルパス。</param>
        /// <param name="destPath">コピー先のファイルパス。</param>
        /// <param name="overwrite">コピー先のファイルを上書きするかどうか。</param>
        /// <param name="cancellationToken">操作をキャンセルするためのトークン。</param>
        /// <remarks>
        /// このメソッドは、大きなファイルのコピー中にキャンセルできるように、CancellationToken をリッスンします。
        /// 既定では、コピー先にファイルが存在する場合、IOException がスローされます。
        /// </remarks>
        public static async Task CopyAsync(string sourcePath, string destPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            // 非同期I/Oを指定し、ファイルにシーケンシャルアクセスすることをOSに通知して最適化を促します。
            // https://learn.microsoft.com/en-us/dotnet/api/system.io.fileoptions?view=net-9.0
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, fileOptions))
            using (var destStream = new FileStream(destPath, fileMode, FileAccess.Write, FileShare.None, DefaultBufferSize, fileOptions))
            {
                await sourceStream.CopyToAsync(destStream, DefaultBufferSize, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ファイルを非同期に移動します。
        /// </summary>
        /// <param name="sourcePath">移動元のファイルパス。</param>
        /// <param name="destPath">移動先のファイルパス。</param>
        /// <param name="overwrite">移動先のファイルを上書きするかどうか。</param>
        /// <param name="cancellationToken">操作をキャンセルするためのトークン。</param>
        /// <remarks>
        /// 同じボリューム内の移動は高速ですが、キャンセルは困難です。
        /// 異なるボリュームへの移動は、内部的にコピーと削除が行われるため、CancellationToken が有効に機能します。
        /// 既定では、移動先にファイルが存在する場合、IOException がスローされます。
        /// </remarks>
        public static async Task MoveAsync(string sourcePath, string destPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // .NET Core 2.1 / .NET 5 以降では、File.Move に上書きオプションがあります。
                // これを利用して、まず高速な同期移動を試みます。これは同じボリューム上で機能します。
                File.Move(sourcePath, destPath, overwrite);
            }
            catch (IOException)
            {
                // File.Move が失敗した場合のフォールバック処理です。
                // 主に異なるボリュームへの移動（ドライブをまたぐ移動）で発生します。
                // その場合、非同期コピーとソースの削除をもって移動とします。
                await CopyAsync(sourcePath, destPath, overwrite, cancellationToken).ConfigureAwait(false);
                File.Delete(sourcePath);
            }
        }
    }
}
