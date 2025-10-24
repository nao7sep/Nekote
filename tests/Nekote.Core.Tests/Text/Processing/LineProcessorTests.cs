using System;
using Nekote.Core.Text.Processing;
using Xunit;

namespace Nekote.Core.Tests.Text.Processing
{
    /// <summary>
    /// LineProcessor クラスのテスト。
    /// </summary>
    public class LineProcessorTests
    {
        /// <summary>
        /// デフォルトのプロセッサ（行末の空白のみトリム）が正しく動作することをテストします。
        /// </summary>
        [Theory]
        [InlineData("  hello world  ", "  hello world")] // 行末の空白をトリム
        [InlineData("line", "line")] // 変更なし
        [InlineData("  leading", "  leading")] // 行頭の空白は保持
        [InlineData("trailing  ", "trailing")] // 行末の空白はトリム
        [InlineData("  both  ", "  both")] // 行頭は保持、行末はトリム
        [InlineData("", "")] // 空文字列
        public void Process_WithDefaultProcessor_ShouldTrimTrailingWhitespace(string input, string expected)
        {
            // Arrange
            var processor = LineProcessor.Default;

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 積極的なプロセッサ（行頭・行末のトリム、行内の空白を単一スペースに圧縮）が
        /// 正しく動作することをテストします。
        /// </summary>
        [Theory]
        [InlineData("  hello   world  ", "hello world")] // 行頭・行末のトリム、行内の圧縮
        [InlineData("  lorem   ipsum  dolor  ", "lorem ipsum dolor")] // 複数の行内空白
        [InlineData("no-whitespace", "no-whitespace")] // 空白なし
        [InlineData("  leading", "leading")] // 行頭のトリム
        [InlineData("trailing  ", "trailing")] // 行末のトリム
        [InlineData("  \t  tabs and spaces \t", "tabs and spaces")] // タブとスペースの混在
        [InlineData("", "")] // 空文字列
        public void Process_WithAggressiveProcessor_ShouldTrimAndCollapseWhitespace(string input, string expected)
        {
            // Arrange
            var processor = LineProcessor.Aggressive;

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// パススループロセッサが、すべての空白を保持して文字列を変更しないことをテストします。
        /// </summary>
        [Theory]
        [InlineData("  hello world  ")]
        [InlineData("  lorem   ipsum  dolor  ")]
        [InlineData("  \t  tabs and spaces \t")]
        [InlineData("")]
        public void Process_WithPassthroughProcessor_ShouldKeepAllWhitespace(string input)
        {
            // Arrange
            var processor = LineProcessor.Passthrough;

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(input, result);
        }

        /// <summary>
        /// 行頭の空白処理（保持またはトリム）が個別に正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(LeadingWhitespaceBehavior.Keep, "  hello", "  hello")]
        [InlineData(LeadingWhitespaceBehavior.Trim, "  hello", "hello")]
        public void Process_WithLeadingWhitespaceBehavior_ShouldWorkCorrectly(LeadingWhitespaceBehavior leading, string input, string expected)
        {
            // Arrange
            // このテストでは行頭の動作のみを検証するため、他はすべて保持に設定
            var processor = new LineProcessor(leading, InternalWhitespaceBehavior.Keep, TrailingWhitespaceBehavior.Keep);

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 行末の空白処理（保持またはトリム）が個別に正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(TrailingWhitespaceBehavior.Keep, "hello  ", "hello  ")]
        [InlineData(TrailingWhitespaceBehavior.Trim, "hello  ", "hello")]
        public void Process_WithTrailingWhitespaceBehavior_ShouldWorkCorrectly(TrailingWhitespaceBehavior trailing, string input, string expected)
        {
            // Arrange
            var processor = new LineProcessor(LeadingWhitespaceBehavior.Keep, InternalWhitespaceBehavior.Keep, trailing);

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 行内の空白処理（保持または単一スペースに圧縮）が個別に正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(InternalWhitespaceBehavior.Keep, "hello   world", "hello   world")]
        [InlineData(InternalWhitespaceBehavior.CollapseToOneSpace, "hello   world", "hello world")]
        public void Process_WithInternalWhitespaceBehavior_ShouldWorkCorrectly(InternalWhitespaceBehavior internalBehavior, string input, string expected)
        {
            // Arrange
            // このテストでは行内の動作のみを検証するため、他はすべて保持に設定
            var processor = new LineProcessor(LeadingWhitespaceBehavior.Keep, internalBehavior, TrailingWhitespaceBehavior.Keep);

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// TryProcess メソッドが、十分なサイズのバッファで正常に処理を完了できることをテストします。
        /// このメソッドはメモリ割り当てを行わないため、パフォーマンスが重要な場面で役立ちます。
        /// </summary>
        [Fact]
        public void TryProcess_WithSufficientBuffer_ShouldSucceed()
        {
            // Arrange
            var processor = LineProcessor.Aggressive;
            var input = "  hello   world  ";
            var expected = "hello world";
            Span<char> buffer = new char[expected.Length];

            // Act
            bool success = processor.TryProcess(input.AsSpan(), buffer, out int charsWritten);

            // Assert
            Assert.True(success);
            Assert.Equal(expected.Length, charsWritten);
            Assert.Equal(expected, buffer.ToString());
        }

        /// <summary>
        /// TryProcess メソッドが、バッファサイズが不足している場合に false を返し、
        /// 書き込みを行わないことをテストします。
        /// </summary>
        [Fact]
        public void TryProcess_WithInsufficientBuffer_ShouldFail()
        {
            // Arrange
            var processor = LineProcessor.Aggressive;
            var input = "  hello   world  ";
            var expected = "hello world";
            // 期待される出力より1文字小さいバッファを準備
            Span<char> buffer = new char[expected.Length - 1];

            // Act
            bool success = processor.TryProcess(input.AsSpan(), buffer, out int charsWritten);

            // Assert
            Assert.False(success);
            Assert.Equal(0, charsWritten);
        }

        /// <summary>
        /// 入力行が空白文字のみで構成されるエッジケースで、各プロセッサが
        /// 期待通りに動作することをテストします。
        /// </summary>
        [Theory]
        [InlineData(LeadingWhitespaceBehavior.Trim, InternalWhitespaceBehavior.Keep, TrailingWhitespaceBehavior.Trim, "   \t   ", "")]
        [InlineData(LeadingWhitespaceBehavior.Keep, InternalWhitespaceBehavior.CollapseToOneSpace, TrailingWhitespaceBehavior.Keep, "   \t   ", " ")]
        [InlineData(LeadingWhitespaceBehavior.Keep, InternalWhitespaceBehavior.Keep, TrailingWhitespaceBehavior.Keep, "   \t   ", "   \t   ")]
        public void Process_WithWhitespaceOnlyInput_ShouldProduceCorrectString(LeadingWhitespaceBehavior lead, InternalWhitespaceBehavior inter, TrailingWhitespaceBehavior trail, string input, string expected)
        {
            // Arrange
            var processor = new LineProcessor(lead, inter, trail);

            // Act
            var result = processor.Process(input.AsSpan());

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// TryProcess メソッドが、空の入力を正しく処理し、0文字書き込んで成功を返すことをテストします。
        /// </summary>
        [Fact]
        public void TryProcess_WithEmptyInput_ShouldSucceed()
        {
            // Arrange
            var processor = LineProcessor.Aggressive;
            var input = "";
            Span<char> buffer = new char[10];

            // Act
            bool success = processor.TryProcess(input.AsSpan(), buffer, out int charsWritten);

            // Assert
            Assert.True(success);
            Assert.Equal(0, charsWritten);
        }
    }
}
