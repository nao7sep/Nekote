using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列を自然順（natural order）で比較するための機能を提供します。この順序は、ファイル名やバージョン番号など、
    /// 人間が直感的に期待するソート順（例: "file 2.txt"が"file 10.txt"より前に来る）を実現します。
    /// このクラスは不変（immutable）であり、その静的インスタンスはスレッドセーフです。
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
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別する自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: ファイルパス、URI、プロトコルメッセージ、内部キーなど、機械が処理する識別子の比較に適しています。
        /// 動作: 文字をUnicodeコードポイントとして直接比較するため、高速で予測可能です。カルチャに依存しないため、環境を問わず一貫した結果が得られます。
        /// 注意: 'f' と 'F' を区別します。言語的な等価性（例: 'é' と 'e' + '´'）を考慮しないため、ユーザー向けの表示には不向きです。Linuxのような大文字と小文字を区別するファイルシステムで特に有用です。
        /// </remarks>
        public static NaturalStringComparer Ordinal { get; } = new NaturalStringComparerImplementation(StringComparer.Ordinal, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別しない自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="Ordinal"/> と同様に機械が処理する識別子向けですが、大文字と小文字を区別しません。
        /// 動作: <see cref="Ordinal"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: Windowsのように大文字と小文字を区別しないファイルシステムでのファイル名比較に最適です。
        /// </remarks>
        public static NaturalStringComparer OrdinalIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.OrdinalIgnoreCase, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別する自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: 言語的に意味のあるが、特定のカルチャに依存しない方法で表示・ソートするデータに適しています。
        /// 動作: 言語的な規則に基づいて比較します。例えば、カノニカル等価な文字列（'é' と 'e' + '´'）を正しく等価と判断します。
        /// 注意: <see cref="Ordinal"/> よりも低速です。また、ファイル名のような技術的識別子に使用すると、直感に反する結果を返すことがあります。
        /// 例えば、`StringComparer.InvariantCulture`が"File1.txt"を"file2.txt"より小さいと判断するのに対し、
        /// この自然順比較では"File1.txt"がより大きいと判断されます。
        /// これは、自然順アルゴリズムが文字列を「File」と「1」、および「file」と「2」のようにチャンクに分割するためです。
        /// この分割により、.NETの`StringComparer`が持つ、大文字小文字以外の部分が同一の場合に数値を優先する能力が妨げられます。
        /// 結果として、最初のテキストチャンク（"File"と"file"）の比較が全体の順序を決定してしまい、直感に反する順序が生まれます。
        /// </remarks>
        public static NaturalStringComparer InvariantCulture { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCulture, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別しない自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="InvariantCulture"/> と同様ですが、大文字と小文字を区別しません。
        /// 動作: <see cref="InvariantCulture"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: ユーザーに表示するリストなどで、カルチャに依存しないが、大文字・小文字を区別しないソートが必要な場合に適しています。
        /// </remarks>
        public static NaturalStringComparer InvariantCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: 現在のシステムカルチャに固有の規則で、ユーザーに表示するデータをソートする場合にのみ使用します。
        /// 動作: 実行環境のカルチャ設定に依存するため、結果が環境によって変わる可能性があります。
        /// 注意: 結果の再現性が保証されないため、データの永続化や内部キーの比較には絶対に使用しないでください。
        /// </remarks>
        public static NaturalStringComparer CurrentCulture { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCulture, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="CurrentCulture"/> と同様ですが、大文字と小文字を区別しません。
        /// 動作: <see cref="CurrentCulture"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: <see cref="CurrentCulture"/> と同じく、結果の再現性が保証されないため、データの永続化や内部キーには使用しないでください。
        /// </remarks>
        public static NaturalStringComparer CurrentCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 指定した StringComparer と正規化オプションを使用して NaturalStringComparer のインスタンスを作成します。
        /// </summary>
        /// <param name="baseComparer">基本的な文字列比較（大文字小文字の区別、カルチャなど）を行うための StringComparer。</param>
        /// <param name="normalize">比較前にUnicode正規化（例：全角数字を半角に変換）を行うかどうか。デフォルトは true です。</param>
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
        /// <param name="left">比較する最初のオブジェクト。</param>
        /// <param name="right">比較する 2 番目のオブジェクト。</param>
        /// <returns>
        /// 0未満: leftはrightより小さい。
        /// 0: leftはrightと等しい。
        /// 0より大きい: leftはrightより大きい。
        /// </returns>
        public abstract int Compare(string? left, string? right);

        /// <summary>
        /// 2つの文字列が等しいかどうかを判断します。
        /// </summary>
        /// <param name="left">比較する最初の文字列。</param>
        /// <param name="right">比較する 2 番目の文字列。</param>
        /// <returns>文字列が等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(string? left, string? right);

        /// <summary>
        /// 指定した文字列のハッシュコードを返します。
        /// </summary>
        /// <param name="text">ハッシュコードを取得する対象の文字列。</param>
        /// <returns>指定した文字列のハッシュコード。</returns>
        public abstract int GetHashCode(string text);

        /// <summary>
        /// 2つの文字スパンを比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="left">比較する最初の文字スパン。</param>
        /// <param name="right">比較する 2 番目の文字スパン。</param>
        /// <returns>
        /// 0未満: leftはrightより小さい。
        /// 0: leftはrightと等しい。
        /// 0より大きい: leftはrightより大きい。
        /// </returns>
        public abstract int Compare(ReadOnlySpan<char> left, ReadOnlySpan<char> right);

        /// <summary>
        /// 2つの文字スパンが等しいかどうかを判断します。
        /// </summary>
        /// <param name="left">比較する最初の文字スパン。</param>
        /// <param name="right">比較する 2 番目の文字スパン。</param>
        /// <returns>文字スパンが等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(ReadOnlySpan<char> left, ReadOnlySpan<char> right);

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <param name="text">ハッシュコードを取得する対象の文字スパン。</param>
        /// <returns>指定した文字スパンのハッシュコード。</returns>
        public abstract int GetHashCode(ReadOnlySpan<char> text);
    }
}
