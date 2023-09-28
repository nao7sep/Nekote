using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Nekote;

namespace ConsoleTester
{
    internal class Program
    {
#pragma warning disable IDE0060
        static void Main (string [] args)
#pragma warning restore IDE0060
        {
            try
            {
            }

            catch (Exception xException)
            {
                nException.LogConcurrently (xException);
                nConsole.WriteErrorHasOccurredMessage (xException);
                nConsole.WritePressAnyKeyToCloseThisWindowMessage ();
            }
        }
    }
}
