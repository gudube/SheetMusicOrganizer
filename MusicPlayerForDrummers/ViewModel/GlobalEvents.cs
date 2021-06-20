using System;
using System.IO;

namespace MusicPlayerForDrummers.ViewModel
{
    /*
     * For events that can happen anywhere in code (ex: catching an exception) and does not need context.
     */
    static class GlobalEvents
    {
        public static event ErrorEventHandler? ErrorMessage;

        public static void raiseErrorEvent(Exception exception, string? userFriendlyMessage = null)
        {
            if(userFriendlyMessage is not null)
                exception.Data.Add("userFriendlyMessage", userFriendlyMessage);
            ErrorMessage?.Invoke(null, new ErrorEventArgs(exception));
        }
    }
}
