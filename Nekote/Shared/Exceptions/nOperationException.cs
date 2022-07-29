using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nOperationException: nException
    {
        public nOperationException ()
        {
        }

        public nOperationException (string message): base (message)
        {
        }

        public nOperationException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
