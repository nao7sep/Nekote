using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nStringOptimizationOptions
    {
        // nStringLineReader に渡される
        // いずれも具体的に役立つことの少ないものなのでデフォルトで削られる
        // Markdown では行末の半角空白二つで改行になるが、あまり好まれていない

        // How to insert a line break <br> in markdown - Stack Overflow
        // https://stackoverflow.com/questions/26626256/how-to-insert-a-line-break-br-in-markdown

        public bool TrimsTrailingWhiteSpaces = true;
        public bool ReducesEmptyLines = true;

        // オンだとインデント部分の全ての空白系文字が削られる
        // その場合も MinIndentationLength は有効で、
        //     1以上にすると、インデントが削られた行にもインデントが戻る

        public bool RemovesIndentation = false;

        // インデント部分の各文字を半角空白いくつに置換するか

        // タブ
        // Unicode では CHARACTER TABULATION とされるが、冗長
        // ASCII の文字で、キーボードに刻印されるほどメジャーなものは通名でよい
        // ソフトタブが好ましい理由は後述
        // .NET のガイドラインに one tab stop (four spaces) とあるため、幅を4とする

        // The Unicode Standard, Version 15.0
        // https://unicode.org/charts/PDF/U0000.pdf

        // C# Coding Conventions | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

        // ソフトタブが好ましい理由
        // ハードタブの方がインデント1段ごとに数バイト節約でき、各人が好みの幅で表示できる
        // メリットはその程度
        // デメリットは、エディターで入力しようとすると次のコントロールにフォーカスが移動することの多さ、
        //     表示が8文字に固定されていてネストされたコードの閲覧性が低いソフトやコントロールが散見されること、
        //     ソフトタブとハードタブが混在しているところで、幅の設定によっては行頭などが揃わないこと、
        //     HTML 生成時の &nbsp; への変換などにおいて、結局、幅の確定が強いられることなど
        // Nekote は .NET のクラスライブラリーであり、.NET では4という明確な数字が一応は示されている
        // それにならい、4で入力を確定し、その段階で文字列のフォーマットを整えるのがシンプル
        // どこにあろうと上記のデメリットが付きまとうものなので、常に置換されるべき

        // ノーブレークスペース
        // Wikipedia の見出しがこうなっている
        // Unicode でも NO-BREAK SPACE
        // Electron などで多用され、このコードポイントにグリフのないフォントだと表示が崩れることがある
        // JavaScript/TypeScript からの流入も考えられるものなので、単一の半角空白に置換
        // これも、どこだろうと扱いに困るものなので、常に置換されるべき

        // ノーブレークスペース - Wikipedia
        // https://ja.wikipedia.org/wiki/%E3%83%8E%E3%83%BC%E3%83%96%E3%83%AC%E3%83%BC%E3%82%AF%E3%82%B9%E3%83%9A%E3%83%BC%E3%82%B9

        // 全角空白
        // Unicode では IDEOGRAPHIC SPACE
        // full-width space も考えたが、ASCII でないので通名を避けている
        // 不規則に通名を採用すると、今後、外国語で使われる、自分はなじみのない文字にも対応することになったときに不揃いになる
        // 幅を2とするのは常識の範囲内
        // インデント部分に入っていれば、他との混在によりインデントの幅が不定扱いになったり、
        //     CJK 圏でないプログラマーがインデントの一部と判別できなかったりの可能性があるため置換
        // しかし、インライン部分なら、ニュース記事や行政のサイトによく見られる「空白まで全角の英語」のうち区切り文字のみ置換される恐れがある
        // インデント部分では置換されることで破壊されるものを想定しにくいが、インライン部分では日常的にありそうだから、インデント部分においてのみ置換

        // MinIndentationLength は、最適化の終了直前のインデント幅の調整に使われる
        // null なら何も行われない
        // 0以上の整数なら、インデントの文字数が最も少ない行がその数になるように全体が調整される
        // その際、まだタブや全角空白などが残っていれば、それらも char 単位で数えられる点に注意が必要
        // 実際の挙動については、iStringTester.TestStringOptimization が分かりやすい
        // 見える文字がない行にインデントを追加する場合の挙動は、Visual Studio と Visual Studio Code とで異なる
        // 前者では見える文字がなければインデントが広がらず、後者では、見える文字のない、インデントだけの行でも広がる
        // 編集を継続するにおいては後者の方が好都合なので、Nekote では後者のように実装した
        // なお、いずれにおいても、インデントも見える文字もない行は空のまま

        public int? IndentationTabWidth = 4;
        public int? IndentationNoBreakSpaceWidth = 1;
        public int? IndentationIdeographicSpaceWidth = 2;
        public int? MinIndentationLength = 0;

        // 空白系文字は他にも多数ある
        // だからこそ、このクラスを作り、対応する文字が増えてもメソッドの引数が増えないようにしている
        // 多くは幅が狭いようなので Char.IsWhiteSpace が true になるものの残りを全て半角空白一つにすることも考えたが、
        //     「未対応の文字 → N 個の半角空白」の処理はいったん DB に入れた文字列などに対してもいつでも行えるため、急ぐこともない
        // 今すぐに残りを全て置換することは情報の喪失である部分が大きいとの判断

        // Whitespace character - Wikipedia
        // https://en.wikipedia.org/wiki/Whitespace_character

        // これが true だと、インデントでない部分（つまり、見える文字の領域と行末の空白系文字）のあらゆる空白系文字の一つ以上の連続が単一の半角空白に置換される
        // たとえば、「あ（タブ）（全角空白）ほ（タブ）（全角空白）」を処理すれば、「あ（半角空白）ほ（半角空白）」になる
        // これは、コマンドラインのパラメーターだったり、ちょっとした自作スクリプト言語だったりの構文解析のセキュリティーを高める
        // 行末の空白系文字も影響を受けるが、そうならないように実装すると処理のコストが増大する
        // インラインの空白系文字を画一的に潰すときに行末のものを無傷で残したいニーズは考えにくく、まず影響はない
        // このオプションは、人間が読む文字列においては空白系文字列による表現の多くが壊れるため、デフォルトではオフになっている

        public bool ReducesInlineWhiteSpaces = false;

        public int? InlineTabWidth = 4;
        public int? InlineNoBreakSpaceWidth = 1;
        public int? InlineIdeographicSpaceWidth = null;
    }
}
