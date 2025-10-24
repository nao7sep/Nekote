using System;
using System.Collections.Generic;
using System.Linq;
using Nekote.Core.Text;
using Nekote.Core.Text.Processing;
using Xunit;

namespace Nekote.Core.Tests.Text.Processing
{
    /// <summary>
    /// TextProcessor クラスのテスト。
    /// </summary>
    public class TextProcessorTests
    {
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
            var result = TextProcessor.EnumerateLines(text);

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
            var result = TextProcessor.Reformat(text, LineReaderConfiguration.Aggressive, NewlineSequence.Lf);

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
            var result = TextProcessor.EnumerateLines(text, LineReaderConfiguration.Aggressive);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Reformat が、空文字列を渡された場合に空文字列を返すことをテストします。
        /// </summary>
        [Fact]
        public void Reformat_WithEmptyInput_ShouldReturnEmptyString()
        {
            // Act
            var result = TextProcessor.Reformat("");

            // Assert
            Assert.Empty(result);
        }
    }
}
