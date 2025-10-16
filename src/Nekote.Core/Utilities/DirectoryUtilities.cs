using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nekote.Core.Utilities
{
    /// <summary>
    /// ディレクトリ操作に関する静的ユーティリティメソッドを提供します。
    /// </summary>
    public static class DirectoryUtilities
    {
        /// <summary>
        /// ディレクトリの内容を別のディレクトリに非同期にコピー（マージ）します。
        /// </summary>
        /// <param name="sourcePath">コピー元のディレクトリパス。</param>
        /// <param name="destPath">コピー先のディレクトリパス。</param>
        /// <param name="overwrite">同名のファイルが存在する場合に上書きするかどうか。</param>
        /// <param name="cancellationToken">操作をキャンセルするためのトークン。</param>
        /// <remarks>
        /// コピー先のディレクトリが存在する場合、ソースの内容がマージされます。
        /// `overwrite` パラメータはファイルの上書きのみに適用されます。
        /// </remarks>
        public static Task CopyAsync(string sourcePath, string destPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var sourceDirectory = new DirectoryInfo(sourcePath);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory.FullName}");
            }

            var destDirectory = new DirectoryInfo(destPath);

            Directory.CreateDirectory(destDirectory.FullName);
            return CopyInternalAsync(sourceDirectory, destDirectory, overwrite, cancellationToken);
        }

        /// <summary>
        /// 再帰的にディレクトリをコピーするプライベートヘルパーメソッド。
        /// </summary>
        private static async Task CopyInternalAsync(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite, CancellationToken cancellationToken)
        {
            // ファイルをコピーします
            foreach (var fileInfo in sourceDirectory.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string targetFilePath = Path.Combine(targetDirectory.FullName, fileInfo.Name);
                await FileUtilities.CopyAsync(fileInfo.FullName, targetFilePath, overwrite, cancellationToken).ConfigureAwait(false);
            }

            // サブディレクトリを処理します
            foreach (var subDirectoryInfo in sourceDirectory.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                DirectoryInfo newTargetDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, subDirectoryInfo.Name));
                Directory.CreateDirectory(newTargetDirectory.FullName);
                await CopyInternalAsync(subDirectoryInfo, newTargetDirectory, overwrite, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ディレクトリの内容を別のディレクトリに非同期に移動（マージ）します。
        /// </summary>
        /// <param name="sourcePath">移動元のディレクトリパス。</param>
        /// <param name="destPath">移動先のディレクトリパス。</param>
        /// <param name="overwrite">同名のファイルが存在する場合に上書きするかどうか。</param>
        /// <param name="cancellationToken">操作をキャンセルするためのトークン。</param>
        /// <remarks>
        /// この操作は、各ファイルを移動し、空になったソースディレクトリを削除することで実装されます。
        /// 移動先のディレクトリが存在する場合、ソースの内容がマージされます。
        /// このメソッドは完全にキャンセル可能です。
        /// </remarks>
        public static Task MoveAsync(string sourcePath, string destPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var sourceDirectory = new DirectoryInfo(sourcePath);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory.FullName}");
            }

            var destDirectory = new DirectoryInfo(destPath);
            Directory.CreateDirectory(destDirectory.FullName);

            return MoveInternalAsync(sourceDirectory, destDirectory, overwrite, cancellationToken);
        }

        /// <summary>
        /// 再帰的にディレクトリを移動するプライベートヘルパーメソッド。
        /// </summary>
        private static async Task MoveInternalAsync(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite, CancellationToken cancellationToken)
        {
            // ファイルを移動します
            foreach (var fileInfo in sourceDirectory.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string targetFilePath = Path.Combine(targetDirectory.FullName, fileInfo.Name);
                await FileUtilities.MoveAsync(fileInfo.FullName, targetFilePath, overwrite, cancellationToken).ConfigureAwait(false);
            }

            // サブディレクトリを処理します
            foreach (var subDirectoryInfo in sourceDirectory.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                DirectoryInfo newTargetDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, subDirectoryInfo.Name));
                Directory.CreateDirectory(newTargetDirectory.FullName);
                await MoveInternalAsync(subDirectoryInfo, newTargetDirectory, overwrite, cancellationToken).ConfigureAwait(false);
            }

            // 中身が空になったソースディレクトリを削除します
            sourceDirectory.Delete();
        }
    }
}
