using System.Windows;
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
        }

        private void AddNewSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow(); //TODO: Add update button when song already existing
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenFolderWindow();
        }
    }
}
