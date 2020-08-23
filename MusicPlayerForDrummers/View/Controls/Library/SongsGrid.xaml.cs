using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for SongsGrid.xaml
    /// </summary>
    public partial class SongsGrid : UserControl
    {
        public SongsGrid()
        {
            InitializeComponent();
            DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedSongs, Songs, Songs.SelectedItems);
            DataContextChanged += SongsGrid_DataContextChanged;
            Songs.Sorting += Songs_Sorting;
        }

        private void Songs_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("Trying to sort by column {column} when the dataContext is not libraryVM but is {dataContext}", column, DataContext);
                return;
            }
            column.SortDirection = column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            libraryVM.SortSongs((string)column.Header, column.SortDirection == ListSortDirection.Ascending);
            e.Handled = true;
        }

        private void SongsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!(DataContext is LibraryVM libraryVM))
                return;

            CollectionViewSource itemSourceList = new CollectionViewSource() { Source = libraryVM.Session.Songs };

            // ICollectionView the View/UI part 
            ICollectionView itemList = itemSourceList.View;

            // your Filter
            var masteryFilter = new Predicate<object>(item => libraryVM.Session.SelectedMasteryLevels.Count == 0 
                                                                 || libraryVM.Session.SelectedMasteryLevels.Any(x => x.Id == ((SongItem)item).MasteryId));
            itemSourceList.View.Filter = masteryFilter;
            
            Songs.ItemsSource = itemList;

            SubscribeToSongMasteryId(itemList);
            //todo: any problem with that? since we never unsubscribe but can delete songs
            libraryVM.Session.SelectedMasteryLevels.CollectionChanged += (a, b) => itemList.Refresh();
            libraryVM.Session.Songs.CollectionChanged += (a, b) =>
            {
                SubscribeToSongMasteryId(itemList);
            };
        }

        private void SubscribeToSongMasteryId(ICollectionView itemList)
        {
            if (!(DataContext is LibraryVM libraryVM))
                return;
            foreach (SongItem song in libraryVM.Session.Songs)
            {
                song.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(song.MasteryId))
                        itemList.Refresh();
                };
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

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenFolderWindow();
        }

        //TODO: Be able to copy paste songs between playlists
        //TODO: Be able to drag-and-drop songs to mastery levels
        //TODO: Add warning when deleting song from All Music
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(!(DataContext is LibraryVM libraryVM))
            {
                Log.Warning("DataContext of SongsGrid is not LibraryVM in DataGrid_PreviewKeyDown, but is {type}", DataContext.GetType());
                return;
            }

            if (e.Key == Key.Delete)
            {
                libraryVM.RemoveSelectedSongsCommand.Execute(null);
                e.Handled = true;
            } 
            else if (e.Key == Key.Enter)
            {
                libraryVM.PlaySelectedSongCommand.Execute(null);
                e.Handled = true;
            }

        }
    }
}
