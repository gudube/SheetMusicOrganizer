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
            if(_openedWindow != null)
            {
                _openedWindow.Closed -= OpenedWindow_Closed;
                _openedWindow.Close();
            }
            _openedWindow = new AddNewSongWindow();
            _openedWindow.Closed += OpenedWindow_Closed;
            _openedWindow.ShowDialog();
        }

        public static void OpenOpenFolderWindow()
        {
            if (_openedWindow != null)
            {
                _openedWindow.Closed -= OpenedWindow_Closed;
                _openedWindow.Close();
            }
            _openedWindow = new OpenFolderWindow();
            _openedWindow.Closed += OpenedWindow_Closed;
            _openedWindow.ShowDialog();
        }

        public static void OpenErrorWindow(Exception exception, string? customMessage = null, bool closeOtherWindows = false)
        {
            if (closeOtherWindows && _openedWindow != null)
            {
                _openedWindow.Closed -= OpenedWindow_Closed;
                _openedWindow.Close();
            }
            ErrorWindow errorWindow = new ErrorWindow(_openedWindow ?? Application.Current.MainWindow, exception, customMessage);
            errorWindow.ShowDialog();
        }

        private static void OpenedWindow_Closed(object? sender, EventArgs e)
        {
            if(_openedWindow != null)
            {
                _openedWindow.Closed -= OpenedWindow_Closed;
                _openedWindow = null;
            }
        }
    }
}
