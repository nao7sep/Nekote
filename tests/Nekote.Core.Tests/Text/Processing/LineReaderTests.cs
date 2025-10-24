using System;
using System.Collections.Generic;
using System.Linq;
using Nekote.Core.Text;
using Nekote.Core.Text.Processing;
using Xunit;

namespace Nekote.Core.Tests.Text.Processing
{
    /// <summary>
    /// LineReader クラスのテスト。
    /// </summary>
    public class LineReaderTests
    {
        // テスト全体で共通して使用する、リーダーからすべての行を読み取るヘルパーメソッド
        private static List<string> ReadAllLines(LineReader reader)
        {
            var lines = new List<string>();
            while (reader.ReadLine(out ReadOnlySpan<char> line))
            {
                lines.Add(line.ToString());
            }
            return lines;
        }

        /// <summary>
        /// デフォルト設定の LineReader が、行頭・行末の空行を無視し、
        /// 中間の空行を1つにまとめ、行末の空白をトリムすることをテストします。
        /// </summary>
        [Fact]
        public void Create_WithDefaultConfiguration_ShouldProcessLinesCorrectly()
        {
            // Arrange
            var source = "\n  line1  \n\n  line2  \n\n";
            var reader = LineReader.Create(LineReaderConfiguration.Default, source.AsMemory());

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            // 期待される結果: ["  line1", "", "  line2"]
            // - 先頭の空行は無視される
            // - "  line1  " は行末がトリムされ "  line1" となる
            // - 中間の空行2つは1つにまとめられる
            // - "  line2  " は行末がトリムされ "  line2" となる
            // - 末尾の空行2つは無視される
            Assert.Equal(new[] { "  line1", "", "  line2" }, lines.ToArray());
        }

        /// <summary>
        /// 積極的な設定の LineReader が、すべての空白をトリム・圧縮し、
        /// 空行処理を正しく行うことをテストします。
        /// </summary>
        [Fact]
        public void Create_WithAggressiveConfiguration_ShouldProcessLinesCorrectly()
        {
            // Arrange
            var source = "\n  line1   extra  \n\n  line2  \n\n";
            var reader = LineReader.Create(LineReaderConfiguration.Aggressive, source.AsMemory());

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            // 期待される結果: ["line1 extra", "", "line2"]
            // - Aggressive設定は行頭・行末の空白をトリムし、行内の連続する空白を1つのスペースに統一する
            Assert.Equal(new[] { "line1 extra", "", "line2" }, lines.ToArray());
        }

        /// <summary>
        /// パススルー設定の LineReader が、すべての行と空白を
        /// 元のまま保持することをテストします。
        /// </summary>
        [Fact]
        public void Create_WithPassthroughConfiguration_ShouldProcessLinesCorrectly()
        {
            // Arrange
            var source = "\n  line1  \n\n  line2  \n\n";
            var reader = LineReader.Create(LineReaderConfiguration.Passthrough, source.AsMemory());

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            // Passthrough設定はすべての空白と空行をそのまま保持する
            Assert.Equal(new[] { "", "  line1  ", "", "  line2  ", "" }, lines.ToArray());
        }

        /// <summary>
        /// 空のソースから LineReader を作成した場合、行が読み取られないことをテストします。
        /// </summary>
        [Fact]
        public void ReadLine_WithEmptySource_ShouldReturnNoLines()
        {
            // Arrange
            var reader = LineReader.Create(LineReaderConfiguration.Default, "".AsMemory());

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            Assert.Empty(lines);
        }

