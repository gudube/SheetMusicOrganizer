using System;
using System.Windows;
using MusicPlayerForDrummers.View.Windows;

namespace MusicPlayerForDrummers.View.Tools
{
    public static class WindowManager
    {
        private static Window? _openedWindow;

        public static void OpenAddNewSongWindow()
        {
            OpenOptionWindow(new AddNewSongWindow());
        }

        public static void OpenOpenFolderWindow()
        {
            OpenOptionWindow(new OpenFolderWindow());
        }

        private static void OpenOptionWindow(Window window)
        {
            _openedWindow?.Close();
            _openedWindow = new AddNewSongWindow();
            _openedWindow.Closed += OpenedWindow_Closed;
            _openedWindow.ShowDialog();
        }

        public static void OpenErrorWindow(Exception exception, string? customMessage = null, Window? parent = null)
        {          
            ErrorWindow errorWindow = new ErrorWindow(parent ?? Application.Current.MainWindow, exception, customMessage);
            errorWindow.ShowDialog();
        }

        private static void OpenedWindow_Closed(object? sender, EventArgs e)
        {
            if(_openedWindow == sender)
            {
                _openedWindow = null;
            }
        }
    }
}
