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
        /// テスト用のコンパレータインスタンスのコレクションです。
        /// </summary>
        private static readonly NaturalStringComparer[] _comparers = new[]
        {
            NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: true),
            NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: true),
            NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: true),
            NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: true),
            NaturalStringComparer.Create(StringComparer.Ordinal, normalize: true),
            NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: true),
            NaturalStringComparer.Create(StringComparer.InvariantCulture, normalize: false),
            NaturalStringComparer.Create(StringComparer.InvariantCultureIgnoreCase, normalize: false),
            NaturalStringComparer.Create(StringComparer.CurrentCulture, normalize: false),
            NaturalStringComparer.Create(StringComparer.CurrentCultureIgnoreCase, normalize: false),
            NaturalStringComparer.Create(StringComparer.Ordinal, normalize: false),
            NaturalStringComparer.Create(StringComparer.OrdinalIgnoreCase, normalize: false)
        };

        [Fact]
        public void Compare_BasicNaturalSorting_ReturnsCorrectOrder()
        {
            var testData = new[] { "file10.txt", "file2.txt", "file1.txt" };
            var expected = new[] { "file1.txt", "file2.txt", "file10.txt" };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();
                Assert.Equal(expected, sorted);
            }
        }

        [Fact]
        public void Compare_LeadingZeros_AreTreatedAsNumericallyEqual()
        {
            var testCases = new[]
            {
                ("file01.txt", "file1.txt"),
                ("file001.txt", "file1.txt"),
                ("file010.txt", "file10.txt")
            };

            foreach (var comparer in _comparers)
            {
                foreach (var (withZeros, withoutZeros) in testCases)
                {
                    Assert.Equal(0, comparer.Compare(withZeros, withoutZeros));
                    Assert.True(comparer.Equals(withZeros, withoutZeros));
                }
            }
        }

        [Fact]
        public void Compare_LargeNumbers_ReturnsCorrectOrder()
        {
            var largeNum1 = "999999999999999999999999999999";
            var largeNum2 = "1000000000000000000000000000000";
            var testData = new[] { $"file{largeNum2}.txt", $"file{largeNum1}.txt", "file1.txt" };
            var expected = new[] { "file1.txt", $"file{largeNum1}.txt", $"file{largeNum2}.txt" };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();
                Assert.Equal(expected, sorted);
            }
        }

        [Fact]
        public void Compare_ZeroOnlyNumbers_ReturnsCorrectOrder()
        {
            foreach (var comparer in _comparers)
            {
                Assert.True(comparer.Compare("file0.txt", "file1.txt") < 0);
                Assert.Equal(0, comparer.Compare("file0.txt", "file00.txt"));
            }
        }

        [Fact]
        public void Compare_ComplexMixedCases_HandlesCorrectly()
        {
            var testData = new[] { "file10a.txt", "file2a.txt", "file1a.txt" };
            var expected = new[] { "file1a.txt", "file2a.txt", "file10a.txt" };

            foreach (var comparer in _comparers)
            {
                var sorted = testData.OrderBy(x => x, comparer).ToArray();
                Assert.Equal(expected, sorted);
            }
        }

        [Fact]
        public void Compare_CaseHandling_ReflectsBaseComparerBehavior()
        {
            var caseSensitiveComparers = _comparers.Where(c => c.ToString()!.Contains("IgnoreCase") == false);
            var caseInsensitiveComparers = _comparers.Where(c => c.ToString()!.Contains("IgnoreCase"));

            foreach (var comparer in caseSensitiveComparers)
            {
                Assert.NotEqual(0, comparer.Compare("FILE1.txt", "file1.txt"));
            }

            foreach (var comparer in caseInsensitiveComparers)
            {
                Assert.Equal(0, comparer.Compare("FILE1.txt", "file1.txt"));
            }
        }

        [Fact]
        public void Compare_FullWidthDigits_WithNormalization()
        {
            var normalizingComparers = _comparers.Take(6).ToArray();

            foreach (var comparer in normalizingComparers)
            {
                // 全角１０と半角2を比較 => 10 > 2
                Assert.True(comparer.Compare("file１０.txt", "file2.txt") > 0);
                // 全角２と半角10を比較 => 2 < 10
                Assert.True(comparer.Compare("file２.txt", "file10.txt") < 0);
                // 全角１と半角1は等しい
                Assert.Equal(0, comparer.Compare("file１.txt", "file1.txt"));
            }
        }

        [Fact]
        public void Compare_NullAndEmptyStrings_ReturnsCorrectOrder()
        {
            foreach (var comparer in _comparers)
            {
                Assert.Equal(0, comparer.Compare(null, null));
                Assert.True(comparer.Compare(null, "") < 0);
                Assert.True(comparer.Compare("", null) > 0);
                Assert.Equal(0, comparer.Compare("", ""));
                Assert.True(comparer.Compare("", "a") < 0);
                Assert.True(comparer.Compare("a", "") > 0);
            }
        }

        [Fact]
        public void GetHashCode_ForNumericallyEqualStrings_ReturnsSameHashCode()
        {
            var testCases = new[]
            {
                ("file1.txt", "file01.txt"),
                ("file1.txt", "file001.txt"),
                ("file10.txt", "file010.txt"),
                ("file0.txt", "file00.txt")
            };

            var normalizingComparers = _comparers.Take(6).ToArray();

            foreach (var comparer in normalizingComparers)
            {
                // Case-insensitive comparers might produce different hashes for different casings.
                // This is acceptable as long as they are consistent for equal strings.
                var hashA = comparer.GetHashCode("test");
                var hashB = comparer.GetHashCode("test");
                Assert.Equal(hashA, hashB);

                // Test numerically equal strings
                foreach (var (a, b) in testCases)
                {
                    Assert.Equal(comparer.GetHashCode(a), comparer.GetHashCode(b));
                }

                // Test full-width vs half-width
                Assert.Equal(comparer.GetHashCode("file1.txt"), comparer.GetHashCode("file１.txt"));
            }
        }
    }
}
