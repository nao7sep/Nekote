using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Nekote.Core.Tests.DotNet
{
    /// <summary>
    /// StringComparerの動作をテストするクラス。
    /// </summary>
    public class StringComparerTests
    {
        /// <summary>
        /// InvariantCultureで完全な文字列と部分文字列の比較結果が逆転することを実証します。
        /// この現象により、文字列比較の結果が文字の順序によるものか数値によるものかが不明確になります。
        /// </summary>
        [Fact]
        public void StringComparison_DemonstratesCounterIntuitiveInvariantCultureBehavior()
        {
            // ASCII表では 'F'(70) < 'f'(102) であり、数値では '1' < '2' である
            // 理論的には：
            // - 大文字小文字を区別する比較では File2.txt < file1.txt ('F' < 'f')
            // - 大文字小文字を区別しない比較では file1.txt < File2.txt ('1' < '2')
            // しかし、InvariantCultureでは予想と異なる結果になることを実証する

            // Arrange
            string fileName1 = "file1.txt";
            string fileName2 = "File2.txt";
            string fileName1Prefix = fileName1.Substring(0, 4); // "file"
            string fileName2Prefix = fileName2.Substring(0, 4); // "File"

            // Act - InvariantCulture比較
            int fullStringInvariant = string.Compare(fileName1, fileName2, StringComparison.InvariantCulture);
            int prefixInvariant = string.Compare(fileName1Prefix, fileName2Prefix, StringComparison.InvariantCulture);

            // Act - 比較のためのOrdinal比較
            int fullStringOrdinal = string.Compare(fileName1, fileName2, StringComparison.Ordinal);
            int prefixOrdinal = string.Compare(fileName1Prefix, fileName2Prefix, StringComparison.Ordinal);

            // Assert - InvariantCultureでは数値が優先される
            Assert.True(fullStringInvariant < 0,
                $"InvariantCulture full string: '{fileName1}' < '{fileName2}' (numerical 1<2 is prioritized), Result: {fullStringInvariant}");
            Assert.True(prefixInvariant < 0,
                $"InvariantCulture substring: '{fileName1Prefix}' < '{fileName2Prefix}' (lowercase < uppercase), Result: {prefixInvariant}");

            // Assert - Ordinalでは文字のASCII値が優先される
            Assert.True(fullStringOrdinal > 0,
                $"Ordinal full string: '{fileName1}' > '{fileName2}' ('f'>'F' is prioritized), Result: {fullStringOrdinal}");
            Assert.True(prefixOrdinal > 0,
                $"Ordinal substring: '{fileName1Prefix}' > '{fileName2Prefix}' ('f'>'F'), Result: {prefixOrdinal}");

            // この現象により比較結果の解釈が複雑になる:
            // InvariantCulture: "file1.txt" < "File2.txt"（数値 '1' < '2' が決定要因）
            // Ordinal: "file1.txt" > "File2.txt"（文字 'f' > 'F' が決定要因）
            // 同じ文字列ペアでも比較方法により結果が逆転し、何が決定要因かが不明確

            // InvariantCultureは「Ordinal + 文化的理解（特殊なラテン文字や絵文字など）」として
            // 万能ツールのように見えるが、実際は以下の予期しない動作をする:
            // - 数値が文字の大小より決定的な場合、大文字小文字を区別しない動作になる
            // - 大文字小文字が決定的な場合、ASCII順序を無視して File > file とする

            // 適切な使い分け:
            // 【Ordinal/OrdinalIgnoreCaseが適している場面】
            // - ファイル名やパスの比較（プラットフォーム依存性を考慮）
            // - プログラム内部の識別子やキーの比較
            // - パフォーマンスが重要な場面
            // - 予測可能で一貫した結果が必要な場面
            // - バイナリデータや暗号化ハッシュの比較

            // 【InvariantCultureが適している場面】
            // - ユーザー表示用のソート（UI上のリスト表示など）
            // - 文化的に中立でありながら人間が読みやすい順序が必要な場面
            // - 特殊文字（アクセント付き文字など）を含む国際化されたテキスト
            // - ログファイルやレポートでの人間向けの並び順
            //
            // 注意: InvariantCultureは英語でもなく完全に中立でもないため、
            // ファイル名比較には使用せず、文化的配慮が必要な場面でのみ使用すること
        }
    }
}
