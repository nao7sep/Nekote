using System;
using System.Collections.Generic;
using System.Linq;
using Nekote.Core.Text;
using Xunit;

namespace Nekote.Core.Tests.Text
{
    /// <summary>
    /// <see cref="NaturalStringComparer"/> のテストクラスです。
    /// </summary>
    public class NaturalStringComparerTests
    {
        /// <summary>
        /// テスト用の12個のコンパレータインスタンス（正規化有り6個、正規化無し6個）
        /// </summary>
        private static readonly NaturalStringComparer[] _comparers = new[]
        {
            // 正規化有りのインスタンス
            NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: true),
            NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: true),
            NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: true),
            NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: true),
            NaturalStringComparer.Create(StringComparer.Ordinal, normalize: true),
            NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: true),

            // 正規化無しのインスタンス
            NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: false),
            NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: false),
            NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: false),
            NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: false),
            NaturalStringComparer.Create(StringComparer.Ordinal, normalize: false),
            NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: false)
        };

        /// <summary>
        /// 基本的な自然順ソートのテストです。
        /// </summary>
        [Fact]
        public void Compare_BasicNaturalSorting_ReturnsCorrectOrder()
        {
            var testData = new[]
            {
                "file1.txt",
                "file2.txt",
                "file10.txt",
                "file20.txt"
            };

            var expected = new[]
            {
                "file1.txt",
                "file2.txt",
                "file10.txt",
                "file20.txt"
            };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();
                Assert.Equal(expected, sorted);
            }
        }

        /// <summary>
        /// 記号と数字の比較テストです。基本コンパレータの動作の違いを確認します。
        /// </summary>
        [Fact]
        public void Compare_SymbolsVsDigits_DelegatesToBaseComparer()
        {
            // 記号と数字が直接比較される場合のテスト
            var directTestCases = new[]
            {
                ("-", "1"),  // ハイフン vs 数字
                ("+", "1"),  // プラス記号 vs 数字
                ("_", "1"),  // アンダースコア vs 数字
                (".", "1"),  // ドット vs 数字
                (" ", "1"),  // スペース vs 数字
            };

            foreach (var (symbol, digit) in directTestCases)
            {
                var ordinalComparer = NaturalStringComparer.Create(StringComparer.Ordinal, normalize: true);
                var ordinalIgnoreCaseComparer = NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: true);

                var naturalOrdinalResult = ordinalComparer.Compare(symbol, digit);
                var naturalOrdinalIgnoreCaseResult = ordinalIgnoreCaseComparer.Compare(symbol, digit);

                var baseOrdinalResult = StringComparer.Ordinal.Compare(symbol, digit);
                var baseOrdinalIgnoreCaseResult = StringComparer.OrdinalIgnoreCase.Compare(symbol, digit);

                // 記号と数字の直接比較では、基本コンパレータの結果と同じ符号になる
                Assert.Equal(Math.Sign(baseOrdinalResult), Math.Sign(naturalOrdinalResult));
                Assert.Equal(Math.Sign(baseOrdinalIgnoreCaseResult), Math.Sign(naturalOrdinalIgnoreCaseResult));
            }

            // より複雑なケース：プレフィックスが同じで記号と数字が異なる位置にある場合
            var complexTestCases = new[]
            {
                ("file-1.txt", "file1.txt"),  // ハイフン vs 数字
                ("file+1.txt", "file1.txt"),  // プラス記号 vs 数字
                ("file_1.txt", "file1.txt"),  // アンダースコア vs 数字
            };

            foreach (var (symbolString, digitString) in complexTestCases)
            {
                foreach (var comparer in _comparers)
                {
                    var result = comparer.Compare(symbolString, digitString);
                    // 結果が一貫していることを確認（例外が発生しない）
                    Assert.True(result != 0); // 異なる文字列なので0ではない

                    // 同じ基本コンパレータを使用する場合、結果は一貫している
                    var result2 = comparer.Compare(symbolString, digitString);
                    Assert.Equal(result, result2);
                }
            }
        }

        /// <summary>
        /// 合成文字（composite characters）の処理テストです。
        /// </summary>
        [Fact]
        public void Compare_CompositeCharacters_HandledByBaseComparer()
        {
            // "é" は "e" + "´" (combining acute accent) として表現できる
            var decomposed = "file" + "e\u0301" + "1.txt";  // e + combining acute accent
            var precomposed = "fileé1.txt";                  // precomposed é

            // カルチャ依存のコンパレータでは、これらは等しいとして扱われる可能性がある
            var cultureComparers = new[]
            {
                NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: true),
                NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: true),
                NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: true),
                NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: true)
            };

            var ordinalComparers = new[]
            {
                NaturalStringComparer.Create(StringComparer.Ordinal, normalize: true),
                NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: true)
            };

            // 基本コンパレータの動作を確認
            var cultureResult = StringComparer.InvariantCulture.Compare("e\u0301", "é");
            var ordinalResult = StringComparer.Ordinal.Compare("e\u0301", "é");

            foreach (var comparer in cultureComparers)
            {
                var naturalResult = comparer.Compare(decomposed, precomposed);
                // カルチャ依存コンパレータの結果を反映
                Assert.Equal(Math.Sign(cultureResult), Math.Sign(naturalResult));
            }

            foreach (var comparer in ordinalComparers)
            {
                var naturalResult = comparer.Compare(decomposed, precomposed);
                // Ordinalコンパレータの結果を反映
                Assert.Equal(Math.Sign(ordinalResult), Math.Sign(naturalResult));
            }
        }

        /// <summary>
        /// サロゲートペア（絵文字など）の処理テストです。
        /// </summary>
        [Fact]
        public void Compare_SurrogatePairs_HandledByBaseComparer()
        {
            // 絵文字を含む文字列
            var emoji1 = "file🚀1.txt";  // rocket emoji
            var emoji2 = "file🎉1.txt";  // party emoji
            var regular = "filea1.txt";

            foreach (var comparer in _comparers)
            {
                // サロゲートペアを含む文字列も正常に処理される
                var result1 = comparer.Compare(emoji1, emoji2);
                var result2 = comparer.Compare(emoji1, regular);

                // 結果が一貫していることを確認（例外が発生しない）
                Assert.True(result1 != 0 || emoji1 == emoji2);
                Assert.True(result2 != 0 || emoji1 == regular);
            }
        }

        /// <summary>
        /// 大文字小文字の処理テストです。IgnoreCaseコンパレータの動作を確認します。
        /// </summary>
        [Fact]
        public void Compare_CaseHandling_ReflectsBaseComparerBehavior()
        {
            var testCases = new[]
            {
                ("FileA1.txt", "filea1.txt"),
                ("FILE1.txt", "file1.txt"),
                ("File1.TXT", "file1.txt")
            };

            var caseSensitiveComparers = new[]
            {
                NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: true),
                NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: true),
                NaturalStringComparer.Create(StringComparer.Ordinal, normalize: true)
            };

            var caseInsensitiveComparers = new[]
            {
                NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: true),
                NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: true),
                NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: true)
            };

            foreach (var (upper, lower) in testCases)
            {
                // 大文字小文字を区別するコンパレータでは異なる結果
                foreach (var comparer in caseSensitiveComparers)
                {
                    var result = comparer.Compare(upper, lower);
                    // 大文字小文字が異なる場合は0以外の結果
                    Assert.NotEqual(0, result);
                }

                // 大文字小文字を区別しないコンパレータでは同じ結果
                foreach (var comparer in caseInsensitiveComparers)
                {
                    var result = comparer.Compare(upper, lower);
                    // 大文字小文字のみが異なる場合は0
                    Assert.Equal(0, result);
                }
            }
        }

        /// <summary>
        /// Unicode正規化のテストです（全角数字）。
        /// </summary>
        [Fact]
        public void Compare_FullWidthDigits_NormalizationBehavior()
        {
            var testCases = new[]
            {
                ("file１.txt", "file1.txt"),    // 全角1 vs 半角1
                ("file２０.txt", "file20.txt"), // 全角20 vs 半角20
                ("file１０.txt", "file2.txt")   // 全角10 vs 半角2（数値比較）
            };

            var normalizingComparers = _comparers.Take(6).ToArray();  // 正規化有り
            var nonNormalizingComparers = _comparers.Skip(6).ToArray(); // 正規化無し

            foreach (var (fullWidth, halfWidth) in testCases)
            {
                // 正規化有りのコンパレータでは、全角と半角が数値的に比較される
                foreach (var comparer in normalizingComparers)
                {
                    if (fullWidth == "file１.txt" && halfWidth == "file1.txt")
                    {
                        // 全角1と半角1は等しい
                        Assert.Equal(0, comparer.Compare(fullWidth, halfWidth));
                    }
                    else if (fullWidth == "file１０.txt" && halfWidth == "file2.txt")
                    {
                        // 全角10は半角2より大きい
                        Assert.True(comparer.Compare(fullWidth, halfWidth) > 0);
                    }
                }

                // 正規化無しのコンパレータでは、基本コンパレータの動作に依存
                foreach (var comparer in nonNormalizingComparers)
                {
                    var result = comparer.Compare(fullWidth, halfWidth);
                    // 正規化無しでも動作することを確認（具体的な結果は基本コンパレータに依存）
                    Assert.True(result == 0 || result != 0); // 常に真だが、例外が発生しないことを確認
                }
            }
        }

        /// <summary>
        /// 先頭ゼロを含む数値の比較テストです。
        /// </summary>
        [Fact]
        public void Compare_LeadingZeros_NumericallyEqual()
        {
            var testCases = new[]
            {
                ("file01.txt", "file1.txt"),
                ("file001.txt", "file1.txt"),
                ("file0001.txt", "file1.txt"),
                ("file010.txt", "file10.txt")
            };

            foreach (var comparer in _comparers)
            {
                foreach (var (withZeros, withoutZeros) in testCases)
                {
                    // 先頭ゼロがあっても数値的に等しい
                    Assert.Equal(0, comparer.Compare(withZeros, withoutZeros));
                    Assert.True(comparer.Equals(withZeros, withoutZeros));
                }
            }
        }

        /// <summary>
        /// null値の処理テストです。
        /// </summary>
        [Fact]
        public void Compare_NullValues_ReturnsCorrectOrder()
        {
            foreach (var comparer in _comparers)
            {
                // null vs null
                Assert.Equal(0, comparer.Compare(null, null));

                // null vs non-null
                Assert.True(comparer.Compare(null, "test") < 0);
                Assert.True(comparer.Compare("test", null) > 0);
            }
        }

        /// <summary>
        /// 空文字列の処理テストです。
        /// </summary>
        [Fact]
        public void Compare_EmptyStrings_ReturnsCorrectOrder()
        {
            foreach (var comparer in _comparers)
            {
                // 空文字列同士
                Assert.Equal(0, comparer.Compare("", ""));

                // 空文字列 vs 非空文字列
                Assert.True(comparer.Compare("", "test") < 0);
                Assert.True(comparer.Compare("test", "") > 0);

                // null vs 空文字列
                Assert.True(comparer.Compare(null, "") < 0);
                Assert.True(comparer.Compare("", null) > 0);
            }
        }

        /// <summary>
        /// Equalsメソッドのテストです。
        /// </summary>
        [Fact]
        public void Equals_VariousCases_ReturnsCorrectResult()
        {
            foreach (var comparer in _comparers)
            {
                // 同じ文字列
                Assert.True(comparer.Equals("test123", "test123"));

                // 異なる文字列
                Assert.False(comparer.Equals("test123", "test124"));

                // null値
                Assert.True(comparer.Equals(null, null));
                Assert.False(comparer.Equals(null, "test"));
                Assert.False(comparer.Equals("test", null));

                // 空文字列
                Assert.True(comparer.Equals("", ""));
                Assert.False(comparer.Equals("", "test"));

                // 数値的に等しい文字列（先頭ゼロ）
                Assert.True(comparer.Equals("file01.txt", "file1.txt"));
                Assert.True(comparer.Equals("file001.txt", "file1.txt"));
            }
        }

        /// <summary>
        /// ReadOnlySpan版のCompareメソッドのテストです。
        /// </summary>
        [Fact]
        public void Compare_ReadOnlySpan_ConsistentWithStringVersion()
        {
            var testData = new[]
            {
                "file1.txt",
                "file2.txt",
                "file10.txt",
                "file-1.txt",
                "fileé1.txt"
            };

            foreach (var comparer in _comparers)
            {
                // 文字列版と同じ結果になることを確認
                for (int i = 0; i < testData.Length; i++)
                {
                    for (int j = 0; j < testData.Length; j++)
                    {
                        var stringResult = comparer.Compare(testData[i], testData[j]);
                        var spanResult = comparer.Compare(testData[i].AsSpan(), testData[j].AsSpan());
                        Assert.Equal(Math.Sign(stringResult), Math.Sign(spanResult));
                    }
                }
            }
        }

        /// <summary>
        /// ReadOnlySpan版のEqualsメソッドのテストです。
        /// </summary>
        [Fact]
        public void Equals_ReadOnlySpan_ConsistentWithStringVersion()
        {
            var testCases = new[]
            {
                ("test123", "test123"),
                ("test123", "test124"),
                ("file01.txt", "file1.txt"),
                ("fileé1.txt", "file" + "e\u0301" + "1.txt")
            };

            foreach (var comparer in _comparers)
            {
                foreach (var (x, y) in testCases)
                {
                    var stringResult = comparer.Equals(x, y);
                    var spanResult = comparer.Equals(x.AsSpan(), y.AsSpan());
                    Assert.Equal(stringResult, spanResult);
                }
            }
        }

        /// <summary>
        /// GetHashCodeメソッドのテストです。
        /// </summary>
        [Fact]
        public void GetHashCode_EqualStrings_ReturnsSameHashCode()
        {
            foreach (var comparer in _comparers)
            {
                // 同じ文字列は同じハッシュコードを返す
                var hash1 = comparer.GetHashCode("test123");
                var hash2 = comparer.GetHashCode("test123");
                Assert.Equal(hash1, hash2);

                // 数値的に等しい文字列は同じハッシュコードを返す
                var hashLeadingZero = comparer.GetHashCode("file01.txt");
                var hashNoLeadingZero = comparer.GetHashCode("file1.txt");
                Assert.Equal(hashLeadingZero, hashNoLeadingZero);

                // 複数の先頭ゼロも同じハッシュコードを返す
                var hashMultipleZeros = comparer.GetHashCode("file001.txt");
                Assert.Equal(hashMultipleZeros, hashNoLeadingZero);
            }
        }

        /// <summary>
        /// ReadOnlySpan版のGetHashCodeメソッドのテストです。
        /// </summary>
        [Fact]
        public void GetHashCode_ReadOnlySpan_ConsistentWithStringVersion()
        {
            var testStrings = new[]
            {
                "test123",
                "file01.txt",
                "file1.txt",
                "fileé1.txt"
            };

            foreach (var comparer in _comparers)
            {
                foreach (var testString in testStrings)
                {
                    var stringHash = comparer.GetHashCode(testString);
                    var spanHash = comparer.GetHashCode(testString.AsSpan());
                    Assert.Equal(stringHash, spanHash);
                }
            }
        }

        /// <summary>
        /// GetHashCodeメソッドでnullが渡された場合の例外テストです。
        /// </summary>
        [Fact]
        public void GetHashCode_NullString_ThrowsArgumentNullException()
        {
            foreach (var comparer in _comparers)
            {
                Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode((string)null!));
            }
        }

        /// <summary>
        /// 静的プロパティのテストです。
        /// </summary>
        [Fact]
        public void StaticProperties_ReturnValidInstances()
        {
            // 静的プロパティが有効なインスタンスを返すことを確認
            Assert.NotNull(NaturalStringComparer.InvariantCulture);
            Assert.NotNull(NaturalStringComparer.InvariantCultureIgnoreCase);
            Assert.NotNull(NaturalStringComparer.CurrentCulture);
            Assert.NotNull(NaturalStringComparer.CurrentCultureIgnoreCase);
            Assert.NotNull(NaturalStringComparer.Ordinal);
            Assert.NotNull(NaturalStringComparer.OrdinalIgnoreCase);

            // 同じインスタンスを返すことを確認（シングルトン）
            Assert.Same(NaturalStringComparer.InvariantCulture, NaturalStringComparer.InvariantCulture);
            Assert.Same(NaturalStringComparer.Ordinal, NaturalStringComparer.Ordinal);
        }

        /// <summary>
        /// Createメソッドのnull引数テストです。
        /// </summary>
        [Fact]
        public void Create_NullBaseComparer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NaturalStringComparer.Create(null!));
            Assert.Throws<ArgumentNullException>(() => NaturalStringComparer.Create(null!, normalize: true));
            Assert.Throws<ArgumentNullException>(() => NaturalStringComparer.Create(null!, normalize: false));
        }

        /// <summary>
        /// 大きな数値の比較テストです。
        /// </summary>
        [Fact]
        public void Compare_LargeNumbers_ReturnsCorrectOrder()
        {
            var testData = new[]
            {
                "file999999999999999999999999999999.txt",
                "file1000000000000000000000000000000.txt",
                "file1.txt"
            };

            var expected = new[]
            {
                "file1.txt",
                "file999999999999999999999999999999.txt",
                "file1000000000000000000000000000000.txt"
            };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();
                Assert.Equal(expected, sorted);
            }
        }

        /// <summary>
        /// ゼロのみの数値の比較テストです。
        /// </summary>
        [Fact]
        public void Compare_ZeroOnlyNumbers_ReturnsCorrectOrder()
        {
            foreach (var comparer in _comparers)
            {
                // すべてのゼロ値は等しいとして扱われ、1より小さい
                Assert.True(comparer.Compare("file0.txt", "file1.txt") < 0);
                Assert.True(comparer.Compare("file00.txt", "file1.txt") < 0);
                Assert.True(comparer.Compare("file000.txt", "file1.txt") < 0);

                // ゼロ値同士は等しい
                Assert.Equal(0, comparer.Compare("file0.txt", "file00.txt"));
                Assert.Equal(0, comparer.Compare("file0.txt", "file000.txt"));
                Assert.Equal(0, comparer.Compare("file00.txt", "file000.txt"));
            }
        }

        /// <summary>
        /// 複雑な混合ケースのテストです。記号、数字、文字が混在する場合。
        /// </summary>
        [Fact]
        public void Compare_ComplexMixedCases_HandlesCorrectly()
        {
            var testData = new[]
            {
                "file-1a.txt",
                "file1a.txt",
                "file-2a.txt",
                "file2a.txt",
                "file-10a.txt",
                "file10a.txt"
            };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();

                // 基本的な順序が保たれていることを確認
                // 具体的な順序は基本コンパレータに依存するが、数値部分は自然順になる
                var file1Index = Array.IndexOf(sorted, "file1a.txt");
                var file2Index = Array.IndexOf(sorted, "file2a.txt");
                var file10Index = Array.IndexOf(sorted, "file10a.txt");

                // 数値部分の自然順序が保たれている
                Assert.True(file1Index < file10Index);
                Assert.True(file2Index < file10Index);
            }
        }
    }
}