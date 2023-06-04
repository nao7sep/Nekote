using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nDataException: nException
    {
        public nDataException ()
        {
        }

        public nDataException (string message): base (message)
        {
        }

        public nDataException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
