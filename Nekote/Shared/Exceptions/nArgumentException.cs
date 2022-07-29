using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nArgumentException: nException
    {
        public nArgumentException ()
        {
        }

        public nArgumentException (string message): base (message)
        {
        }

        public nArgumentException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
