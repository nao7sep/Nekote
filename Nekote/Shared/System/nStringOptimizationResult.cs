using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public readonly struct nStringOptimizationResult
    {
        // 処理において最初に得られるのは List <string> だが、呼び出し側で欲しいのはたいてい string の方
        // List <string> を string にしてすぐ破棄するほど一度でメモリーを消費する処理でないため、両方を受け取る
        // HTML のタグを付けるなどで前者の方が欲しいこともある

        public readonly List <string> Lines;

        public readonly string Value;

        // Char.IsWhiteSpace が false の文字が一つでも含まれる行の数
        public readonly int VisibleLineCount;

        // nStringOptimizationOptions のコメントに書いた VSC 方式での最小のインデント幅
        // この値がたとえば3のとき、「空でない全ての行において先頭の3文字を削る」という処理はうまくいく
        // そうすることで範囲外の例外になったり、見える文字が削られたりすることはない
        public readonly int MinIndentationLength;

        public nStringOptimizationResult (List <string> lines, string value, int visibleLineCount, int minIndentationLength)
        {
            Lines = lines;
            Value = value;
            VisibleLineCount = visibleLineCount;
            MinIndentationLength = minIndentationLength;
        }
    }
}
