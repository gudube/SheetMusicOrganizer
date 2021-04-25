using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerForDrummers.ViewModel
{
    /*
     * For events that can happen anywhere in code (ex: catching an exception) and does not need context.
     */
    static class GlobalEvents
    {
        public static event ErrorEventHandler? ErrorMessage;

        public static void raiseErrorEvent(Exception exception)
        {
            ErrorMessage?.Invoke(null, new ErrorEventArgs(exception));
        }
    }
}
