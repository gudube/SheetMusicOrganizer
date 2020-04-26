using Microsoft.Win32;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for SongsGrid.xaml
    /// </summary>
    public partial class SongsGrid : UserControl
    {
        public SongsGrid()
        {
            InitializeComponent();
        }

        //TODO: Accept drag-and-drop
        //TODO: Hide button after a song is added
        //TODO: Add this option in File->Add Song
        //TODO: Directory import (batch import, see performance with taglib)
        //TODO: Test more formats
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Audio (*.mp3, *.flac)|*.mp3;*.flac|All Files (*.*)|*.*",
                Multiselect = true,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                foreach (string fileName in openDialog.FileNames)
                    ((LibraryVM)this.DataContext).AddSongFileCommand.Execute(fileName);
            }
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
        }

        //TODO: Be able to copy paste songs between playlists
        //TODO: Be able to drag-and-drop songs to mastery levels
        //TODO: Add warning when deleting song from All Music
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                ((LibraryVM)this.DataContext).RemoveSelectedSongsCommand.Execute(null);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.DataContext is LibraryVM libraryContext)
            {
                List<SongItem> selectedSongs = ((DataGrid)sender).SelectedItems.Cast<SongItem>().ToList();
                libraryContext.SelectedSongs = new ObservableCollection<SongItem>(selectedSongs);
            }
        }
    }
}
