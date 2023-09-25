using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // 入れ物クラスなので、Guid もコンストラクターで設定されるなどがない
    // 値がないと処理に問題のある StartUtc と ElapsedTime 以外はオプションなので Nullable に

    // 追記: nStopwatch をスレッドセーフのクラスと位置づけ、nConcurrentStopwatch としたが、
    //     このクラスのほうは特に変更がなく、スレッドセーフでない実装でも使えるので、名前をそのままとする

    public class nStopwatchEntry <TagType>
    {
        // Pause/Resume を行えば、連続する二つ以上のエントリーの Guid が一致する
        // StartUtc でソートしながら Guid でグループ化することで、ラップタイムなどを扱える
        public Guid? Guid;

        public string? Name;

        public DateTime StartUtc;

        public TimeSpan ElapsedTime;

        // 計測に使われるクラスであり、そのときのパラメーターや、処理の結果などをコレクション的に扱えると便利だろう
        // 「クラスか構造体かすら分からない型」は実装のあいまいさにつながりうるので、どちらも扱えるように両方を個別に用意 → Tag に統一

        // FrameworkElement.Tag Property (System.Windows) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.tag

        public TagType? Tag;
    }
}
