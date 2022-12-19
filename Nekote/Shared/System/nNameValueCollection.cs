using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // URL のクエリー文字列や古い AppSettings などが NameValueCollection で得られる → URL のクエリー文字列も実装が一新されているようだ
    // このクラスは、設計にいくつもの問題がある
    // 幸い、ごくたまにしか遭遇しないものだが、たまに遭遇すれば、あれこれとチェックするのがめんどくさい
    // そのため、パフォーマンスを低下させることなく最善の処理を行えるラッパーを用意

    // NameValueCollection Class (System.Collections.Specialized) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.namevaluecollection

    // NameValueCollection.cs
    // https://source.dot.net/#System.Collections.Specialized/System/Collections/Specialized/NameValueCollection.cs

    // NameObjectCollectionBase.cs
    // https://source.dot.net/#System.Collections.Specialized/System/Collections/Specialized/NameObjectCollectionBase.cs

    // NameValueCollection の問題

    // Comparer を取得できない
    // Comparer は基底クラスの方で internal になっている
    // 後述する ArrayList と Hashtable の併存による複雑な仕様も関係してのことか

    // ContainsKey がない
    // Comparer がなく、ContainsKey もないため、キーが既存か調べるという基本的なことすら確実には行えない
    // 当時の設計では this または Get に string? name を与えて null か見てもらいたかったようだが、
    //     その null が何なのかも不詳という問題がある（詳しくは後述する）
    // となると、Keys/AllKeys を自分で見るしかないが、Comparer が分からない
    // 自分が new する NameValueCollection なら問題でないが、
    //     ライブラリーなどから得られたものの場合、Comparer の詳細をソースまたはドキュメントで調べることになる
    // その場合、決め打ちの処理になるため、ライブラリーなどの側に変更があれば、Comparer の不一致による問題が起こりうる

    // Comparer を推測できなくはないが、現実的でない
    // 大文字・小文字を変更できるキーがあるなら、そうしての Get により case-sensitivity を調べられる
    // ない場合、読み取り専用のインスタンスでないなら、衝突の可能性の極めて低い文字列をキーとしての Add により同様の調査が可能
    // カルチャーも必要なら、合字など、特殊なものを使うことで、Ordinal 系か InvariantCulture 系かくらいは分かる
    // しかし、とにかくスマートでない
    // NameValueCollection は、それを使っているライブラリーをその理由で使わないほど目の上のたんこぶ
    // ConfigurationManager が NuGet の外部パッケージになったのも、そういったこととの関係を疑わざるを得ない
    // テスト用途を除き、自分が new することはまずないクラス
    // どうしてもデータの読み出しが必要なところでは、ソースまたはドキュメントで Comparer を調べればよい
    // といった理由により、Comparer を調べず、必要に応じて引数として受け取る仕様とする

    // Keys の実装がよく分からない
    // new KeysCollection (this) → this [index] → Get (index) → _coll.BaseGetKey (index) → ((NameObjectEntry) _entriesArray [index]!).Key
    // KeysCollection 型のインスタンスに、基底クラスである NameObjectCollectionBase を this で入れ、
    //     後述する、ArrayList と Hashtable の併存における前者から NameObjectEntry のインスタンスを取得し、そのフィールドである Key を this/Get で返す
    // たぶん NameObjectKeysEnumerator のために KeysCollection を作ったのだろうが、無駄なことをしすぎていると感じる
    // boxing/unboxing は、以前調べたところ、実用に耐えないほど遅いわけでもなかった
    // よく分からない実装だが、AllKeys も微妙なので、コレクションの使い方に基づき二つを使い分けるべき

    // AllKeys が内部データと同期していない
    // Add/Set により InvalidateCachedArrays が呼ばれる
    // これは string? []? _allKeys などを null にする
    // AllKeys は、_allKeys が null だと BaseGetAllKeys から新しい配列を取得する
    // このメソッドは、for ループにより既存のキーを新しい配列にコピーする

    // Keys と AllKeys の使い分け
    // クエリー文字列や AppSettings など、読み取りのみ行うところでは、
    //     つまり、コレクションの内容を変更しないところでは、初回に全てキャッシュする AllKeys は悪くない
    // 一方、「ContainsKey が false なら追加」という典型的な処理においては、Keys の方が無駄がない
    // これを AllKeys で行うと、追加のたびにキャッシュが破棄され、全てのキーをコピーしてから一つを探すことの繰り返しになる

    // キーに null を設定できる
    // わざわざ NameObjectEntry? _nullKeyEntry が用意されている
    // .NET の各所で null と "" は同等とみなされるが、NameValueCollection のキーでは積極的に区別される

    // クエリー文字列を構文解析したものの表現に null キーが必要だった記憶があるが、
    //     しばらくプログラミングから離れていた間に HttpRequest.QueryString → Microsoft.AspNetCore.Http.QueryString、
    //     HttpRequest.Query → Microsoft.AspNetCore.Http.IQueryCollection と変わったようなので、確認しない

    // HttpRequest.QueryString Property (Microsoft.AspNetCore.Http) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.querystring

    // HttpRequest.Query Property (Microsoft.AspNetCore.Http) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.query

    // そもそも name なのか key なのか
    // クラス名や引数名では name なのに、プロパティーは *Keys
    // Contains* も、変更できないプロパティー名に合わせて ContainsKey とした

    // ArrayList _entriesArray と Hashtable _entriesTable の併存
    // Dictionary にも Entry []? _entries と ValueCollection? _values があるが、
    //     Hashtable には Bucket [] _buckets と ICollection? _values があるため、さらに ArrayList _entriesArray が必要な理由は不詳
    // いずれもジェネリックがなかった頃のクラスなので、boxing/unboxing が増えすぎては、減らすためのクラスを追加したくなったか

    // Dictionary.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs

    // Hashtable.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Collections/Hashtable.cs

    // それぞれのエントリーの値側が ArrayList? になっている
    // NameValueCollection は、実質的には Dictionary <string, List <string>> に近い
    // 各キーに対する値が必ず一つのケースでは、List のようなものが必ず生成される実装には無駄が多い

    // Get のたびに GetAsOneString が呼ばれる
    // 値が一つなら [0] を返す実装なので読み出しのコストは許容範囲内
    // 値が複数のときに非可逆的に , で結合されるのは、設計としてヤバいとしか言いようがない
    // 値に , が含まれていれば、, で区切るときに、より多くの値が得られる

    // null が得られたときに、それが何なのか不詳
    // ドキュメントにもあるが、Get/this は、キーがない場合、キーがあっても値がない場合、値があっても一つだけでそれが null の場合の（少なくとも）三つで null を返す
    // もっとも、「結局、そのキーで有効な値を得られたのか」という点のみ評価すれば処理を続行できることも多い
    // null が何を示すのか調べる必要があるなら、nNameValueCollection.ContainsKey を試す

    // 値として null を与えたときの挙動が Set と Add で異なる
    // Set は、値が null 一つの ArrayList を必ず作り、それによる既存のエントリーの値側の上書きまたは新規エントリーの作成を行う
    // Add は、値が null なら、なぜか値側の ArrayList にそれを追加しない
    // そのため、エントリーが既存でも値が null なら Add 自体が呼ばれなかったかのように振る舞い、既存でないなら値側が空の新規エントリーを作成する
    // Add (..., null) のあと Get (...) で null が返るため分かりにくいが、与えた null が返ってきたのではない
    // この不可解な null の無視による実害は、値側の ArrayList の長さがイメージ通りにならないこと
    // といったことに影響されないためには、nNameValueCollection.AddString を使う

    public class nNameValueCollection: NameValueCollection
    {
        // ラッパーなので、既に存在する NameValueCollection のインスタンスなしには初期化できなくしている

        public nNameValueCollection (NameValueCollection collection): base (collection)
        {
        }

        // ToFriendlyString では、AllKeys.OrderBy のところで IComparer の中身が string? でないと怒られる
        // こちらでは ? なしで問題ないようだが、Keys に null キーも入るのが確認されているため一応

        public bool ContainsKey (string? name, IEqualityComparer <string?>? comparer = null)
        {
            // 先述した理由により Keys の方を使う
            // null キーは、最初は Keys に含まれず、一度 Set などを行えば、それからは含まれる
            // ここで name == null の場合を個別に見る必要はない

            // new 時のデフォルトの comparer は、CultureInfo.InvariantCulture.CompareInfo.GetStringComparer (CompareOptions.IgnoreCase)
            // 完全に同一かは分からないが、InvariantCulture 系の IgnoreCase なので、それにならっている
            IEqualityComparer <string?> xComparer = comparer ?? StringComparer.InvariantCultureIgnoreCase;

            // NameObjectKeysEnumerator.Current → _coll.BaseGetKey (_pos)
            // ここには null キーを含む全てが入るため、xName は null になることがある

            foreach (string? xName in Keys)
            {
                if (xComparer.Equals (xName, name))
                    return true;
            }

            return false;
        }

        // ラッパーのメソッドには型名を入れる
        // 気が向けば GetInt32 なども追加する可能性がある
        // this も Get なので、オーバーヘッドは気にしなくてよい

        public string? GetString (string? name)
        {
            // 許容範囲内の実装
            // 値が複数のときに , 区切りになるのも、たとえば ", " での結合にして実利があるわけでない
            return Get (name);
        }

        public string? GetStringOrDefault (string? name, string? value)
        {
            string? xValue = Get (name);

            if (string.IsNullOrEmpty (xValue) == false)
                return xValue;

            return value;
        }

        public string? GetString (int index)
        {
            // 添え字による単純アクセスなので、これでよい
            return Get (index);
        }

        public string? []? GetStrings (string? name)
        {
            // 内部で呼ばれる GetAsStringArray の戻り値は string []? だが、
            //     Set などが値側に null を流し込むので、string? []? であるべき
            // 実装は許容範囲内なので、そのまま呼ぶ
            return GetValues (name);
        }

        // GetStringsOrDefault は、使いどころを想定しにくいので用意しない

        public string? []? GetStrings (int index)
        {
            return GetValues (index);
        }

        public void SetString (string? name, string? value)
        {
            // イメージ通りの挙動なのを確認した
            Set (name, value);
        }

        public void AddString (string? name, string? value)
        {
            // NameValueCollection.Add の実装を拝借
            // SR クラスが internal なので、GetResourceString の defaultString で妥協

            // System.SR.cs
            // https://source.dot.net/#System.Collections.Specialized/artifacts/obj/System.Collections.Specialized/Debug/net8.0/System.SR.cs

            // SR.cs
            // https://source.dot.net/#System.Collections.Specialized/src/libraries/Common/src/System/SR.cs

            // 通常、Nekote の実装では Nekote の例外クラスを投げる
            // しかし、ここは、.NET の Add がもうちょっとちゃんとしていて、それをラップしただけのイメージ
            // あまりこういうことをしたくないが、Add においてのみ null が無視されるよりマシな挙動

            if (IsReadOnly)
                // throw new NotSupportedException (SR.CollectionReadOnly);
                throw new NotSupportedException ("Collection is read-only.");

            InvalidateCachedArrays();

            ArrayList? values = (ArrayList?) BaseGet (name);

            if (values == null)
            {
                values = new ArrayList (1);

                // if (value != null)
                    values.Add (value);

                BaseAdd (name, values);
            }

            else
            {
                // if (value != null)
                    values.Add (value);
            }
        }

        // キーが複数行の場合に表示が崩れるのを想定の上、
        //     user-friendly な文字列、つまり、可逆性より人間の可読性を優先した文字列にするメソッド
        // プログラマーがエラー発生時のログに含めるなどの用途を主に想定
        // 緊急時には、いずれ実装するローカライズの機能が動かないことも考えられる
        // そのため、nStringLiterals のフィールドの値を埋め込む

        public string ToFriendlyString (IComparer <string?>? comparer = null, string? newLine = null)
        {
            if (Keys.Count == 0)
                return nStringLiterals.EmptyLabel;

            string? xNewLine = newLine ?? Environment.NewLine;

            StringBuilder xBuilder = new StringBuilder (/* 不詳 */);

            // インデント幅の調整に
            nStringOptimizationOptions xOptions = new nStringOptimizationOptions ();

            // 値側の ArrayList の要素のうち一つを最適化し、
            //     0～1行なら右に、複数行ならインデントを付けて下に出力

            void iAppend (int minIndentationLength, string value)
            {
                xOptions.MinIndentationLength = minIndentationLength;
                nStringOptimizationResult xResult = nStringOptimizer.Default.Optimize (value, xOptions, xNewLine);

                // 行がないのは、値が最適化により削られきったということ
                // user-friendly な出力だし、Nekote 全体で「見えない文字列」を重く扱わないため、この出力でよい

                if (xResult.VisibleLineCount == 0)
                    xBuilder.Append ($"\x20{nStringLiterals.EmptyLabel}"); // OK

                else if (xResult.VisibleLineCount == 1)
                    xBuilder.Append ($"\x20{xResult.Lines [0].VisibleString}"); // OK

                else xBuilder.Append ($"{xNewLine}{xResult.Value}"); // OK
            }

            // Keys では OrderBy を使えない
            // ContainsKey と異なり、追加のたびに呼ばれるなどのメソッドでないため、
            //     全てのキーが内部的にキャッシュされる AllKeys でよい

            // 基本的には NameValueCollection のデフォルトと同等の comparer
            // ユーザーに表示する場合には、そのユーザーのロケールの指定を検討

            foreach (string? xName in AllKeys.OrderBy (x => x, comparer ?? StringComparer.InvariantCultureIgnoreCase))
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (xNewLine);

                // 値が null または "" または1行
                // フィールドやプロパティーには : + 半角空白、配列の各要素には [...] + 半角空白
                // いずれも後者の形式にすると、フィールドやプロパティーが視覚的に分かりにくい
                // 0: ... というのも違和感がある

                // 値が複数行
                // 1行目に : や [...] が入るのは美しくない
                // 次の行に同じインデントで置くと、キーなのか値なのか分かりにくい
                // といったことから、次の行に、1段分のインデントを加えて出力

                xBuilder.Append ($"{nString.GetLiteralIfNullOrEmpty (xName)}:");

                string? []? xValues = GetStrings (xName);

                // 上述した、null が返る三つの場合に対応
                // GetAsStringArray と GetAsOneString の実装を再確認した
                // ここでこれら三つを区別しての表示には利益が乏しい

                // 細かいことだが、値が一つだけあって null の場合だけでなく、
                //     値がない場合の表示でもあるので、nString.GetLiteralIfNullOrEmpty を通さない

                if (xValues == null || xValues.Length == 0 || (xValues.Length == 1 && xValues [0] == null))
                    xBuilder.Append ($"\x20{nStringLiterals.NullLabel}");

                // xValues [0] == null の場合を既に見たので、ここでは null でない
                // 値が一つなので、添え字を伴わない1行の表示として、インデントは1段分になる

                else if (xValues.Length == 1)
                    iAppend (minIndentationLength: 4, xValues [0]!);

                else
                {
                    // 値が一つ以上あり、いずれも null であり得る場合
                    // 添え字の表示は確定
                    // 値が "" 以上（？）なら iAppend がうまく処理する
                    // null の場合のみ、先ほどの三つのケースのところのコードと同様に

                    for (int temp = 0; temp < xValues.Length; temp ++)
                    {
                        xBuilder.Append (FormattableString.Invariant ($"{xNewLine}\x20\x20\x20\x20[{temp}]"));

                        if (xValues [temp] == null)
                            xBuilder.Append ($"\x20{nStringLiterals.NullLabel}");

                        else iAppend (minIndentationLength: 8, xValues [temp]!);
                    }
                }
            }

            return xBuilder.ToString ();
        }
    }
}
