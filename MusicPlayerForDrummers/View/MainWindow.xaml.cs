using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        //TODO: Add Load Playlist, Save Playlist
        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.File("log.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 20000000,
                    retainedFileCountLimit: 15)
                .CreateLogger();
            InitializeComponent();
            foreach (string recentDB in Settings.Default.RecentDBs)
            {
                MenuItem recentDBItem = new MenuItem { Header = recentDB };
                recentDBItem.Click += (s, e) => MainVm.LoadDatabase(recentDB);
                FileMenuItem.Items.Add(recentDBItem);
            }
        }

        private void AddNewSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow(); //TODO: Add update button when song already existing
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

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                MainVm.PlayerVM.ChangeMuteCommand?.Execute(null);
                e.Handled = true;
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainVM mainVM)
                await mainVM.LoadData();

        }
    }
}
