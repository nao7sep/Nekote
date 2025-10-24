using System;
using System.Collections.Generic;
using System.Linq;
using Nekote.Core.Text;
using Nekote.Core.Text.Processing;
using Xunit;

namespace Nekote.Core.Tests.Text
{
    /// <summary>
    /// StringHelper クラスのテスト。
    /// </summary>
    public class StringHelperTests
    {
        /// <summary>
        /// NullIfEmpty が null または空文字列の場合に正しく null を返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", " ")] // 空白文字のみの文字列は空ではない
        [InlineData("hello", "hello")]
        public void NullIfEmpty_ShouldReturnNullForNullOrEmpty(string? input, string? expected)
        {
            // Act
            var result = StringHelper.NullIfEmpty(input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// NullIfWhiteSpace が null または空白文字のみで構成される文字列の場合に
        /// 正しく null を返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", null)]
        [InlineData("\t", null)]
        [InlineData("hello", "hello")]
        [InlineData(" hello ", " hello ")]
        public void NullIfWhiteSpace_ShouldReturnNullForNullOrWhitespace(string? input, string? expected)
        {
            // Act
            var result = StringHelper.NullIfWhiteSpace(input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// IsWhiteSpace が、与えられた ReadOnlySpan<char> が空または空白文字のみで
        /// 構成されているかを正しく判定することをテストします。
        /// </summary>
        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("\t\r\n", true)]
        [InlineData("a", false)]
        [InlineData(" a ", false)]
        public void IsWhiteSpace_ShouldCorrectlyIdentifyWhitespace(string input, bool expected)
        {
            // Act
            var result = StringHelper.IsWhiteSpace(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// EnumerateLines が、デフォルト設定で文字列を正しく行ごとに列挙することをテストします。
        /// </summary>
        [Fact]
        public void EnumerateLines_WithDefaultConfiguration_ShouldEnumerateCorrectly()
        {
            // Arrange
            var text = "\nline1\n\nline2\n";
            var expected = new[] { "line1", "", "line2" };

            // Act
            var result = StringHelper.EnumerateLines(text);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// JoinLines が、異なる改行シーケンス指定で正しく行を結合できることをテストします。
        /// </summary>
        [Theory]
        [InlineData(NewlineSequence.Lf, "line1\nline2")]
        [InlineData(NewlineSequence.CrLf, "line1\r\nline2")]
        public void JoinLines_WithDifferentNewlineSequences_ShouldJoinCorrectly(NewlineSequence sequence, string expected)
        {
            // Arrange
            var lines = new[] { "line1", "line2" };

            // Act
            var result = StringHelper.JoinLines(lines, sequence);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Reformat が、積極的なリーダー設定とLF改行を使用して、
        /// 複雑な空白や空行を含むテキストを正しく整形できることをテストします。
        /// </summary>
        [Fact]
        public void Reformat_WithAggressiveConfigAndLfNewline_ShouldReformatCorrectly()
        {
            // Arrange
            var text = "  \n  line1   extra  \n\n  line2  \n \n ";
            var expected = "line1 extra\n\nline2";

            // Act
            var result = StringHelper.Reformat(text, LineReaderConfiguration.Aggressive, NewlineSequence.Lf);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// EnumerateLines が、空または空白のみの文字列に対して、
        /// Aggressive 設定で空のシーケンスを返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   \t\n  ")]
        public void EnumerateLines_WithEmptyOrWhitespaceSource_ShouldReturnEmpty(string text)
        {
            // Act
            // Aggressive設定は空行を無視するため、結果は空になる
            var result = StringHelper.EnumerateLines(text, LineReaderConfiguration.Aggressive);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// JoinLines が、空のリストや空文字列を含むリストなどのエッジケースを
        /// 正しく処理できることをテストします。
        /// </summary>
        [Fact]
        public void JoinLines_WithEdgeCaseContent_ShouldBehaveCorrectly()
        {
            // Assert
            // 空のシーケンスを結合すると空文字列になる
            Assert.Empty(StringHelper.JoinLines(new List<string>()));
            // 1つの空文字列だけを持つシーケンスを結合すると空文字列になる
            Assert.Empty(StringHelper.JoinLines(new[] { "" }));
            // 2つの空文字列を結合すると、間に改行が1つ入る
            Assert.Equal(Environment.NewLine, StringHelper.JoinLines(new[] { "", "" }));
        }

        /// <summary>
        /// JoinLines が、未定義の改行シーケンス値に対して
        /// ArgumentOutOfRangeException をスローすることをテストします。
        /// </summary>
        [Fact]
        public void JoinLines_WithInvalidNewlineSequence_ShouldThrowException()
        {
            // Arrange
            var lines = new[] { "line1" };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => StringHelper.JoinLines(lines, (NewlineSequence)99));
        }

        /// <summary>
        /// Reformat が、空文字列を渡された場合に空文字列を返すことをテストします。
        /// </summary>
        [Fact]
        public void Reformat_WithEmptyInput_ShouldReturnEmptyString()
        {
            // Act
            var result = StringHelper.Reformat("");

            // Assert
            Assert.Empty(result);
        }
    }
}
