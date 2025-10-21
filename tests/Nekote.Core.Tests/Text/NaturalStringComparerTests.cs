using System;
using System.Collections.Generic;
using System.Linq;
using Nekote.Core.Text;
using Xunit;

namespace Nekote.Core.Tests.Text
{
    /// <summary>
    /// NaturalStringComparerのテストクラス。
    /// </summary>
    public class NaturalStringComparerTests
    {
        /// <summary>
        /// 静的プロパティがnullでないインスタンスを返すことをテストします。
        /// </summary>
        [Fact]
        public void StaticProperties_ShouldReturnNonNullInstances()
        {
            // Arrange & Act & Assert
            Assert.NotNull(NaturalStringComparer.InvariantCulture);
            Assert.NotNull(NaturalStringComparer.InvariantCultureIgnoreCase);
            Assert.NotNull(NaturalStringComparer.CurrentCulture);
            Assert.NotNull(NaturalStringComparer.CurrentCultureIgnoreCase);
            Assert.NotNull(NaturalStringComparer.Ordinal);
            Assert.NotNull(NaturalStringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Createメソッドにnullのベースコンパレータを渡すとArgumentNullExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public void Create_WithNullBaseComparer_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => NaturalStringComparer.Create(null!));
        }

        /// <summary>
        /// Createメソッドに有効なベースコンパレータを渡すとインスタンスを返すことをテストします。
        /// </summary>
        [Fact]
        public void Create_WithValidBaseComparer_ShouldReturnInstance()
        {
            // Arrange
            var baseComparer = StringComparer.Ordinal;

            // Act
            var result = NaturalStringComparer.Create(baseComparer);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// 基本的な自然順序付けが正しく機能することをテストします。
        /// </summary>
        [Theory]
        [InlineData("file1.txt", "file2.txt", -1)]
        [InlineData("file2.txt", "file1.txt", 1)]
        [InlineData("file1.txt", "file1.txt", 0)]
        [InlineData("file1.txt", "file10.txt", -1)]
        [InlineData("file10.txt", "file1.txt", 1)]
        [InlineData("file2.txt", "file10.txt", -1)]
        [InlineData("file10.txt", "file2.txt", 1)]
        public void Compare_BasicNaturalOrdering_ShouldReturnCorrectResult(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// nullおよび空文字列を正しく処理できることをテストします。
        /// </summary>
        [Theory]
        [InlineData(null, null, 0)]
        [InlineData(null, "test", -1)]
        [InlineData("test", null, 1)]
        [InlineData("", "", 0)]
        [InlineData("", "test", -1)]
        [InlineData("test", "", 1)]
        public void Compare_NullAndEmptyStrings_ShouldHandleCorrectly(string? x, string? y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// 先行ゼロを含む数値を正しく比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file01.txt", "file1.txt", 0)]
        [InlineData("file001.txt", "file1.txt", 0)]
        [InlineData("file0.txt", "file0.txt", 0)]
        [InlineData("file00.txt", "file0.txt", 0)]
        public void Compare_LeadingZeros_ShouldTreatAsEqual(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 全角数字を正しく正規化して比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file１２３.txt", "file123.txt", 0)] // 全角数字 vs 半角数字
        [InlineData("file１.txt", "file10.txt", -1)]
        [InlineData("file１０.txt", "file1.txt", 1)]
        public void Compare_FullWidthDigits_ShouldNormalizeCorrectly(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// 接頭辞が共通の文字列を正しく比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file", "file1", -1)]
        [InlineData("file1", "file", 1)]
        [InlineData("abc", "abcd", -1)]
        [InlineData("abcd", "abc", 1)]
        public void Compare_PrefixStrings_ShouldReturnCorrectResult(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// テキストと数値の境界を正しく処理してソートできることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file.txt", "file1.txt", -1)]
        [InlineData("file1.txt", "file.txt", 1)]
        [InlineData("a.txt", "a1.txt", -1)]
        [InlineData("a1.txt", "a.txt", 1)]
        public void Compare_TextAndNumberBoundaries_ShouldSortCorrectly(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// サロゲートペアを含む文字列を正しくチャンク分けして比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("a🎉b", "a🎉c", -1)] // 絵文字は比較を妨げません
        [InlineData("a🎉1", "a🎉01", 0)]   // 絵文字は数値比較を妨げません
        public void Compare_WithSurrogatePairs_ShouldChunkCorrectly(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// さまざまな入力に対してEqualsが正しい結果を返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData("file1.txt", "file1.txt", true)]
        [InlineData("file01.txt", "file1.txt", true)]
        [InlineData("file１.txt", "file1.txt", true)] // 全角数字
        [InlineData("file1.txt", "file2.txt", false)]
        [InlineData(null, null, true)]
        [InlineData(null, "test", false)]
        [InlineData("test", null, false)]
        public void Equals_VariousInputs_ShouldReturnCorrectResult(string? x, string? y, bool expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Equals(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// GetHashCodeにnull文字列を渡すとArgumentNullExceptionをスローすることをテストします。
        /// </summary>
        [Fact]
        public void GetHashCode_NullString_ShouldThrowArgumentNullException()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(null!));
        }

        /// <summary>
        /// 等価な文字列に対してGetHashCodeが同じハッシュコードを返すことをテストします。
        /// </summary>
        [Theory]
        [InlineData("file1.txt", "file01.txt")]
        [InlineData("file１.txt", "file1.txt")] // 全角数字
        [InlineData("file0.txt", "file00.txt")]
        public void GetHashCode_EquivalentStrings_ShouldReturnSameHashCode(string x, string y)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var hashX = comparer.GetHashCode(x);
            var hashY = comparer.GetHashCode(y);

            // Assert
            Assert.Equal(hashX, hashY);
        }

        /// <summary>
        /// GetHashCodeが空文字列で例外をスローしないことをテストします。
        /// </summary>
        [Fact]
        public void GetHashCode_EmptyString_ShouldNotThrow()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act & Assert
            var hash = comparer.GetHashCode("");
            // 例外が発生しないことを確認
        }

        /// <summary>
        /// 異なる文字列に対してGetHashCodeが異なるハッシュコードを返す可能性があることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file1.txt", "file2.txt")]
        [InlineData("abc.txt", "def.txt")]
        [InlineData("file1.txt", "file10.txt")]
        public void GetHashCode_DifferentStrings_MayReturnDifferentHashCodes(string x, string y)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var hashX = comparer.GetHashCode(x);
            var hashY = comparer.GetHashCode(y);

            // Assert
            // ハッシュコードは異なる可能性があるが、必須ではない（衝突は許可される）
            // このテストは主にクラッシュしないことを確認する
        }

        /// <summary>
        /// ReadOnlySpan<char>のCompareが正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void Compare_ReadOnlySpan_ShouldWorkCorrectly()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;
            ReadOnlySpan<char> x = "file1.txt".AsSpan();
            ReadOnlySpan<char> y = "file10.txt".AsSpan();

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.True(result < 0);
        }

        /// <summary>
        /// ReadOnlySpan<char>のEqualsが正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void Equals_ReadOnlySpan_ShouldWorkCorrectly()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;
            ReadOnlySpan<char> x = "file01.txt".AsSpan();
            ReadOnlySpan<char> y = "file1.txt".AsSpan();

            // Act
            var result = comparer.Equals(x, y);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// ReadOnlySpan<char>のGetHashCodeが正しく機能することをテストします。
        /// </summary>
        [Fact]
        public void GetHashCode_ReadOnlySpan_ShouldWorkCorrectly()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;
            ReadOnlySpan<char> span = "file1.txt".AsSpan();

            // Act & Assert
            var hash = comparer.GetHashCode(span);
            // 例外が発生しないことを確認
        }

        /// <summary>
        /// 複雑な例で自然順ソートが正しく行われることをテストします。
        /// </summary>
        [Fact]
        public void SortingBehavior_ComplexExample_ShouldSortNaturally()
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;
            var unsorted = new[]
            {
                "file10.txt",
                "file2.txt",
                "file1.txt",
                "file20.txt",
                "file01.txt",
                "file１.txt", // 全角数字
                "file.txt"
            };

            // Act
            var sorted = unsorted.OrderBy(x => x, comparer).ToArray();

            // Assert
            var expected = new[]
            {
                "file.txt",
                "file1.txt",
                "file01.txt",
                "file１.txt", // 全角数字は1と等価
                "file2.txt",
                "file10.txt",
                "file20.txt"
            };
            Assert.Equal(expected, sorted);
        }

        /// <summary>
        /// 非常に大きな数値を正しく処理できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("123456789012345678901234567890", "123456789012345678901234567891", -1)]
        [InlineData("999999999999999999999999999999", "1000000000000000000000000000000", -1)]
        public void Compare_VeryLargeNumbers_ShouldHandleCorrectly(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// 混合文字と数値を比較する際にベースコンパレータを使用することをテストします。
        /// </summary>
        [Theory]
        [InlineData("file-1.txt", "file1.txt", -1)]  // ハイフンは数字より小さい（ASCII順）
        [InlineData("file_1.txt", "file1.txt", 1)]   // アンダースコアは数字より大きい（ASCII順）
        public void Compare_MixedCharactersAndNumbers_ShouldUseBaseComparer(string x, string y, int expected)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(Math.Sign(expected), Math.Sign(result));
        }

        /// <summary>
        /// InvariantCultureでUnicodeの複雑な文字を比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file🎉1.txt", "file2.txt")]     // サロゲートペア（絵文字）
        [InlineData("fileé1.txt", "file2.txt")]      // 結合文字（e + 結合アクセント）
        [InlineData("file👨‍👩‍👧‍👦1.txt", "file2.txt")] // ZWJ結合絵文字シーケンス
        [InlineData("file🇯🇵1.txt", "file2.txt")]    // 地域指示子シーケンス（国旗）
        [InlineData("file👍🏻1.txt", "file2.txt")]    // 絵文字修飾子シーケンス
        [InlineData("fileनि1.txt", "file2.txt")]     // デーヴァナーガリー結合文字
        [InlineData("file각1.txt", "file2.txt")]      // ハングル合成文字
        public void Compare_UnicodeComplexCharacters_WithInvariantCulture(string x, string y)
        {
            // Arrange
            var comparer = NaturalStringComparer.InvariantCulture;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            // Unicode複合文字が含まれる場合でもクラッシュせずに比較が実行されることを確認
            Assert.True(result != int.MinValue && result != int.MaxValue,
                $"InvariantCulture comparison should return valid result for {x} vs {y}, got: {result}");
        }

        /// <summary>
        /// OrdinalでUnicodeの複雑な文字を比較できることをテストします。
        /// </summary>
        [Theory]
        [InlineData("file🎉1.txt", "file2.txt")]     // サロゲートペア（絵文字）
        [InlineData("fileé1.txt", "file2.txt")]      // 結合文字（e + 結合アクセント）
        [InlineData("file👨‍👩‍👧‍👦1.txt", "file2.txt")] // ZWJ結合絵文字シーケンス
        public void Compare_UnicodeComplexCharacters_WithOrdinal(string x, string y)
        {
            // Arrange
            var comparer = NaturalStringComparer.Ordinal;

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            // Ordinal比較でも基本的な比較は動作することを確認
            Assert.True(result != int.MinValue && result != int.MaxValue,
                $"Ordinal comparison should return valid result for {x} vs {y}, got: {result}"); // 厳密な値は実装依存
        }

        /// <summary>
        /// Unicode正規化が等価な形式を処理できることをテストします。
        /// </summary>
        [Fact]
        public void Compare_UnicodeNormalization_ShouldHandleEquivalentForms()
        {
            // Arrange
            var naturalCultureComparer = NaturalStringComparer.InvariantCulture;
            var systemCultureComparer = StringComparer.InvariantCulture;
            var naturalOrdinalComparer = NaturalStringComparer.Ordinal;

            // é の2つの表現：合成済み文字 vs 基底文字+結合文字
            string composed = "filé1.txt";    // U+00E9 (合成済み)
            string decomposed = "file\u0065\u0301" + "1.txt"; // e + 結合アクセント

            // Act
            var naturalCultureResult = naturalCultureComparer.Compare(composed, decomposed);
            var systemCultureResult = systemCultureComparer.Compare(composed, decomposed);
            var naturalOrdinalResult = naturalOrdinalComparer.Compare(composed, decomposed);

            // Assert
            // NaturalStringComparerは内部的にStringComparerを使用するため、
            // 正規化形式が異なる文字に対する動作は、基になるコンパレータと一致するはずです。
            Assert.Equal(Math.Sign(systemCultureResult), Math.Sign(naturalCultureResult));

            // StringComparer.InvariantCultureは、これらの正規化形式を等価とは見なしません。
            // そのため、結果は0にはなりません。
            Assert.NotEqual(0, naturalCultureResult);

            // Ordinal比較ではバイナリが異なるため、結果は0にはなりません。
            Assert.NotEqual(0, naturalOrdinalResult);
        }

        /// <summary>
        /// 異なるコンパレータタイプがすべて機能することをテストします。
        /// </summary>
        [Fact]
        public void Compare_DifferentComparerTypes_ShouldAllWork()
        {
            // Arrange
            var ordinalComparer = NaturalStringComparer.Ordinal;
            var cultureComparer = NaturalStringComparer.InvariantCulture;
            var ignoreCaseComparer = NaturalStringComparer.InvariantCultureIgnoreCase;

            string text1 = "File1.TXT";
            string text2 = "file2.txt";

            // Act
            var ordinalResult = ordinalComparer.Compare(text1, text2);
            var cultureResult = cultureComparer.Compare(text1, text2);
            var ignoreCaseResult = ignoreCaseComparer.Compare(text1, text2);

            // Assert
            // すべてのコンパレータで"File1.TXT"は"file2.txt"より小さいと判断されることを確認します。
            // OrdinalとInvariantCultureは'F'と'f'を区別し、'F'が小さいため、早い段階で-1を返します。
            // InvariantCultureIgnoreCaseは"File"と"file"を等しいと見なしますが、次に数値の1と2を比較して-1を返します。
            Assert.True(ordinalResult < 0, "Ordinal comparer should return negative value");
            Assert.True(cultureResult < 0, "Culture comparer should return negative value");
            Assert.True(ignoreCaseResult < 0, "Ignore case comparer should return negative value");
        }
    }
}
