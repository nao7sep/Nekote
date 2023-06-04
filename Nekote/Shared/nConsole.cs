using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nConsole
    {
        public static readonly char [] ProcessingSymbols = { '|', '/', '-', '\\' };

        public static int ProcessingSymbolsCurrentIndex { get; private set; }

        public static char GetNextProcessingSymbol ()
        {
            char xSymbol = ProcessingSymbols [ProcessingSymbolsCurrentIndex];

            if (++ ProcessingSymbolsCurrentIndex >= ProcessingSymbols.Length)
                ProcessingSymbolsCurrentIndex = 0;

            return xSymbol;
        }

        public static void WriteNextProcessingSymbol ()
        {
            Console.Write (GetNextProcessingSymbol ());
        }

        /// <summary>
        /// \r(message)...(symbol) の決め打ち。文字列が短くなるケースに注意。
        /// </summary>
        public static void WriteProcessingMessage (string message)
        {
            Console.Write ($"\r{message}...{GetNextProcessingSymbol ()}");
        }

        // マルチプラットフォーム対応のクラスライブラリーにしたいので、各部で、できるだけ newLine を指定できるようにしている
        // ここでも、nException.AdjustIndentationWidth に指定できるので、上位で潰す必要もなく、デフォルト値ありの引数を用意

        // しかし、Console.WriteLine により内部的に Environment.NewLineConst.ToCharArray の戻り値が使われるのは、問題なしとみなす
        // すぐに描画に使われ、消費される引数だし、Console クラスと OS の API との間のコードがうまく処理する可能性が高いため

        // Console.cs
        // https://source.dot.net/#System.Console/System/Console.cs

        // TextWriter.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/TextWriter.cs

        public static void WriteErrorHasOccurredMessage (Exception exception, string? newLine = null)
        {
            Console.WriteLine ("エラーが発生しました:");
            Console.WriteLine (nException.AdjustIndentationWidth (exception.ToString (), 4, newLine));
        }

        private static void iPause (string message)
        {
            Console.Write (message + ": ");
            Console.ReadKey (true);
            Console.WriteLine ();
        }

        // .bat ファイルに pause と書いて実行すると、「続行するには何かキーを押してください . . . 」と表示される
        // "□.□.□.□" になるのは原文ママ
        // コピーすると . までだが（おそらく行末の半角空白が自動的にトリミングされている）、
        //     画面上では間違いなくその次に1文字分の余白があってのキャレット表示

        // Visual Studio でコンソールアプリを実行すると、「このウィンドウを閉じるには、任意のキーを押してください...」と表示される

        // これらは、英語では、Press any key to continue . . .□ と Press any key to close this window... のようだ

        // 「何かキーを」という表現に違和感があったので、今後、any を「任意の」と訳すことで統一する
        // 「には」の「は」などの提題助詞の直後を読点で区切り、「ください」を漢字にしないことも

        public static void WritePressAnyKeyToContinueMessage ()
        {
            iPause ("続行するには、任意のキーを押してください");
        }

        public static void WritePressAnyKeyToCloseThisWindowMessage ()
        {
            iPause ("このウィンドウを閉じるには、任意のキーを押してください");
        }
    }
}
