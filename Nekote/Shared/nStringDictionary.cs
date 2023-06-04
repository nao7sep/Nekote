using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Dictionary<TKey,TValue> Class (System.Collections.Generic) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2

    public class nStringDictionary: Dictionary <string, string?>
    {
        // comparer を指定しなければ、EqualityComparer <T>.Default が使われる
        // これは、ComparerHelpers.CreateDefaultEqualityComparer → string なら new GenericEqualityComparer となっている
        // 比較の処理は、IEquatable <T>.Equals → string.Equals → string.EqualsHelper → SpanHelpers.SequenceEqual によって行われる
        // string.GetRawStringData を呼んでの処理なので、おそらくバイト単位での Ordinal な比較
        // ということから、Dictionary <string, ...> のデフォルトの comparer は Ordinal だと考えられる

        // ComparerHelpers.cs
        // https://source.dot.net/#System.Private.CoreLib/src/System/Collections/Generic/ComparerHelpers.cs

        // EqualityComparer.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/EqualityComparer.cs

        // String.Comparison.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Comparison.cs

        // String.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.cs

        // NameValueCollection では、CultureInfo.InvariantCulture.CompareInfo.GetStringComparer (CompareOptions.IgnoreCase) が使われる
        // 詳細は nNameValueCollection.cs の方に

        // JSON では、キーの大文字・小文字が区別されるとのこと

        // Are keys in JSON case sensitive? - Prog.World
        // https://prog.world/are-keys-in-json-case-sensitive/

        // .ini ファイルについては、次のページに Section and property names are case insensitive とある

        // INI file - Wikipedia
        // https://en.wikipedia.org/wiki/INI_file

        // INI File Parser というライブラリーのページには、I think that a case insensitive flag in the parser configuración
        //     would be an useful add on to the library so it is compatible with Windows native functions という書き込みがある
        // Windows native functions が case-insensitive な実装なら、Wikipedia との整合もあり、それが答えと見てよいだろう

        // Section and key names are case-sensitive, could this be configurable? · Issue #76 · rickyah/ini-parser
        // https://github.com/rickyah/ini-parser/issues/76

        // このクラスのインスタンスは、.ini ファイル「的」なものとの相互変換が可能になる
        // そういったファイルを（IT の専門家でない）一般のユーザーも編集することを想定するなら、キーを case-insensitive にすることにはメリットもある
        // しかし、このクラスは、CRUD で使われることもある
        // そのときに、ほとんどあるいは全てがコンピューターによるラウンドトリップなのに、毎回、キーの大文字・小文字に配慮するのは無駄が大きい
        // .ini ファイル「的」なものを扱うクラスだが、現行の主流である JSON に寄せて、キーを StringComparer.Ordinal で扱う

        public nStringDictionary (): base (StringComparer.Ordinal)
        {
        }

        // .NET の実装は IEqualityComparer <TKey>? と Nullable だが、ここでは ? なしに

        public nStringDictionary (IEqualityComparer <string> comparer): base (comparer)
        {
        }

        // キーがあれば最速で戻り、なければ例外が飛ぶのは、this

        // Dictionary.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs

        public string? GetString (string key)
        {
            return base [key];
        }

        // 最速なのは、TryGetValue

        // nNameValueCollection.GetStringOrDefault と同様、長さ1以上の値が得られたときのみ返す

        public string? GetStringOrDefault (string key, string? value)
        {
            if (TryGetValue (key, out string? xResult))
            {
                if (string.IsNullOrEmpty (xResult) == false)
                    return xResult;
            }

            return value;
        }

        // this が最速

        public void SetString (string key, string? value)
        {
            base [key] = value;
        }

        // Add は、TryInsert を呼び、戻り値が false なら投げる
        // ContainsKey → Add より、TryAdd だけの方が速い

        // しかし、TryAddString を用意しない
        // *String は、ごくわずかなオーバーヘッドを無視し、「これを呼んでおけば間違いない」というものを提供するもの
        // また、（いずれは）GetIntOrDefault などによるコーディングの省力化も行いたくてのもの
        // パフォーマンスを追求するなら、単純継承のクラスなので、TryAdd を使えばよい

        public void AddString (string key, string? value)
        {
            Add (key, value);
        }

        // nNameValueCollection.ToFriendlyString では IComparer <string?>? だが、こちらではキーが Nullable でない

        /// <summary>
        /// comparer ?? Comparer がキーのソートに使われる。
        /// </summary>
        public string ToFriendlyString (IComparer <string>? comparer = null, string? newLine = null)
        {
            if (Count == 0)
                return nStringLiterals.EmptyLabel;

            string xNewLine = newLine ?? Environment.NewLine;

            StringBuilder xBuilder = new StringBuilder (/* 不詳 */);

            nStringOptimizationOptions xOptions = new nStringOptimizationOptions
            {
                MinIndentationLength = 4
            };

            // StringComparer のプロパティーは、いずれも IEqualityComparer と IComparer の両方を実装
            // いずれかの指定なら、キャストは確実に成功する
            // ランタイム時まで Comparer が確定しないコードでなく、ここで変換できないならテスト時に分かる

            // StringComparer Class (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.stringcomparer

            // キャストと as の違いを確認した
            // キャストは、変換できないときに InvalidCastException を投げる
            // as は、変換できなければ null を返すため、Nullable でない値型には使えない

            // C# difference between casting and as? - Stack Overflow
            // https://stackoverflow.com/questions/955250/c-sharp-difference-between-casting-and-as

            foreach (var xPair in this.OrderBy (x => x.Key, comparer ?? (IComparer <string>) Comparer))
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (xNewLine);

                xBuilder.Append ($"{nString.GetLiteralIfNullOrEmpty (xPair.Key)}:");

                if (string.IsNullOrEmpty (xPair.Value))
                    xBuilder.Append ($"\x20{nStringLiterals.NullLabel}");

                else
                {
                    nStringOptimizationResult xResult = nStringOptimizer.Default.Optimize (xPair.Value, xOptions, xNewLine);

                    if (xResult.VisibleLineCount == 0)
                        xBuilder.Append ($"\x20{nStringLiterals.EmptyLabel}");

                    else if (xResult.VisibleLineCount == 1)
                        xBuilder.Append ($"\x20{xResult.Lines [0].VisibleString}");

                    else xBuilder.Append ($"{xNewLine}{xResult.Value}");
                }
            }

            return xBuilder.ToString ();
        }

        // このクラスによる CRUD について

        // 長い間、プログラミングを離れていたので、Entity Framework Core についても現時点（2023年1月4日）では何も知らない
        // Core なしの Entity Framework の最初の方のバージョンをチラ見して、重厚長大すぎて、「これなら自分で SQL を書く」と思って以来だ
        // 大勢が使うものには理由があるので改めて調べたところ、EF Core は良いようだった
        // データベースを作り、データを追加するまでを EF Core でやり、読み出しを Dapper でやる人も多い印象
        // 現時点の考えとしては、Nekote でも、EF Core と Dapper との親和性を考えることになりそう

        // EF Core には、Microsoft.EntityFrameworkCore.Sqlite があり、Microsoft がネイティブのライブラリーを出している
        // SQLite も自分はまだ本格的に使ったことはないが、あらゆるところで目にするので、Nekote でも対応することになりそう
        // CRUD は、パフォーマンスが問われないなら、SQLite + EF Core (+ Dapper) で、間違いなく不自由しない

        // Database Providers - EF Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/ef/core/providers/

        // NuGet Gallery | Microsoft.EntityFrameworkCore.Sqlite 7.0.1
        // https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite

        // それでもこのクラスを用意し、.ini ファイル「的」なものでの CRUD を可能にするのは、差分を取りたいため

        // 自分は、同じデータを2回チェックしたくない
        // 2回、3回とチェックを繰り返すことでようやく気付く問題も多々あるが、費用対効果は、「頭が冴えているときの1回目のチェック」において頂点に至る
        // メールなど、すぐに送ってしまうものは、送ったあとのチェックが無意味なので、「頭が冴えているときしか送らない」と決めるしかない
        // 一方、作業ログ、CRM 的な情報管理、ブログ記事といったものは、「今日はもう眠たいからザッと入力だけして、明日、1回だけチェック」というのがよくある
        // そういった「1回だけのチェック」を忘れないための「アーカイブ」的なボタンを全てのローカルアプリに実装するのはめんどくさい
        // データがたかだか数千件くらいのアプリなら、.ini 的なファイルで CRUD をやっても、SSD だと何も困らない
        // そのデータを丸ごと Git/Subversion に入れていけば、「1回だけのチェック」を忘れず、ついでにバックアップも可能

        // 外部ライブラリーに依存しないデータ入出力の方法が少なくとも一つ欲しいというのもある

        // その最たる例がログデータだ
        // どこからか飛んできた例外情報をとりあえず保存しておくときに、外部ライブラリーの DLL がなくなっているから出力できないようなことを避けたい
        // ウィンドウの位置と大きさ程度の簡単なセッション情報も、わざわざ外部ライブラリーを入れてデータベースに出し入れするほどのものでない
        // カルチャーによるアプリのローカライズのためのリソースファイルも、.ini ファイル「的」なものくらいがちょうど良い
        // .resx ファイルはとにかく古く、JSON は、ただキーと文字列を一対一で並べたいだけなら記号が多すぎる

        // =============================================================================

        // 現時点では ToIniLikeString のみで使うメソッドだが、internal にして他のクラスから使う可能性がゼロでないのでメソッド外に
        // C/C++ のエスケープシーケンスのうち、このクラスのインスタンスと文字列のラウンドトリップに必要な文字のみ扱う

        // StringBuilder を使い回す
        // 100の項目を持つインスタンスにおいて new StringBuilder が100回行われたり、文字が増えるたびに容量の拡大および既存の文字列のコピーが行われたりを避けるため
        // ということを気にするなら、CRUD においてデータの件数の分、ToIniLikeString が毎回 new StringBuilder を行うのも避けたくなるが、
        //     「クラスのインスタンスの内容を文字列化するにおける1～2度の new StringBuilder」は、他でも発生する、常識的に許容されるべき範囲内のコスト
        // そこまで無理やり最適化してもメリットが限定的である一方、クラスライブラリーとしての thread safety などに影響が及ぶ

        private static bool iEscapeString (StringBuilder builder, string? value, out string result)
        {
            // 次のページにある、Null を \0 にするというのは、文字列中の 0x00 のこと

            // Escape Sequence in C - javatpoint
            // https://www.javatpoint.com/escape-sequence-in-c

            // C# では 0x00 を文字列の終端に使うことはなく、文字列中に含まれていても問題にならない
            // この INI-like なフォーマットにおいて、他の文字のエスケープにより \0 という文字列が得られることはない
            // 本来の仕様と異なるが、ニュアンスは近く、処理上の問題もないため、null は \0 とラウンドトリップされるようにした

            // \0 については、次のページに The escape sequence \0 is a commonly used octal escape sequence, which denotes the null character, with value zero とある
            // \0 が個別に定義されているというより、\nnn において特によく使われるのが \0 のようだ

            // Escape sequences in C - Wikipedia
            // https://en.wikipedia.org/wiki/Escape_sequences_in_C

            // 使われていない文字、たとえば \l（L の小文字）を使うとか、それでは数字の1に見えうるので \z にするとか、
            //     null は必ず全体で解釈されるのだから、\null として、最初に null かどうかを調べるようにするとかも考えた

            // しかし、\l はやはり分かりにくく、\z などもピンとこない
            // せめて「ニュアンスの近さ」だけでも仕様に理由がないと、いずれ右往左往するリスクがある
            // 一方、\null は、"LF + ull" がラウンドトリップにより null になってしまう
            // 仕様に理由があり、正常な値から得られない文字列となると、\0 が妥当

            // \0 は 0x00 のエスケープだが、Nekote が 0x00 をエスケープしないため、null 以外では出力されない
            // 悪意あるユーザーが INI-like なファイルに入れてきても、全体でそうなら null になり、
            //     値の文字列の一部なら、\ のあとの文字が不正とされて nFormatException が飛ぶだけ

            if (value == null)
            {
                result = @"\0";
                return true;
            }

            if (value.Length == 0)
            {
                result = value;
                return false;
            }

            builder.Clear ();

            int xLength = value.Length;

            for (int temp = 0; temp < xLength; temp ++)
            {
                char xCurrent = value [temp];

                // 「: または = の直後から改行まで」なので、改行のみエスケープすれば足りる
                // 念のため制御文字を全て \nnn にするなども考えたが、文字列の処理や表示に影響しうる文字は Unicode にこそ多い
                // INI-like なファイルをどんなテキストエディターでも編集できるということは、Nekote が保証する必要のあることでない

                if (xCurrent == '\n')
                    builder.Append (@"\n");

                else if (xCurrent == '\r')
                    builder.Append (@"\r");

                else if (xCurrent == '\\')
                    builder.Append (@"\\");

                else builder.Append (xCurrent);
            }

            // 長さが1以上でエスケープの処理が行われた場合、どの変換でも文字が増えるため、
            //     エスケープされたのに文字列の長さが同じということはない

            result = builder.ToString ();
            return builder.Length > xLength;
        }

        public static readonly char [] KeyValueSeparators = { ':', '=' };

        public string ToIniLikeString (string? newLine = null)
        {
            if (Count == 0)
                return string.Empty;

            string xNewLine = newLine ?? Environment.NewLine;

            StringBuilder xBuilder = new StringBuilder (/* 不詳 */), // メイン
                xBuilderAlt = new StringBuilder (/* 不詳 */); // iEscapeString で使い回される

            foreach (var xPair in this)
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (xNewLine);
#if DEBUG
                // ユーザー入力をキーと値の両方に使うコーディングはあり得ないため、
                //     キーに不正な文字が含まれるミスは、デバッグ時に落とせる

                if (xPair.Key.IndexOfAny (KeyValueSeparators) >= 0)
                    throw new nDebugException ();
#endif
                xBuilder.Append (xPair.Key);

                string xResult;

                if (iEscapeString (xBuilderAlt, xPair.Value, out xResult))
                    xBuilder.Append ('=');

                else xBuilder.Append (':');

                xBuilder.Append (xResult);
            }

            return xBuilder.ToString ();
        }

        private static string? iUnescapeString (StringBuilder builder, string value)
        {
            if (string.IsNullOrEmpty (value))
                return string.Empty;

            if (value.Equals (@"\0", StringComparison.Ordinal))
                return null;

            builder.Clear ();

            int xLength = value.Length;

            for (int temp = 0; temp < xLength; temp ++)
            {
                char xCurrent = value [temp];

                if (xCurrent == '\\')
                {
                    if ((temp + 1 < xLength) == false)
                        throw new nFormatException ();

                    char xNext = value [temp + 1];

                    if (xNext == 'n')
                    {
                        builder.Append ('\n');
                        temp ++;
                    }

                    else if (xNext == 'r')
                    {
                        builder.Append ('\r');
                        temp ++;
                    }

                    else if (xNext == '\\')
                    {
                        builder.Append ('\\');
                        temp ++;
                    }

                    // 文字列中に \0 が含まれる場合、ここで例外が飛ぶ
                    else throw new nFormatException ();
                }

                else builder.Append (xCurrent);
            }

            return builder.ToString ();
        }

        public void LoadIniLikeString (string value)
        {
            if (string.IsNullOrEmpty (value))
                return;

            StringBuilder xBuilder = new StringBuilder (/* 不詳 */);

            // ラウンドトリップのため、行末の空白系文字を取り除かない
            // 不要な空行は、デフォルトで取り除かれるのを明示的に残す理由がない

            foreach (string xLine in nString.EnumerateLines (value, trimsTrailingWhiteSpaces: false))
            {
                // 行頭に // がある場合のみ、その行全体がコメントとして無視される
                // キーが必ず行頭に置かれるフォーマットなので、コメントも行頭から書かなければならないことに大きな問題はない
                // 「インデント + //」くらいは簡単に検出できるが、不可欠でない機能のために処理速度を落とせない

                if (xLine.StartsWith ("//", StringComparison.Ordinal))
                    continue;

                int xIndex = xLine.IndexOfAny (KeyValueSeparators);

                // コメント行でなく、: も = も見付からないとき、空白系文字だけなら何もせず、そうでないなら例外を投げる
                // キーに指が当たり空行に半角空白がいくつか入ったり、テキストエディターが入れた自動インデントが残ったりはよくある
                // それらは、テキストエディターの設定によってはユーザーに見えないため、なかったことにしてよい
                // しかし、目に見える文字があるのにコメントでもキーでもないのは、修正されるべきミスなのでエラーになるべき

                if (xIndex < 0)
                {
                    // nStringLineReader.ReadLine などは、空白系文字の判別に char.IsWhiteSpace を使う
                    // string.IsNullOrWhiteSpace も、そういう実装なのを確認した

                    // String.cs
                    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.cs

                    if (string.IsNullOrWhiteSpace (xLine))
                        continue;

                    else throw new nFormatException ();
                }

                // 値がエスケープされているのに記号がそれと整合しない場合のリスクはない
                // 元に戻らないだけで、システムに影響の及ぶ文字が混入するなどはない

                string xKey = xLine.Substring (0, xIndex);
                string? xValue = xLine [xIndex] == '=' ? iUnescapeString (xBuilder, xLine.Substring (xIndex + 1)) : xLine.Substring (xIndex + 1);

                // 上書きなので、キーが既存だというエラーにはならない

                // ロードやパーズにおけるキーの一意性への期待は、人間の限界を考えるにおいて現実的でない
                // INI-like なファイルを人間が編集する場合、人間には、目の前の1組のキーと値の正誤はすぐ分かっても、
                //     そのキーが他のところに現れないというのは、全体を見ないことには分からない

                // コメント行でない行に区切り文字がなく、キーというわけでもないなどは、それ単独で分かるミスなので改められるべき
                // しかし、全体を見ないと分からず、項目が多ければ見ても分からない「キーの重複」をエラーとすることは、
                //     それにより INI-like なファイルの質が向上するメリットより、落ちすぎて信頼性も落ちるリスクの方が大きい

                // CSS 同様、「最後に設定された値が優先される」という考え方を

                SetString (xKey, xValue);
            }
        }

        public static nStringDictionary ParseIniLikeString (string value, IEqualityComparer <string>? comparer = null)
        {
            // comparer が null なら StringComparer.Ordinal なのが分かっているが、
            //     コンストラクター側にデフォルト値が入っているため、そちらに頼る実装に

            nStringDictionary xDictionary = comparer != null ? new nStringDictionary (comparer) : new nStringDictionary ();
            xDictionary.LoadIniLikeString (value);
            return xDictionary;
        }
    }
}
