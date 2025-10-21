using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列を自然順（natural order）で比較するための機能を提供します。この順序は、ファイル名やバージョン番号など、
    /// 人間が直感的に期待するソート順（例: "file 2.txt"が"file 10.txt"より前に来る）を実現します。
    /// </summary>
    /// <remarks>
    /// 設計思想
    ///
    /// このAPIは、.NET標準の <see cref="System.StringComparer"/> と同様の使い慣れた静的プロパティとファクトリパターンを提供するため、
    /// 抽象クラスとして設計されています。これにより、<see cref="InvariantCulture"/> のような定義済みインスタンスに簡単にアクセスできます。
    ///
    /// Unicodeの取り扱い
    ///
    /// 内部実装では、<see cref="GraphemeReader"/> を使用して、文字列を文字素クラスタ（grapheme cluster）単位で
    /// 安全に反復処理します。これにより、サロゲートペアで表現される絵文字や、結合文字（例: "e" + "´" = "é"）などが
    /// 1つの文字単位として正しく認識され、比較アルゴリズムが破壊されるのを防ぎます。この堅牢なUnicode処理が、このクラスの重要な特徴です。
    ///
    /// デフォルトの動作: Unicode正規化
    ///
    /// 静的プロパティ（例：<see cref="InvariantCulture"/>）によって返されるデフォルトのコンパレータは、Unicode正規化を実行します。
    /// これにより、全角数字（例：「１２３」）と半角数字（例：「123」）が等価として扱われ、特に日本語環境で直感的なソート順序を提供します。
    /// パフォーマンスが最優先で、入力文字列に全角文字などが含まれないことが確実な場合は、<see cref="Create(StringComparer, bool)"/> メソッドで
    /// 正規化を無効にしたインスタンスを生成できます。（例: NaturalStringComparer.Create(StringComparer.Ordinal, normalize: false)）
    ///
    /// 現在の実装の制限事項
    ///
    /// 現在の実装は、符号なし整数を効率的に処理することに特化しており、以下の要素はサポートされていません：
    ///
    /// - 符号: 正（+）または負（-）の符号は数字の一部として解釈されません。
    /// - 浮動小数点数: 小数点（.や,）は数字の一部として認識されず、文字列として扱われます。
    /// - 桁区切り文字: 桁区切り文字（,や.）はサポートされていません。
    ///
    /// これらの機能をサポートするには、カルチャに依存した高度な数値解析が必要となり、パフォーマンスへの影響と設計の複雑化を伴うため、
    /// 現在の実装では意図的に除外されています。
    /// </remarks>
    public abstract class NaturalStringComparer : IComparer<string>, IEqualityComparer<string>
    {
        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer InvariantCulture { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCulture, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer InvariantCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer CurrentCulture { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCulture, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer CurrentCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer Ordinal { get; } = new NaturalStringComparerImplementation(StringComparer.Ordinal, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer OrdinalIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.OrdinalIgnoreCase, normalize: true);

        /// <summary>
        /// 指定した StringComparer と正規化オプションを使用して NaturalStringComparer のインスタンスを作成します。
        /// </summary>
        /// <param name="baseComparer">基本的な文字列比較（大文字小文字の区別、カルチャなど）を行うための StringComparer。</param>
        /// <param name="normalize">比較前にUnicode正規化（例：全角数字を半角に変換）を行うかどうか。デフォルトは <c>true</c> です。</param>
        /// <returns>NaturalStringComparer の新しいインスタンス。</returns>
        public static NaturalStringComparer Create(StringComparer baseComparer, bool normalize = true)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            return new NaturalStringComparerImplementation(baseComparer, normalize);
        }

        /// <summary>
        /// 2つの文字列を比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="x">比較する最初のオブジェクト。</param>
        /// <param name="y">比較する 2 番目のオブジェクト。</param>
        /// <returns>
        /// 0未満: xはyより小さい。
        /// 0: xはyと等しい。
        /// 0より大きい: xはyより大きい。
        /// </returns>
        public abstract int Compare(string? x, string? y);

        /// <summary>
        /// 2つの文字列が等しいかどうかを判断します。
        /// </summary>
        /// <param name="x">比較する最初の文字列。</param>
        /// <param name="y">比較する 2 番目の文字列。</param>
        /// <returns>文字列が等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(string? x, string? y);

        /// <summary>
        /// 指定した文字列のハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字列。</param>
        /// <returns>指定した文字列のハッシュコード。</returns>
        public abstract int GetHashCode(string obj);

        /// <summary>
        /// 2つの文字スパンを比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="x">比較する最初の文字スパン。</param>
        /// <param name="y">比較する 2 番目の文字スパン。</param>
        /// <returns>
        /// 0未満: xはyより小さい。
        /// 0: xはyと等しい。
        /// 0より大きい: xはyより大きい。
        /// </returns>
        public abstract int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        /// <summary>
        /// 2つの文字スパンが等しいかどうかを判断します。
        /// </summary>
        /// <param name="x">比較する最初の文字スパン。</param>
        /// <param name="y">比較する 2 番目の文字スパン。</param>
        /// <returns>文字スパンが等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字スパン。</param>
        /// <returns>指定した文字スパンのハッシュコード。</returns>
        public abstract int GetHashCode(ReadOnlySpan<char> obj);
    }
}
