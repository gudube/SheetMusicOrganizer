using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.View.Tools;
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
    }
}
