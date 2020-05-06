using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Linq;
using MusicPlayerForDrummers.ViewModel;

namespace MusicPlayerForDrummers.View
{
    public static class WindowManager
    {
        private static Window openedWindow = null;

        public static void OpenAddNewSongWindow()
        {
            if(openedWindow != null)
                openedWindow.Close();
            openedWindow = new AddNewSongWindow();
            openedWindow.ShowDialog();
        }
    }
}