        /// <summary>
        /// 空行の定義（完全に空か、空白のみか）によって、行の解釈が
        /// どのように変わるかをテストします。
        /// </summary>
        [Theory]
        [InlineData(EmptyLineDefinition.IsEmpty, new[] { "line1", "  ", "line2" })]
        [InlineData(EmptyLineDefinition.IsWhitespace, new[] { "line1", "", "line2" })]
        public void ReadLine_WithEmptyLineDefinition_ShouldWorkCorrectly(EmptyLineDefinition definition, string[] expected)
        {
            // Arrange
            var source = "line1\n  \nline2";
            var rawReader = new RawLineReader(source.AsMemory());
            var reader = new LineReader(
                rawReader,
                LineProcessor.Passthrough, // 行内の空白はそのまま保持
                definition,
                LeadingEmptyLineHandling.Ignore, // 先頭の空行は無視
                InterstitialEmptyLineHandling.CollapseToOne, // 中間の空行は1つにまとめる
                TrailingEmptyLineHandling.Ignore);

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            // IsEmpty: "  " は空行と見なされず、そのまま出力される
            // IsWhitespace: "  " は空行と見なされ、CollapseToOneによって "" に変換される
            Assert.Equal(expected, lines.ToArray());
        }

        /// <summary>
        /// 先頭の空行の処理（保持または無視）が正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(LeadingEmptyLineHandling.Keep, new[] { "", "", "line1" })]
        [InlineData(LeadingEmptyLineHandling.Ignore, new[] { "line1" })]
        public void ReadLine_WithLeadingEmptyLineHandling_ShouldWorkCorrectly(LeadingEmptyLineHandling handling, string[] expected)
        {
            // Arrange
            var source = "\n\nline1";
            var rawReader = new RawLineReader(source.AsMemory());
            var reader = new LineReader(
                rawReader,
                LineProcessor.Passthrough,
                EmptyLineDefinition.IsEmpty,
                handling,
                InterstitialEmptyLineHandling.Keep,
                TrailingEmptyLineHandling.Keep);

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            Assert.Equal(expected, lines.ToArray());
        }

        /// <summary>
        /// 中間の空行の処理（保持、無視、または1つにまとめる）が正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(InterstitialEmptyLineHandling.Keep, new[] { "line1", "", "", "line2" })]
        [InlineData(InterstitialEmptyLineHandling.CollapseToOne, new[] { "line1", "", "line2" })]
        [InlineData(InterstitialEmptyLineHandling.Ignore, new[] { "line1", "line2" })]
        public void ReadLine_WithInterstitialEmptyLineHandling_ShouldWorkCorrectly(InterstitialEmptyLineHandling handling, string[] expected)
        {
            // Arrange
            var source = "line1\n\n\nline2";
            var rawReader = new RawLineReader(source.AsMemory());
            var reader = new LineReader(
                rawReader,
                LineProcessor.Passthrough,
                EmptyLineDefinition.IsEmpty,
                LeadingEmptyLineHandling.Keep,
                handling,
                TrailingEmptyLineHandling.Keep);

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            Assert.Equal(expected, lines.ToArray());
        }

        /// <summary>
        /// 末尾の空行の処理（保持または無視）が正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData(TrailingEmptyLineHandling.Keep, new[] { "line1", "" })]
        [InlineData(TrailingEmptyLineHandling.Ignore, new[] { "line1" })]
        public void ReadLine_WithTrailingEmptyLineHandling_ShouldWorkCorrectly(TrailingEmptyLineHandling handling, string[] expected)
        {
            // Arrange
            var source = "line1\n\n";
            var rawReader = new RawLineReader(source.AsMemory());
            var reader = new LineReader(
                rawReader,
                LineProcessor.Passthrough,
                EmptyLineDefinition.IsEmpty,
                LeadingEmptyLineHandling.Keep,
                InterstitialEmptyLineHandling.Keep,
                handling);

            // Act
            var lines = ReadAllLines(reader);

            // Assert
            Assert.Equal(expected, lines.ToArray());
        }

        /// <summary>
        /// Reset メソッドがリーダーの状態を初期化し、再度先頭から読み取りが
        /// 可能になることをテストします。
        /// </summary>
        [Fact]
        public void Reset_ShouldAllowReadingFromBeginning()
        {
            // Arrange
            var source = "line1\nline2";
            var reader = LineReader.Create(LineReaderConfiguration.Default, source.AsMemory());
            var firstRun = ReadAllLines(reader);

            // Act
            reader.Reset();
            var secondRun = ReadAllLines(reader);

            // Assert
            Assert.True(firstRun.Any());
            Assert.Equal(firstRun, secondRun);
        }
    }
}
