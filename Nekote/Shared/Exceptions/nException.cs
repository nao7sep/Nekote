using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nException: Exception
    {
        public nException ()
        {
        }

        public nException (string message): base (message)
        {
        }

        public nException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
