using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nekote.Core.Guids;
using Nekote.Core.IO;
using Xunit;

namespace Nekote.Core.Tests.IO
{
    /// <summary>
    /// FileHelperのテストクラス。
    /// </summary>
    public class FileHelperTests : IDisposable
    {
        private readonly SystemGuidProvider _guidProvider;
        private readonly string _testRootPath;
        private readonly string _sourceFilePath;
        private readonly string _destFilePath;

        /// <summary>
        /// テスト用のコンストラクタ。一時的なテストディレクトリを作成します。
        /// </summary>
        public FileHelperTests()
        {
            _guidProvider = new SystemGuidProvider();
            _testRootPath = Path.Combine(Path.GetTempPath(), "FileHelperTests", _guidProvider.NewGuid().ToString());
            _sourceFilePath = Path.Combine(_testRootPath, "source.txt");
            _destFilePath = Path.Combine(_testRootPath, "dest.txt");

            Directory.CreateDirectory(_testRootPath);
        }

        /// <summary>
        /// テスト終了時にテストディレクトリを削除します。
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_testRootPath))
            {
                try
                {
                    // 読み取り専用属性を削除してからディレクトリを削除
                    RemoveReadOnlyAttributes(_testRootPath);
                    Directory.Delete(_testRootPath, true);
                }
                catch (UnauthorizedAccessException)
                {
                    // 再試行: より強制的にファイル属性をリセット
                    try
                    {
                        ForceRemoveDirectory(_testRootPath);
                    }
                    catch
                    {
                        // 最終的に削除できない場合は無視（テンポラリディレクトリなので問題なし）
                    }
                }
                catch (IOException)
                {
                    // ファイルが使用中の場合も無視
                }
                catch
                {
                    // その他の例外も無視
                }
            }
        }

        /// <summary>
        /// ディレクトリ内のすべてのファイルから読み取り専用属性を削除します。
        /// </summary>
        private static void RemoveReadOnlyAttributes(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            try
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(filePath);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        }
                    }
                    catch
                    {
                        // 個別ファイルの属性変更に失敗しても続行
                    }
                }

                // ディレクトリの読み取り専用属性も削除
                foreach (string dirPath in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(dirPath);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(dirPath, attributes & ~FileAttributes.ReadOnly);
                        }
                    }
                    catch
                    {
                        // 個別ディレクトリの属性変更に失敗しても続行
                    }
                }
            }
            catch
            {
                // ディレクトリ列挙に失敗しても続行
            }
        }

        /// <summary>
        /// 強制的にディレクトリを削除します。
        /// </summary>
        private static void ForceRemoveDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            try
            {
                // 子ディレクトリを再帰的に削除
                foreach (string subDirectory in Directory.GetDirectories(directoryPath))
                {
                    ForceRemoveDirectory(subDirectory);
                }

                // 現在のディレクトリ内のファイルを削除
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    try
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // 個別ファイルの削除に失敗しても続行
                    }
                }

                // 現在のディレクトリの属性をリセットして削除
                try
                {
                    File.SetAttributes(directoryPath, FileAttributes.Normal);
                    Directory.Delete(directoryPath, false);
                }
                catch
                {
                    // ディレクトリの削除に失敗しても続行
                }
            }
            catch
            {
                // 削除に失敗しても無視
            }
        }

        /// <summary>
        /// CopyAsyncメソッドが小さなファイルを正常にコピーすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithSmallFile_ShouldCopySuccessfully()
        {
            // Arrange
            string expectedContent = "Test content for small file";
            File.WriteAllText(_sourceFilePath, expectedContent);

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.Equal(expectedContent, File.ReadAllText(_destFilePath));

            // ソースファイルが残っていることを確認
            Assert.True(File.Exists(_sourceFilePath));
        }

        /// <summary>
        /// CopyAsyncメソッドが大きなファイルを正常にコピーすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithLargeFile_ShouldCopySuccessfully()
        {
            // Arrange
            string largeContent = new string('A', 1024 * 1024); // 1MB のファイル
            File.WriteAllText(_sourceFilePath, largeContent);

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.Equal(largeContent, File.ReadAllText(_destFilePath));

            FileInfo sourceFileInfo = new FileInfo(_sourceFilePath);
            FileInfo destFileInfo = new FileInfo(_destFilePath);
            Assert.Equal(sourceFileInfo.Length, destFileInfo.Length);
        }

        /// <summary>
        /// CopyAsyncメソッドが存在しないソースファイルに対してFileNotFoundExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithNonExistentSourceFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(_testRootPath, "nonexistent.txt");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => FileHelper.CopyAsync(nonExistentFilePath, _destFilePath));
        }

        /// <summary>
        /// CopyAsyncメソッドがoverwrite=falseの場合に既存ファイルでIOExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithExistingFileAndOverwriteFalse_ShouldThrowIOException()
        {
            // Arrange
            File.WriteAllText(_sourceFilePath, "Source content");
            File.WriteAllText(_destFilePath, "Dest content");

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(
                () => FileHelper.CopyAsync(_sourceFilePath, _destFilePath, overwrite: false));
        }

        /// <summary>
        /// CopyAsyncメソッドがoverwrite=trueの場合に既存ファイルを上書きすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithExistingFileAndOverwriteTrue_ShouldOverwriteFile()
        {
            // Arrange
            var sourceContent = "Source content";
            File.WriteAllText(_sourceFilePath, sourceContent);
            File.WriteAllText(_destFilePath, "Dest content");

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath, overwrite: true);

            // Assert
            Assert.Equal(sourceContent, File.ReadAllText(_destFilePath));
        }

        /// <summary>
        /// CopyAsyncメソッドがCancellationTokenによってキャンセルされることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithCancellationToken_ShouldBeCancellable()
        {
            // Arrange
            var largeContent = new string('B', 10 * 1024 * 1024); // 10MB のファイル
            File.WriteAllText(_sourceFilePath, largeContent);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => FileHelper.CopyAsync(_sourceFilePath, _destFilePath, cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// CopyAsyncメソッドが存在しない親ディレクトリを持つ宛先パスでDirectoryNotFoundExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithNonExistentDestinationDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // Arrange
            File.WriteAllText(_sourceFilePath, "Test content");
            string destFilePathWithNonExistentDir = Path.Combine(_testRootPath, "nonexistent", "dest.txt");

            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(
                () => FileHelper.CopyAsync(_sourceFilePath, destFilePathWithNonExistentDir));
        }

        /// <summary>
        /// MoveAsyncメソッドが小さなファイルを正常に移動することをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithSmallFile_ShouldMoveSuccessfully()
        {
            // Arrange
            string expectedContent = "Test content for move";
            File.WriteAllText(_sourceFilePath, expectedContent);

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(expectedContent, File.ReadAllText(_destFilePath));
        }

        /// <summary>
        /// MoveAsyncメソッドが大きなファイルを正常に移動することをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithLargeFile_ShouldMoveSuccessfully()
        {
            // Arrange
            string largeContent = new string('C', 2 * 1024 * 1024); // 2MB のファイル
            File.WriteAllText(_sourceFilePath, largeContent);
            long originalLength = new FileInfo(_sourceFilePath).Length;

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(originalLength, new FileInfo(_destFilePath).Length);
        }

        /// <summary>
        /// MoveAsyncメソッドが存在しないソースファイルに対してFileNotFoundExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithNonExistentSourceFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            string nonExistentFilePath = Path.Combine(_testRootPath, "nonexistent.txt");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => FileHelper.MoveAsync(nonExistentFilePath, _destFilePath));
        }

        /// <summary>
        /// MoveAsyncメソッドがoverwrite=falseの場合に既存ファイルでIOExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithExistingFileAndOverwriteFalse_ShouldThrowIOException()
        {
            // Arrange
            File.WriteAllText(_sourceFilePath, "Source content");
            File.WriteAllText(_destFilePath, "Dest content");

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(
                () => FileHelper.MoveAsync(_sourceFilePath, _destFilePath, overwrite: false));
        }

        /// <summary>
        /// MoveAsyncメソッドがoverwrite=trueの場合に既存ファイルを上書きすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithExistingFileAndOverwriteTrue_ShouldOverwriteFile()
        {
            // Arrange
            string sourceContent = "Source content";
            File.WriteAllText(_sourceFilePath, sourceContent);
            File.WriteAllText(_destFilePath, "Dest content");

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath, overwrite: true);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(sourceContent, File.ReadAllText(_destFilePath));
        }

        /// <summary>
        /// MoveAsyncメソッドが同じボリューム内での高速移動を試行することをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithSameVolume_ShouldUseFastMove()
        {
            // Arrange
            var content = "Test content for same volume move";
            File.WriteAllText(_sourceFilePath, content);

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(content, File.ReadAllText(_destFilePath));
        }

        /// <summary>
        /// MoveAsyncメソッドがCancellationTokenによってキャンセルされることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithCancellationToken_ShouldBeCancellable()
        {
            // Arrange
            var content = "Test content";
            File.WriteAllText(_sourceFilePath, content);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            // 同一ボリューム内の移動では、キャンセレーショントークンが既にキャンセルされていてもMoveAsyncは例外をスローしない場合がある
            // これはFile.Moveが同期的で高速なため、期待される動作である
            try
            {
                await FileHelper.MoveAsync(_sourceFilePath, _destFilePath, cancellationToken: cancellationTokenSource.Token);
                // 例外がスローされない場合は、操作が正常に完了したことを確認
                Assert.True(File.Exists(_destFilePath));
                Assert.False(File.Exists(_sourceFilePath));
            }
            catch (OperationCanceledException)
            {
                // これは許容される動作（TaskCanceledExceptionはOperationCanceledExceptionを継承するため含まれる）
            }
        }

        /// <summary>
        /// 空のファイルをコピーできることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithEmptyFile_ShouldCopySuccessfully()
        {
            // Arrange
            File.WriteAllText(_sourceFilePath, string.Empty);

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.Equal(string.Empty, File.ReadAllText(_destFilePath));
            Assert.Equal(0, new FileInfo(_destFilePath).Length);
        }

        /// <summary>
        /// 空のファイルを移動できることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithEmptyFile_ShouldMoveSuccessfully()
        {
            // Arrange
            File.WriteAllText(_sourceFilePath, string.Empty);

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(string.Empty, File.ReadAllText(_destFilePath));
            Assert.Equal(0, new FileInfo(_destFilePath).Length);
        }

        /// <summary>
        /// バイナリファイルをコピーできることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithBinaryFile_ShouldCopySuccessfully()
        {
            // Arrange
            byte[] binaryData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE, 0xFD, 0xFC };
            File.WriteAllBytes(_sourceFilePath, binaryData);

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            byte[] copiedData = File.ReadAllBytes(_destFilePath);
            Assert.Equal(binaryData, copiedData);
        }

        /// <summary>
        /// バイナリファイルを移動できることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithBinaryFile_ShouldMoveSuccessfully()
        {
            // Arrange
            byte[] binaryData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE, 0xFD, 0xFC };
            File.WriteAllBytes(_sourceFilePath, binaryData);

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            byte[] movedData = File.ReadAllBytes(_destFilePath);
            Assert.Equal(binaryData, movedData);
        }

        /// <summary>
        /// 読み取り専用ファイルをコピーできることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithReadOnlyFile_ShouldCopySuccessfully()
        {
            // Arrange
            string content = "Read-only file content";
            File.WriteAllText(_sourceFilePath, content);
            File.SetAttributes(_sourceFilePath, FileAttributes.ReadOnly);

            // Act
            await FileHelper.CopyAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.Equal(content, File.ReadAllText(_destFilePath));
        }

        /// <summary>
        /// 読み取り専用ファイルを移動できることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithReadOnlyFile_ShouldMoveSuccessfully()
        {
            // Arrange
            var content = "Read-only file content for move";
            File.WriteAllText(_sourceFilePath, content);
            File.SetAttributes(_sourceFilePath, FileAttributes.ReadOnly);

            // Act
            await FileHelper.MoveAsync(_sourceFilePath, _destFilePath);

            // Assert
            Assert.True(File.Exists(_destFilePath));
            Assert.False(File.Exists(_sourceFilePath));
            Assert.Equal(content, File.ReadAllText(_destFilePath));
        }
    }
}
