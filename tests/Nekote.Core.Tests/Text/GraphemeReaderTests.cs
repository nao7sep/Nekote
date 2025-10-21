using System;
using System.Linq;
using Nekote.Core.Text;
using Xunit;

namespace Nekote.Core.Tests.Text
{
    /// <summary>
    /// GraphemeReader クラスのテスト。
    /// </summary>
    public class GraphemeReaderTests
    {
        /// <summary>
        /// コンストラクタに null を渡した場合、ArgumentNullException がスローされることをテストします。
        /// </summary>
        [Fact]
        public void Constructor_WithNullSource_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GraphemeReader(null!));
        }

        /// <summary>
        /// コンストラクタに空文字列を渡した場合、正常に初期化されることをテストします。
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyString_ShouldSucceed()
        {
            // Arrange
            var reader = new GraphemeReader("");

            // Act & Assert
            Assert.Empty(reader);
            Assert.True(reader.IsEndOfText);
        }

        /// <summary>
        /// Count プロパティが正しい書記素クラスタ数を返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData("abc", 3)]
        [InlineData("a👍c", 3)]   // Surrogate pair
        [InlineData("é", 1)]      // Combining character e + ´ (e\u0301)
        [InlineData("👨‍👩‍👧‍👦", 1)]   // ZWJ sequence
        [InlineData("", 0)]
        public void Count_ShouldReturnCorrectGraphemeCount(string source, int expectedCount)
        {
            // Arrange
            var reader = new GraphemeReader(source);

            // Act & Assert
            Assert.Equal(expectedCount, reader.Count);
        }

        /// <summary>
        /// Position プロパティの取得および設定が正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void Position_CanGetAndSet()
        {
            // Arrange
            var reader = new GraphemeReader("a👍c");

            // Act & Assert
            Assert.Equal(0, reader.Position);

            reader.Position = 2;
            Assert.Equal(2, reader.Position);
            Assert.Equal("c", reader.Peek());

            // Position は Count (テキストの終端) に設定できます
            reader.Position = 3;
            Assert.Equal(3, reader.Position);
            Assert.True(reader.IsEndOfText);

            // 不正な範囲外の値を設定しようとすると例外がスローされることを確認します
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = 4); // Count は 3 なので、最大の Position は 3 です
        }

        /// <summary>
        /// IsEndOfText プロパティが正しい状態を返すことをテストします。
        /// </summary>
        [Fact]
        public void IsEndOfText_ShouldBeCorrect()
        {
            // Arrange
            var reader = new GraphemeReader("ab");

            // Act & Assert
            Assert.False(reader.IsEndOfText);
            reader.Position = 1;
            Assert.False(reader.IsEndOfText);
            reader.Position = 2;
            Assert.True(reader.IsEndOfText);
        }

        /// <summary>
        /// Peek および Read メソッドが正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void Peek_And_Read_ShouldWorkCorrectly()
        {
            // Arrange
            var reader = new GraphemeReader("a👍c");

            // Act & Assert
            Assert.Equal("a", reader.Peek());
            Assert.Equal(0, reader.Position);

            Assert.Equal("a", reader.Read());
            Assert.Equal(1, reader.Position);

            Assert.Equal("👍", reader.Peek());
            Assert.Equal(1, reader.Position);

            Assert.Equal("👍", reader.Read());
            Assert.Equal("c", reader.Read());
            Assert.Equal(3, reader.Position);

            Assert.True(reader.IsEndOfText);
            Assert.Null(reader.Peek());
            Assert.Null(reader.Read());
        }

        /// <summary>
        /// PeekAsSpan および ReadAsSpan メソッドが正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void PeekAsSpan_And_ReadAsSpan_ShouldWorkCorrectly()
        {
            // Arrange
            var reader = new GraphemeReader("a👍c");

            // Act & Assert
            Assert.Equal("a", reader.PeekAsSpan().ToString());
            Assert.Equal(0, reader.Position);

            Assert.Equal("a", reader.ReadAsSpan().ToString());
            Assert.Equal(1, reader.Position);

            var sb = new System.Text.StringBuilder();
            sb.Append(reader.ReadAsSpan());
            sb.Append(reader.ReadAsSpan());

            Assert.Equal("👍c", sb.ToString());
            Assert.Equal(3, reader.Position);

            Assert.True(reader.IsEndOfText);
            Assert.True(reader.PeekAsSpan().IsEmpty);
            Assert.True(reader.ReadAsSpan().IsEmpty);
        }

        /// <summary>
        /// インデクサが正しい書記素クラスタを返すことをテストします。
        /// </summary>
        [Fact]
        public void Indexer_ShouldReturnCorrectGrapheme()
        {
            // Arrange
            var reader = new GraphemeReader("a👍c");

            // Act & Assert
            Assert.Equal("a", reader[0]);
            Assert.Equal("👍", reader[1]);
            Assert.Equal("c", reader[2]);
            Assert.Throws<ArgumentOutOfRangeException>(() => reader[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => reader[3]);
        }

        /// <summary>
        /// GetEnumerator がすべての書記素クラスタを列挙することをテストします。
        /// </summary>
        [Fact]
        public void GetEnumerator_ShouldEnumerateAllGraphemes()
        {
            // Arrange
            var source = "a👍c";
            var expected = new[] { "a", "👍", "c" };
            var reader = new GraphemeReader(source);

            // Act
            var actual = new System.Collections.Generic.List<string>();
            foreach (var grapheme in reader)
            {
                actual.Add(grapheme);
            }

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// LINQ をリーダーに対して使用できることをテストします。
        /// </summary>
        [Fact]
        public void Linq_OnReader_ShouldWork()
        {
            // Arrange
            var source = "a👍c";
            var reader = new GraphemeReader(source);

            // Act
            var last = reader.Last();

            // Assert
            Assert.Equal("c", last);
        }

        /// <summary>
        /// Advance メソッドが正しく機能し、範囲外で例外をスローすることをテストします。
        /// </summary>
        [Fact]
        public void Advance_ShouldMovePositionAndThrowWhenOutOfRange()
        {
            // Arrange
            var reader = new GraphemeReader("a👍c"); // Count = 3

            // Act & Assert
            Assert.Equal(0, reader.Position);

            // 1つ進める
            reader.Advance(); // Default is 1
            Assert.Equal(1, reader.Position);
            Assert.Equal("👍", reader.Peek());

            // さらに2つ進めて末尾へ
            reader.Advance(2);
            Assert.Equal(3, reader.Position);
            Assert.True(reader.IsEndOfText);

            // 末尾からさらに進めようとすると例外が発生することを確認
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Advance(1));

            // 先頭より前に戻ろうとすると例外が発生することを確認
            reader.Position = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Advance(-1));
        }

        /// <summary>
        /// Slice および Substring が正しい範囲を返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData("a👍cdéf", 1, 3, "👍cd")]
        [InlineData("a👍cdéf", 0, 1, "a")]
        [InlineData("a👍cdéf", 4, 2, "éf")]
        [InlineData("a👍cdéf", 0, 6, "a👍cdéf")]
        [InlineData("abc", 3, 0, "")]
        public void Slice_And_Substring_ShouldReturnCorrectRange(string source, int start, int count, string expected)
        {
            // Arrange
            var reader = new GraphemeReader(source);

            // Act
            var sliceResult = reader.Slice(start, count);
            var substringResult = reader.Substring(start, count);

            // Assert
            Assert.Equal(expected, sliceResult.ToString());
            Assert.Equal(expected, substringResult);
        }

        /// <summary>
        /// Slice および Substring に不正な範囲を渡した場合、例外がスローされることをテストします。
        /// </summary>
        [Fact]
        public void Slice_And_Substring_WithInvalidRanges_ShouldThrow()
        {
            // Arrange
            var reader = new GraphemeReader("abc");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Slice(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Slice(0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Slice(1, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Slice(4, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Substring(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Substring(0, 4));
        }
    }
}
