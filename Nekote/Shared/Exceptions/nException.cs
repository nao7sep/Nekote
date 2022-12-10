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
