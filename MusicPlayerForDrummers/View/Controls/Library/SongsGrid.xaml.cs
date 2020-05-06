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
            DataContextChanged += (sender, args) => ((LibraryVM)DataContext).SelectedSongs.CollectionChanged += SelectedSongs_CollectionChanged;
        }

        private void SelectedSongs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach(SongItem song in e.NewItems)
                        if(!Songs.SelectedItems.Contains(song))
                            Songs.SelectedItems.Add(song);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (SongItem song in e.OldItems)
                        if (Songs.SelectedItems.Contains(song))
                            Songs.SelectedItems.Remove(song);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Songs.SelectedItems.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    foreach (SongItem song in e.OldItems)
                        if (Songs.SelectedItems.Contains(song))
                            Songs.SelectedItems.Remove(song);
                    foreach (SongItem song in e.NewItems)
                        if (!Songs.SelectedItems.Contains(song))
                            Songs.SelectedItems.Add(song);
                    break;
                default: break;
            }
        }

        //TODO: Accept drag-and-drop
        //TODO: Hide button after a song is added
        //TODO: Add this option in File->Add Song
        //TODO: Directory import (batch import, see performance with taglib)
        //TODO: Test more formats
        private void AddNewSongButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow();
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
            foreach (SongItem addedSong in e.AddedItems)
                ((LibraryVM)DataContext).SelectedSongs.Add(addedSong);
            foreach (SongItem song in e.RemovedItems)
                ((LibraryVM)DataContext).SelectedSongs.Remove(song);
        }
    }
}
