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
    }
}
