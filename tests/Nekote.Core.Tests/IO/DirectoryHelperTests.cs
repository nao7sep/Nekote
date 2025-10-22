using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nekote.Core.IO;
using Nekote.Core.Guids;
using Xunit;

namespace Nekote.Core.Tests.IO
{
    /// <summary>
    /// DirectoryHelperのテストクラス。
    /// </summary>
    public class DirectoryHelperTests : IDisposable
    {
        private readonly SystemGuidProvider _guidProvider;
        private readonly string _testRootPath;
        private readonly string _sourceTestPath;
        private readonly string _destTestPath;

        /// <summary>
        /// テスト用のコンストラクタ。一時的なテストディレクトリを作成します。
        /// </summary>
        public DirectoryHelperTests()
        {
            _guidProvider = new SystemGuidProvider();
            _testRootPath = Path.Combine(Path.GetTempPath(), "DirectoryHelperTests", _guidProvider.NewGuid().ToString());
            _sourceTestPath = Path.Combine(_testRootPath, "source");
            _destTestPath = Path.Combine(_testRootPath, "dest");

            Directory.CreateDirectory(_testRootPath);
        }

        /// <summary>
        /// テスト終了時にテストディレクトリを削除します。
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_testRootPath))
            {
                Directory.Delete(_testRootPath, true);
            }
        }

        /// <summary>
        /// テスト用のディレクトリ構造を作成するヘルパーメソッド。
        /// </summary>
        private void CreateTestDirectoryStructure()
        {
            Directory.CreateDirectory(_sourceTestPath);

            // ルートレベルのファイル
            File.WriteAllText(Path.Combine(_sourceTestPath, "file1.txt"), "Content of file1");
            File.WriteAllText(Path.Combine(_sourceTestPath, "file2.txt"), "Content of file2");

            // サブディレクトリとファイル
            var subDirPath = Path.Combine(_sourceTestPath, "subdir");
            Directory.CreateDirectory(subDirPath);
            File.WriteAllText(Path.Combine(subDirPath, "subfile1.txt"), "Content of subfile1");

            // ネストしたサブディレクトリ
            var nestedDirPath = Path.Combine(subDirPath, "nested");
            Directory.CreateDirectory(nestedDirPath);
            File.WriteAllText(Path.Combine(nestedDirPath, "nestedfile.txt"), "Content of nested file");
        }

        /// <summary>
        /// CopyAsyncメソッドが存在しないソースディレクトリに対してDirectoryNotFoundExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithNonExistentSourceDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // Arrange
            string nonExistentDirectoryPath = Path.Combine(_testRootPath, "nonexistent");

            // Act & Assert
            DirectoryNotFoundException exception = await Assert.ThrowsAsync<DirectoryNotFoundException>(
                () => DirectoryHelper.CopyAsync(nonExistentDirectoryPath, _destTestPath));

            Assert.Contains("Source directory not found", exception.Message);
        }

        /// <summary>
        /// CopyAsyncメソッドが単一ファイルを正常にコピーすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithSingleFile_ShouldCopySuccessfully()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            string sourceFilePath = Path.Combine(_sourceTestPath, "test.txt");
            string expectedContent = "Test content";
            File.WriteAllText(sourceFilePath, expectedContent);

            // Act
            await DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath);

            // Assert
            string destFilePath = Path.Combine(_destTestPath, "test.txt");
            Assert.True(File.Exists(destFilePath));
            Assert.Equal(expectedContent, File.ReadAllText(destFilePath));

            // ソースファイルが残っていることを確認
            Assert.True(File.Exists(sourceFilePath));
        }

        /// <summary>
        /// CopyAsyncメソッドが複雑なディレクトリ構造を正常にコピーすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithComplexStructure_ShouldCopyAllFilesAndDirectories()
        {
            // Arrange
            CreateTestDirectoryStructure();

            // Act
            await DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file1.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file2.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "subdir", "subfile1.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "subdir", "nested", "nestedfile.txt")));

            // 内容の確認
            Assert.Equal("Content of file1", File.ReadAllText(Path.Combine(_destTestPath, "file1.txt")));
            Assert.Equal("Content of nested file", File.ReadAllText(Path.Combine(_destTestPath, "subdir", "nested", "nestedfile.txt")));
        }

        /// <summary>
        /// CopyAsyncメソッドが既存のディレクトリにマージすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_ToExistingDirectory_ShouldMergeContents()
        {
            // Arrange
            CreateTestDirectoryStructure();
            Directory.CreateDirectory(_destTestPath);
            File.WriteAllText(Path.Combine(_destTestPath, "existing.txt"), "Existing content");

            // Act
            await DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(File.Exists(Path.Combine(_destTestPath, "existing.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file1.txt")));
            Assert.Equal("Existing content", File.ReadAllText(Path.Combine(_destTestPath, "existing.txt")));
        }

        /// <summary>
        /// CopyAsyncメソッドがoverwrite=falseの場合に既存ファイルでIOExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithExistingFileAndOverwriteFalse_ShouldThrowIOException()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            Directory.CreateDirectory(_destTestPath);
            string duplicateFileName = "duplicate.txt";
            File.WriteAllText(Path.Combine(_sourceTestPath, duplicateFileName), "Source content");
            File.WriteAllText(Path.Combine(_destTestPath, duplicateFileName), "Dest content");

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(
                () => DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath, overwrite: false));
        }

        /// <summary>
        /// CopyAsyncメソッドがoverwrite=trueの場合に既存ファイルを上書きすることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithExistingFileAndOverwriteTrue_ShouldOverwriteFile()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            Directory.CreateDirectory(_destTestPath);
            string duplicateFileName = "duplicate.txt";
            string sourceContent = "Source content";
            File.WriteAllText(Path.Combine(_sourceTestPath, duplicateFileName), sourceContent);
            File.WriteAllText(Path.Combine(_destTestPath, duplicateFileName), "Dest content");

            // Act
            await DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath, overwrite: true);

            // Assert
            string destFilePath = Path.Combine(_destTestPath, duplicateFileName);
            Assert.Equal(sourceContent, File.ReadAllText(destFilePath));
        }

        /// <summary>
        /// CopyAsyncメソッドがCancellationTokenによってキャンセルされることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithCancellationToken_ShouldBeCancellable()
        {
            // Arrange
            CreateTestDirectoryStructure();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath, cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// MoveAsyncメソッドが存在しないソースディレクトリに対してDirectoryNotFoundExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithNonExistentSourceDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testRootPath, "nonexistent");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DirectoryNotFoundException>(
                () => DirectoryHelper.MoveAsync(nonExistentPath, _destTestPath));

            Assert.Contains("Source directory not found", exception.Message);
        }

        /// <summary>
        /// MoveAsyncメソッドが単一ファイルを正常に移動することをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithSingleFile_ShouldMoveSuccessfully()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            string sourceFilePath = Path.Combine(_sourceTestPath, "test.txt");
            string expectedContent = "Test content";
            File.WriteAllText(sourceFilePath, expectedContent);

            // Act
            await DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath);

            // Assert
            string destFilePath = Path.Combine(_destTestPath, "test.txt");
            Assert.True(File.Exists(destFilePath));
            Assert.Equal(expectedContent, File.ReadAllText(destFilePath));

            // ソースファイルとディレクトリが削除されていることを確認
            Assert.False(File.Exists(sourceFilePath));
            Assert.False(Directory.Exists(_sourceTestPath));
        }

        /// <summary>
        /// MoveAsyncメソッドが複雑なディレクトリ構造を正常に移動することをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithComplexStructure_ShouldMoveAllFilesAndDirectories()
        {
            // Arrange
            CreateTestDirectoryStructure();

            // Act
            await DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file1.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file2.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "subdir", "subfile1.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "subdir", "nested", "nestedfile.txt")));

            // 内容の確認
            Assert.Equal("Content of file1", File.ReadAllText(Path.Combine(_destTestPath, "file1.txt")));
            Assert.Equal("Content of nested file", File.ReadAllText(Path.Combine(_destTestPath, "subdir", "nested", "nestedfile.txt")));

            // ソースディレクトリが削除されていることを確認
            Assert.False(Directory.Exists(_sourceTestPath));
        }

        /// <summary>
        /// MoveAsyncメソッドが既存のディレクトリにマージすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_ToExistingDirectory_ShouldMergeContents()
        {
            // Arrange
            CreateTestDirectoryStructure();
            Directory.CreateDirectory(_destTestPath);
            File.WriteAllText(Path.Combine(_destTestPath, "existing.txt"), "Existing content");

            // Act
            await DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(File.Exists(Path.Combine(_destTestPath, "existing.txt")));
            Assert.True(File.Exists(Path.Combine(_destTestPath, "file1.txt")));
            Assert.Equal("Existing content", File.ReadAllText(Path.Combine(_destTestPath, "existing.txt")));
            Assert.False(Directory.Exists(_sourceTestPath));
        }

        /// <summary>
        /// MoveAsyncメソッドがoverwrite=falseの場合に既存ファイルでIOExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithExistingFileAndOverwriteFalse_ShouldThrowIOException()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            Directory.CreateDirectory(_destTestPath);
            string duplicateFileName = "duplicate.txt";
            File.WriteAllText(Path.Combine(_sourceTestPath, duplicateFileName), "Source content");
            File.WriteAllText(Path.Combine(_destTestPath, duplicateFileName), "Dest content");

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(
                () => DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath, overwrite: false));
        }

        /// <summary>
        /// MoveAsyncメソッドがoverwrite=trueの場合に既存ファイルを上書きすることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithExistingFileAndOverwriteTrue_ShouldOverwriteFile()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);
            Directory.CreateDirectory(_destTestPath);
            var fileName = "duplicate.txt";
            var sourceContent = "Source content";
            File.WriteAllText(Path.Combine(_sourceTestPath, fileName), sourceContent);
            File.WriteAllText(Path.Combine(_destTestPath, fileName), "Dest content");

            // Act
            await DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath, overwrite: true);

            // Assert
            var destFilePath = Path.Combine(_destTestPath, fileName);
            Assert.Equal(sourceContent, File.ReadAllText(destFilePath));
            Assert.False(Directory.Exists(_sourceTestPath));
        }

        /// <summary>
        /// MoveAsyncメソッドがCancellationTokenによってキャンセルされることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithCancellationToken_ShouldBeCancellable()
        {
            // Arrange
            CreateTestDirectoryStructure();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath, cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// 空のディレクトリをコピーできることをテストします。
        /// </summary>
        [Fact]
        public async Task CopyAsync_WithEmptyDirectory_ShouldCreateEmptyDestination()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);

            // Act
            await DirectoryHelper.CopyAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(Directory.Exists(_destTestPath));
            Assert.Empty(Directory.GetFiles(_destTestPath, "*", SearchOption.AllDirectories));
        }

        /// <summary>
        /// 空のディレクトリを移動できることをテストします。
        /// </summary>
        [Fact]
        public async Task MoveAsync_WithEmptyDirectory_ShouldMoveEmptyDirectory()
        {
            // Arrange
            Directory.CreateDirectory(_sourceTestPath);

            // Act
            await DirectoryHelper.MoveAsync(_sourceTestPath, _destTestPath);

            // Assert
            Assert.True(Directory.Exists(_destTestPath));
            Assert.False(Directory.Exists(_sourceTestPath));
            Assert.Empty(Directory.GetFiles(_destTestPath, "*", SearchOption.AllDirectories));
        }
    }
}
