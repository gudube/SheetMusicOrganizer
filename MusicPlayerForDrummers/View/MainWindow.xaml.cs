using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Win32;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, a) => {
                GlobalEvents.ErrorMessage += Status_ErrorMessage;
            };
            Unloaded += (s, a) => {
                GlobalEvents.ErrorMessage -= Status_ErrorMessage;
            };
        }

        private void Status_ErrorMessage(object sender, ErrorEventArgs e)
        {
            WindowManager.OpenErrorWindow(e.GetException());
        }

        private void AddNewSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow();
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenFolderWindow();
        }

        private void LoadDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                Multiselect = false,
                InitialDirectory = DbHandler.DefaultDbDir,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                MainVm.LoadDatabase(openDialog.FileName);
            }
        }

        private void NewDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                InitialDirectory = DbHandler.DefaultDbDir
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Create(saveFileDialog.FileName);
                MainVm.LoadDatabase(saveFileDialog.FileName);
            }
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
                        PressButton(PlayerControl.MuteButton, down); // MainVm.PlayerVM.ChangeMuteCommand.Execute(null);
                        return true; //return if found, break otherwise to try second switch statement
                    default: break;
                }
            }
            switch (e.Key) // insert special keys here (Play, F1... doesnt react in textbox)
            {
                case Key.Play:
                case Key.MediaPlayPause:
                    PressButton(PlayerControl.PauseButton, down);  // MainVm.PlayerVM.PauseCommand.Execute(null);
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
                // button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(button, new object[] { false });
            }
        }

        public async Task Configure()
        {
            foreach (string? recentDB in Settings.Default.RecentDBs)
            {
                if (recentDB != null)
                {
                    MenuItem recentDBItem = new MenuItem { Header = recentDB };
                    recentDBItem.Click += (s, e) => MainVm.LoadDatabase(recentDB);
                    FileMenuItem.Items.Add(recentDBItem);
                }
            }

            if (DataContext is MainVM mainVM)
                await mainVM.LoadData().ConfigureAwait(false);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
