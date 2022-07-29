using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nDebugException: nException
    {
        public nDebugException ()
        {
        }

        public nDebugException (string message): base (message)
        {
        }

        public nDebugException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
