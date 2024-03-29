﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : Window
    {

        private FileStream? lockedFs;
        public MainWindow()
        {
            //bool firstUse = Settings.Default.RecentDBs.Count == 0;
            InitializeComponent();
            Loaded += (s, a) => {
                GlobalEvents.ErrorMessage += Status_ErrorMessage;
                var openedDb = Settings.Default.RecentDBs[0];
                if(openedDb != null)
                    lockedFs = File.Open(openedDb, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //if(firstUse)
                //    WindowManager.OpenFirstTimeWindow();
            };
            Unloaded += (s, a) => {
                GlobalEvents.ErrorMessage -= Status_ErrorMessage;
                lockedFs?.Close();
            };
        }

        private void Status_ErrorMessage(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            string? message = ex.Data.Contains("userFriendlyMessage") ? (string?) ex.Data["userFriendlyMessage"] : null;
            WindowManager.OpenErrorWindow(ex, message);
        }

        private void MainWindow_OnKeyDownUp(object sender, KeyEventArgs e)
        {
            if (WindowManager.IsWindowOpen())
                return;

            e.Handled = HandleKey(e, e.IsDown);
        }

        private bool HandleKey(KeyEventArgs e, bool down)
        {
            if (!(e.OriginalSource is TextBoxBase))
            {
                switch (e.Key) //add keyboard keys here (A, 1, !... that could be used in textbox)
                {
                    case Key.M:
                        PressButton(PlayerControl.MuteButton, down);
                        return true; //return if found, break otherwise to try second switch statement
                    default: break;
                }
            }
            switch (e.Key) // insert special keys here (Play, F1... doesnt react in textbox)
            {
                case Key.Play:
                    PressButton(PlayerControl.PlayButton, down);
                    return true;
                case Key.Pause:
                case Key.MediaPlayPause:
                    PressButton(PlayerControl.PauseButton, down);
                    return true;
                case Key.MediaPreviousTrack:
                    PressButton(PlayerControl.PreviousButton, down);
                    return true;
                case Key.MediaNextTrack:
                    PressButton(PlayerControl.NextButton, down);
                    return true;
                default:
                    return false;
            }
        }

        private void PressButton(Button button, bool down)
        {
            if(down)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(button, new object[] { true });
            } else
            {
                ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(button, new object[] { false });
            }
        }

        public async Task Configure()
        {
            if (DataContext is MainVM mainVM)
                await mainVM.LoadData().ConfigureAwait(false);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
