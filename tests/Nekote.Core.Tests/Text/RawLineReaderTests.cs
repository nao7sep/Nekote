using System;
using System.Collections.Generic;
using Nekote.Core.Text;
using Xunit;

namespace Nekote.Core.Tests.Text
{
    /// <summary>
    /// RawLineReader クラスのテスト。
    /// </summary>
    public class RawLineReaderTests
    {
        /// <summary>
        /// 空のソーステキストから読み取りを試みた場合、即座に false が返されることをテストします。
        /// </summary>
        [Fact]
        public void ReadLine_WithEmptySource_ShouldReturnFalse()
        {
            // Arrange
            var reader = new RawLineReader("".AsMemory());

            // Act
            // 最初の読み取り試行
            bool result = reader.ReadLine(out _);

            // Assert
            // テキストが空であるため、行は読み取れず false が返される
            Assert.False(result);
        }

        /// <summary>
        /// 改行文字を含まない単一行のテキストが正しく読み取られ、
        /// その後の呼び出しで false が返されることをテストします。
        /// </summary>
        [Fact]
        public void ReadLine_WithSingleLine_ShouldReturnCorrectLine()
        {
            // Arrange
            var reader = new RawLineReader("hello world".AsMemory());

            // Act
            bool result1 = reader.ReadLine(out ReadOnlySpan<char> line);

            // Assert
            // 最初の呼び出しでは行が正常に読み取れる
            Assert.True(result1);
            Assert.Equal("hello world", line.ToString());

            // Act
            // テキストの終端に達した後の2回目の呼び出し
            bool result2 = reader.ReadLine(out _);

            // Assert
            // すでに終端に達しているため、2回目の呼び出しでは false が返される
            Assert.False(result2);
        }

        /// <summary>
        /// LF, CRLF, CR およびそれらが混在する様々な形式の改行文字を
        /// RawLineReader が正しく解釈し、行を分割できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("line1\nline2\nline3", 3)] // LF形式 (Unix/Linux)
        [InlineData("line1\r\nline2\r\nline3", 3)] // CRLF形式 (Windows)
        [InlineData("line1\rline2\rline3", 3)] // CR形式 (古いMac)
        [InlineData("line1\r\nline2\nline3\rline4", 4)] // 混在形式
        [InlineData("line1\n\nline3", 3)] // 空行を含む
        public void ReadLine_WithVariousNewlines_ShouldReadCorrectLines(string source, int expectedLines)
        {
            // Arrange
            var reader = new RawLineReader(source.AsMemory());
            var lines = new List<string>();

            // Act
            // テキストの終端に達するまで行を読み取り続ける
            while (reader.ReadLine(out ReadOnlySpan<char> line))
            {
                lines.Add(line.ToString());
            }

            // Assert
            // 期待される行数と実際の結果が一致することを確認
            Assert.Equal(expectedLines, lines.Count);
            // 特定のテストケースで内容が正しいことを部分的に検証
            if (source.Contains("\n\n"))
            {
                Assert.Empty(lines[1]);
            }
        }

        /// <summary>
        /// テキストが改行文字で終了する場合、その改行は行の終端として扱われ、
        /// テキスト全体の終わりに余分な空行が生成されないことをテストします。
        /// これは、多くの標準的なテキストエディタやツールの動作と一致します。
        /// </summary>
        [Theory]
        [InlineData("text\n")]
        [InlineData("text\r\n")]
        [InlineData("text\r")]
        public void ReadLine_WithTrailingNewline_ShouldNotReturnEmptyLineAtEnd(string source)
        {
            // Arrange
            var reader = new RawLineReader(source.AsMemory());

            // Act
            reader.ReadLine(out ReadOnlySpan<char> line1);

            // 2回目の呼び出しではfalseが返ることを確認
            bool result = reader.ReadLine(out _);

            // Assert
            // 最初の行が正しく読み取られ、2回目の呼び出しで終端に達することを確認
            Assert.Equal("text", line1.ToString());
            Assert.False(result);
        }

        /// <summary>
        /// 改行文字のみ、空白のみ、または複数空行など、特殊なケースのテキストを
        /// ReadLine が正しく処理できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("\n", new[] { "" })] // 単一の改行
        [InlineData("\r\n", new[] { "" })] // 単一のCRLF改行
        [InlineData("\r", new[] { "" })] // 単一のCR改行
        [InlineData("\n\n", new[] { "", "" })] // 複数の改行
        [InlineData("   \t   ", new[] { "   \t   " })] // 改行を含まない空白文字のみの行
        [InlineData("line1\n\nline2", new[] { "line1", "", "line2" })] // 内容の間に空行
        public void ReadLine_WithEdgeCases_ShouldBehaveCorrectly(string source, string[] expectedLines)
        {
            // Arrange
            var reader = new RawLineReader(source.AsMemory());
            var lines = new List<string>();

            // Act
            while (reader.ReadLine(out ReadOnlySpan<char> line))
            {
                lines.Add(line.ToString());
            }

            // Assert
            // 期待される行の配列と実際の結果が一致することを確認
            Assert.Equal(expectedLines, lines);
        }

        /// <summary>
        /// Reset メソッドを呼び出すと、読み取り位置がテキストの先頭に戻り、
        /// 再度最初から読み取りが可能になることをテストします。
        /// </summary>
        [Fact]
        public void Reset_ShouldSetPositionToZero()
        {
            // Arrange
            var reader = new RawLineReader("line1\nline2".AsMemory());
            // 最初の行を読み進める
            reader.ReadLine(out _);

            // Act
            // リーダーの状態をリセット
            reader.Reset();

            // Assert
            // 位置が0に戻っていることを確認
            Assert.Equal(0, reader.Position);

            // 再度読み取りができることを確認
            bool result = reader.ReadLine(out ReadOnlySpan<char> line);
            Assert.True(result);
            Assert.Equal("line1", line.ToString());
        }
    }
}
