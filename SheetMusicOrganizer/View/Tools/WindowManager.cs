using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Microsoft.Win32;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.View.Windows;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Tools
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

        public static void OpenFirstTimeWindow()
        {
            OpenOptionWindow(new FirstTimeWindow());
        }

        public static void OpenSettingsWindow()
        {
            OpenOptionWindow(new SettingsWindow());
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

        public static void OpenErrorAsMainWindow(Exception exception, string? customMessage = null)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(() => {
                _openedErrorWindow?.Close();
                _openedErrorWindow = new ErrorWindow(null, exception, customMessage);
                _openedErrorWindow.Closed += OpenedMainWindow_Closed;
                _openedErrorWindow.Show();
                _openedErrorWindow.Activate();
                _openedErrorWindow.Focus();
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
        private static void OpenedMainWindow_Closed(object? sender, EventArgs e)
        {
            if (_openedOptionWindow == sender)
                _openedOptionWindow = null;
            else if (_openedErrorWindow == sender)
                _openedErrorWindow = null;

            Application.Current.Shutdown();
        }

        public static bool OpenOpenLibraryWindow(bool openDatabase)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                Multiselect = false,
                InitialDirectory = Settings.Default.UserDir,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    if (openDatabase)
                        DbHandler.OpenDatabase(openDialog.FileName);
                    else
                        DbHandler.SaveOpenedDbSettings(openDialog.FileName);
                }
                catch (Exception ex)
                {
                    GlobalEvents.raiseErrorEvent(ex);
                }
                return true;
            }
            return false;
        }

        public static bool OpenCreateLibraryWindow(bool openDatabase)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                InitialDirectory = Settings.Default.UserDir
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Create(saveFileDialog.FileName);
                try
                {
                    if (openDatabase)
                        DbHandler.OpenDatabase(saveFileDialog.FileName);
                    else
                        DbHandler.SaveOpenedDbSettings(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    GlobalEvents.raiseErrorEvent(ex);
                }
                return true;
            }
            return false;
        }
    }
}
