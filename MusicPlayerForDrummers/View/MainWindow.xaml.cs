using MusicPlayerForDrummers.ViewModel;
using System;
using System.Windows;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: Add Load Playlist, Save Playlist
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddNewSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow();
        }
    }
}
