using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nException: Exception
    {
        public static void LogSafely <T> (T exception) where T: Exception
        {
            lock (nExceptionLogger.Lock)
            {
                nExceptionLogger.Default.Add (DateTime.UtcNow, exception);
            }
        }

        public nException ()
        {
            LogSafely (this);
        }

        public nException (string message): base (message)
        {
            LogSafely (this);
        }

        public nException (string message, Exception inner): base (message, inner)
        {
            LogSafely (this);
        }
    }
}
