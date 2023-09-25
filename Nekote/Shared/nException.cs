using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nException: Exception
    {
        /// <summary>
        /// 自動 lock。内部的には nExceptionLogger.Default が使われる。
        /// </summary>
        public static void LogConcurrently <ExceptionType> (ExceptionType exception, DateTime? utc = null)
            where ExceptionType: Exception
        {
            lock (nExceptionLogger.Locker)
            {
                nExceptionLogger.Default.Log (exception, utc);
            }
        }

        /// <summary>
        /// StackTrace 部分の at 直前の3文字の半角空白を4文字にする。
        /// </summary>
        public static string AdjustIndentationWidth (string exceptionString, int width = 0, string? newLine = null)
        {
#if DEBUG
            // 処理対象の特殊なメソッドであり、引数が null や空では呼び出し側のミスの可能性が高い
            // リリース版でも積極的に落とすほどのことでないため、こういう書き方にしている

            if (string.IsNullOrEmpty (exceptionString))
                throw new nArgumentException ();
#else
            // nString.EnumerateLines は null や空でも落ちないが、null 対応は各メソッドの最上位で明示的に
            // コストが小さいため、内部で呼び出すメソッドが見ると期待して、そうでなく落ちることの回避を優先

            // if 文が共通だが、いわゆる「プリプロセッサディレクティブ」の使用においては、ブロック単位の記述を心掛ける

            if (string.IsNullOrEmpty (exceptionString))
                return exceptionString;
#endif
            string xIndentationString = new string ('\x20', width);

            return string.Join (newLine ?? Environment.NewLine, exceptionString.EnumerateLines ().Select (x =>
            {
                // 例外情報にインデントが入るのは、StackTrace.ToString の sb.Append ("   ").Append (word_At).Append (' ') によるものだけ
                // ソースをザッと見た限り、innerException でインデントが増えるわけでないし、Data に情報を入れても Exception.ToString では無視される

                // Exception.cs
                // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Exception.cs

                // StackTrace.cs
                // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/StackTrace.cs

                // 可読性のため半角空白を \x20 と書くことにしているが、ここでそうすると後続の a とつながる

                // 行が半角空白三つ + at で始まっている場合のみ処理が行われるため、三つ以外なら影響を受けない

                if (x.StartsWith ("\x20\x20 at\x20"))
                    return xIndentationString + '\x20' + x;

                return xIndentationString + x;
            }));
        }

        // 最初の設計では、Nekote の例外クラスのインスタンスが生成されるたびにそれが全て自動的にログに入るようにした
        // しかし、それでは、呼び出し側が例外を catch で捕捉してログに入れたときに二重登録になる

        public nException ()
        {
            // LogConcurrently (this);
        }

        public nException (string message): base (message)
        {
            // LogConcurrently (this);
        }

        public nException (string message, Exception inner): base (message, inner)
        {
            // LogConcurrently (this);
        }
    }
}
