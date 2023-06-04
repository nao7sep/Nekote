using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nFormatException: nException
    {
        public nFormatException ()
        {
        }

        public nFormatException (string message): base (message)
        {
        }

        public nFormatException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
