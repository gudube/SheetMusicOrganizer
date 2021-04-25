using System;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using MusicPlayerForDrummers.View.Windows;

namespace MusicPlayerForDrummers.View.Tools
{
    public static class WindowManager
    {
        private static Window? _openedOptionWindow;
        private static Window? _openedErrorWindow;

        public static bool IsWindowOpen()
        {
            return _openedOptionWindow != null || _openedErrorWindow != null;
        }

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
            Dispatcher.CurrentDispatcher.BeginInvoke(() =>
            {
                _openedErrorWindow?.Close();
                _openedOptionWindow?.Close();
                _openedOptionWindow = window;
                _openedOptionWindow.Closed += OpenedWindow_Closed;
                DarkenBackground(true);
                _openedOptionWindow.ShowDialog();
            });
        }

        public static void OpenErrorWindow(Exception exception, string? customMessage = null, Window? parent = null)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(() => {
                _openedErrorWindow?.Close();
                _openedErrorWindow = new ErrorWindow(parent ?? Application.Current.MainWindow, exception, customMessage);
                _openedErrorWindow.Closed += OpenedWindow_Closed;
                DarkenBackground(true);
                _openedErrorWindow.ShowDialog();
            });
        }

        private static void DarkenBackground(bool darken)
        {
            Application.Current.MainWindow.Opacity = darken ? 0.5 : 1;
            Application.Current.MainWindow.Effect = darken ? new BlurEffect() : null;
        }

        private static void OpenedWindow_Closed(object? sender, EventArgs e)
        {
            if(_openedOptionWindow == sender)
                _openedOptionWindow = null;
            else if(_openedErrorWindow == sender)
                _openedErrorWindow = null;

            if(_openedErrorWindow == null && _openedOptionWindow == null)
                DarkenBackground(false);
        }
    }
}
