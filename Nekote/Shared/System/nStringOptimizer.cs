using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nStringOptimizer
    {
        /// <summary>
        /// Optimize に options を指定しないか、しても変更しないなら、マルチスレッドでも lock 不要。
        /// </summary>
        public static readonly nStringOptimizer Default = new nStringOptimizer ();

        public nStringOptimizationResult Optimize (string value, nStringOptimizationOptions? options = null, string? newLine = null)
        {
            // value が null でも落ちないようにした

            nStringOptimizationOptions xOptions = options ?? nStringOptimizationOptions.iDefault;
            string xNewLine = newLine ?? Environment.NewLine;

            string? iWidthToString (int? width)
            {
                if (width != null)
                    return new string ('\x20', width.Value);

                else return null;
            }

            string? xIndentationTabString = iWidthToString (xOptions.IndentationTabWidth),
                xIndentationNoBreakSpaceString = iWidthToString (xOptions.IndentationNoBreakSpaceWidth),
                xIndentationIdeographicSpaceString = iWidthToString (xOptions.IndentationIdeographicSpaceWidth);

            string? xInlineTabString = iWidthToString (xOptions.InlineTabWidth),
                xInlineNoBreakSpaceString = iWidthToString (xOptions.InlineNoBreakSpaceWidth),
                xInlineIdeographicSpaceString = iWidthToString (xOptions.InlineIdeographicSpaceWidth);

            nStringLineReader xReader = new nStringLineReader (value ?? string.Empty, xOptions.TrimsTrailingWhiteSpaces, xOptions.ReducesEmptyLines);
            StringBuilder xBuffer = new StringBuilder (/* 行数が分からず、必要ならすぐ大きくなるため指定は不要 */);
            List <(string? IndentationString, string? VisibleString)> xLines = new ();

            while (xReader.ReadLine (out ReadOnlySpan <char> xResult))
            {
                // まずはインデント部分の長さを調べる
                // TrimsTrailingWhiteSpaces == false の場合、
                //     xIndentationLength == xResult.Length になることも

                int xIndentationLength = 0;

                while (xIndentationLength < xResult.Length)
                {
                    if (char.IsWhiteSpace (xResult [xIndentationLength]) == false)
                        break;

                    xIndentationLength ++;
                }

                // インデントがあれば設定され、なければ null になる
                string? xIndentationString = null;

                if (xIndentationLength > 0 && xOptions.RemovesIndentation == false)
                {
                    xBuffer.Clear ();

                    for (int temp = 0; temp < xIndentationLength; temp ++)
                    {
                        char xChar = xResult [temp];

                        if (xChar == '\t' && xIndentationTabString != null)
                            xBuffer.Append (xIndentationTabString);

                        else if (xChar == '\xA0' && xIndentationNoBreakSpaceString != null)
                            xBuffer.Append (xIndentationNoBreakSpaceString);

                        else if (xChar == '\x3000' && xIndentationIdeographicSpaceString != null)
                            xBuffer.Append (xIndentationIdeographicSpaceString);

                        else xBuffer.Append (xChar);
                    }

                    // xIndentationLength > 0 なら長さが0になることはまずないが、
                    //     IndentationNoBreakSpaceWidth == 0 で消すなども一応は想定

                    if (xBuffer.Length > 0)
                        xIndentationString = xBuffer.ToString ();
                }

                // 見える文字があれば設定され、なければ null になる
                // 行末を削らないモードで、そこに空白系文字が多数あろうと、少なくとも1文字は見える
                string? xVisibleString = null;

                if (xIndentationLength < xResult.Length)
                {
                    xBuffer.Clear ();

                    if (xOptions.ReducesInlineWhiteSpaces)
                    {
                        // 行末の空白系文字も削減されるのは仕様とする
                        // そうならないように、行頭、行中、行末に分けて処理すると、処理コストが増大する
                        // 行中の空白系文字を削るのに行末のものは残したいとのニーズを考えにくい

                        // 最初の文字は、そこがインデント部分の終わりとなったのだから、絶対に空白系文字でない
                        // しかし、ここではシンプルな for ループにすることを優先

                        bool xHasDetectedWhiteSpace = false;

                        for (int temp = xIndentationLength; temp < xResult.Length; temp ++)
                        {
                            char xChar = xResult [temp];

                            if (char.IsWhiteSpace (xChar))
                            {
                                if (xHasDetectedWhiteSpace == false)
                                {
                                    xBuffer.Append ('\x20');
                                    xHasDetectedWhiteSpace = true;
                                }
                            }

                            else
                            {
                                xBuffer.Append (xChar);

                                if (xHasDetectedWhiteSpace == true)
                                    xHasDetectedWhiteSpace = false;
                            }
                        }
                    }

                    else
                    {
                        for (int temp = xIndentationLength; temp < xResult.Length; temp ++)
                        {
                            char xChar = xResult [temp];

                            if (xChar == '\t' && xInlineTabString != null)
                                xBuffer.Append (xInlineTabString);

                            else if (xChar == '\xA0' && xInlineNoBreakSpaceString != null)
                                xBuffer.Append (xInlineNoBreakSpaceString);

                            else if (xChar == '\x3000' && xInlineIdeographicSpaceString != null)
                                xBuffer.Append (xInlineIdeographicSpaceString);

                            else xBuffer.Append (xChar);
                        }
                    }

                    // インデント部分と同様、こちらでも長さを0にできなくはない

                    if (xBuffer.Length > 0)
                        xVisibleString = xBuffer.ToString ();
                }

                xLines.Add ((xIndentationString, xVisibleString));
            }

            // インデントの調整の処理は、処理対象が全くないならコードブロックに入る必要すらない
            // たぶんもう少しだけ短く書けるが、分かりやすくしたく、まずはインデントの調整の処理対象を抽出
            // nStringOptimizationOptions のコメントに書いた VSC 方式では、インデントがあるか、見える文字があるか、どちらもあるかの3パターン
            // 処理対象にならない唯一のパターンは、インデントも見える文字もない空の行
            // いずれも null か長さ1以上になる実装なので、null かどうかだけを見れば足りる
            var xIndentationAdjustmentApplicableLines = xLines.Where (x => x.IndentationString != null || x.VisibleString != null);

            // 調整の対象となる行のインデント幅の最小値
            // これが null なのは、最小値が0ということでなく、
            //     そもそも調整の対象となる行がなく、調整の必要がないということ
            int? xMinIndentationLength = null;

            // 調整の対象となる行があるのを確認してからの Min なので、要素がないというエラーにならない

            // 現時点でタブ、ノーブレークスペース、全角空白を扱うが、半角空白に置換されなかったなら、ここで慌てて仮想的に長さを算出することは適さない
            // 算出だけなら可能だが、たとえば「タブ＋半角空白二つ」で6として、そのうち最初の半角空白2文字分だけ削ることになっても、タブを勝手に置換できない
            // といったことから、「インデントを調整したければ、インデント部分を相応に整った状態にする」という暗黙の条件が成立する

            // 調整の対象となる行においては、インデント部分があるから長さが得られるところと、見えるところしかないからインデント部分は null のところの二つを想定

            if (xIndentationAdjustmentApplicableLines.Count () > 0)
                xMinIndentationLength = xIndentationAdjustmentApplicableLines.Min (y => y.IndentationString != null ? y.IndentationString.Length : 0);

            // 削られる場合は一つ目が、増える場合は二つ目が設定される

            int? xIndentationTrimmingLength = null;
            string? xExtraIndentationString = null;

            // 調整の対象となる行が存在し、なおかつ調整の長さの指定により調整の実行が指示されている場合のみ、いずれかを設定
            // 長さが一致すれば、いずれも設定されず、何も行われない

            if (xMinIndentationLength != null && xOptions.MinIndentationLength != null)
            {
                if (xMinIndentationLength > xOptions.MinIndentationLength)
                    xIndentationTrimmingLength = xMinIndentationLength - xOptions.MinIndentationLength;

                else if (xMinIndentationLength < xOptions.MinIndentationLength)
                    xExtraIndentationString = new string ('\x20', xOptions.MinIndentationLength.Value - xMinIndentationLength.Value);
            }

            // 呼び出し側に返す List
            List <(string? IndentationString, string? VisibleString)> xLinesAlt = new ();

            for (int temp = 0; temp < xLines.Count; temp ++)
            {
                var xLine = xLines [temp];

                // 行ごとに複数の変数を見るが、扱う文字列の多くが数行から数百行で、ミリオンなどの単位でないため、微々たるコスト
                // まず変数を見てからそれぞれに for ループを入れることも考えたが、コードを重複させるだけの利益がない

                // 削る場合、削れるものがないと処理できない
                // 削れるものがあるなら、xIndentationAdjustmentApplicableLines.Min により、
                //     「絶対その文字数までは削ってよい」という削り方になっている

                // 仕様変更
                // 元々は、この場でインデント部分とインライン部分を結合して List <string> に入れていたが、分けたまま入れることにした
                // ここで結合しても、nStringOptimizationResult.Value のために結合してもコストが同じなので、より生データに近い方を残す
                // たとえば HTML 化するならインライン部分のみをタグに入れたいことも考えられる
                // なお、双方の部分で「ないなら null」が保証されるのを確認した

                if (xIndentationTrimmingLength != null && xLine.IndentationString != null)
                    xLinesAlt.Add ((xLine.IndentationString.Substring (xIndentationTrimmingLength.Value), xLine.VisibleString));

                // 足す場合、その行が上述した「調整の対象となる行」であるかを見る必要がある

                else if (xExtraIndentationString != null && (xLine.IndentationString != null || xLine.VisibleString != null))
                    xLinesAlt.Add ((xExtraIndentationString + xLine.IndentationString, xLine.VisibleString));

                else xLinesAlt.Add ((xLine.IndentationString, xLine.VisibleString));
            }

            return new nStringOptimizationResult
            (
                xLinesAlt,
                string.Join (xNewLine, xLinesAlt.Select (x => x.IndentationString + x.VisibleString)),
                xLinesAlt.Count (x => x.VisibleString != null),

                // xMinIndentationLength が null なら、調整の対象となる行が一つもなかったということ
                // 行が一つもない場合や全ての行が空の場合が当てはまる
                // この場合、最小のインデント幅は0になる

                // null でない場合、調整の対象となる行が少なくとも一つはあった
                // そこにオプションで調整が指示されたなら、基本的に失敗しない処理なので、指定された長さになっている
                // 調整が指示されなかったなら、xIndentationAdjustmentApplicableLines.Min で得られた xMinIndentationLength の値をそのまま返せる

                xMinIndentationLength != null ? (xOptions.MinIndentationLength != null ? xOptions.MinIndentationLength.Value : xMinIndentationLength.Value) : 0
            );
        }
    }
}
