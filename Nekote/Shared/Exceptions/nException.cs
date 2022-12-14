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
        /// 自動 lock。
        /// </summary>
        public static void Log <ExceptionType> (ExceptionType exception)
            where ExceptionType: Exception
        {
            lock (nExceptionLogger.Locker)
            {
                nExceptionLogger.Default.Add (DateTime.UtcNow, exception);
            }
        }

        /// <summary>
        /// StackTrace 部分の at 直前の3文字の半角空白を4文字にする。
        /// </summary>
        public static string AdjustIndentationWidth (string exceptionString, int width = 0, string? newLine = null)
        {
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

                if (x.StartsWith ("\x20\x20 at\x20"))
                    return xIndentationString + '\x20' + x;

                return xIndentationString + x;
            }));
        }

        public nException ()
        {
            Log (this);
        }

        public nException (string message): base (message)
        {
            Log (this);
        }

        public nException (string message, Exception inner): base (message, inner)
        {
            Log (this);
        }
    }
}
