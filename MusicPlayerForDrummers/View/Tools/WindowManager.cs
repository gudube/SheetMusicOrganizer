using System;
using System.Windows;
using MusicPlayerForDrummers.View.Windows;

namespace MusicPlayerForDrummers.View.Tools
{
    public static class WindowManager
    {
        private static Window? _openedOptionWindow;
        private static Window? _openedErrorWindow;

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
            _openedErrorWindow?.Close();
            _openedOptionWindow?.Close();
            _openedOptionWindow = new AddNewSongWindow();
            _openedOptionWindow.Closed += OpenedWindow_Closed;
            _openedOptionWindow.ShowDialog();
        }

        public static void OpenErrorWindow(Exception exception, string? customMessage = null, Window? parent = null)
        {
            _openedErrorWindow?.Close();
            _openedErrorWindow = new ErrorWindow(parent ?? Application.Current.MainWindow, exception, customMessage);
            _openedErrorWindow.Closed += OpenedWindow_Closed;
            _openedErrorWindow.ShowDialog();
        }

        private static void OpenedWindow_Closed(object? sender, EventArgs e)
        {
            if(_openedOptionWindow == sender)
                _openedOptionWindow = null;
            else if(_openedErrorWindow == sender)
                _openedErrorWindow = null;
        }
    }
}
