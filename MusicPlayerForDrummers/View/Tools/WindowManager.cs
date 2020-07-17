using System;
using System.Windows;
using MusicPlayerForDrummers.View.Windows;

namespace MusicPlayerForDrummers.View.Tools
{
    public static class WindowManager
    {
        private static Window openedWindow = null;

        public static void OpenAddNewSongWindow()
        {
            if(openedWindow != null)
            {
                openedWindow.Closed -= OpenedWindow_Closed;
                openedWindow.Close();
            }
            openedWindow = new AddNewSongWindow();
            openedWindow.Closed += OpenedWindow_Closed;
            openedWindow.ShowDialog();
        }

        public static void OpenOpenFolderWindow()
        {
            if (openedWindow != null)
            {
                openedWindow.Closed -= OpenedWindow_Closed;
                openedWindow.Close();
            }
            openedWindow = new OpenFolderWindow();
            openedWindow.Closed += OpenedWindow_Closed;
            openedWindow.ShowDialog();
        }

        private static void OpenedWindow_Closed(object sender, EventArgs e)
        {
            if(openedWindow != null)
            {
                openedWindow.Closed -= OpenedWindow_Closed;
                openedWindow = null;
            }
        }
    }
}
