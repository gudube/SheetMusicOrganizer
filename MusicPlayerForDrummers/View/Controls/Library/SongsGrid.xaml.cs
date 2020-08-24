using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for SongsGrid.xaml
    /// </summary>
    public partial class SongsGrid : UserControl, IDropTarget
    {
        public SongsGrid()
        {
            InitializeComponent();
            //DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedSongs, Songs, Songs.SelectedItems);
            DataContextChanged += SongsGrid_DataContextChanged;
            Songs.Sorting += Songs_Sorting;
            Songs.SelectionChanged += Songs_SelectionChanged;
        }

        #region Changed Event
        private void Songs_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("Trying to sort by column {column} when the dataContext is not libraryVM but is {dataContext}", column, DataContext?.GetType());
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
            if (e.OldValue is LibraryVM oldVM)
            {
                oldVM.Session.SelectedSongs.CollectionChanged -= SelectedSongs_CollectionChanged;
                oldVM.Session.Songs.CollectionChanged -= Songs_CollectionChanged;
                oldVM.Session.SelectedMasteryLevels.CollectionChanged -= SelectedMasteryLevels_CollectionChanged;
                foreach (SongItem? song in oldVM.Session.Songs)
                {
                    if (song != null)
                        song.PropertyChanged -= Song_PropertyChanged;
                }
            }

            if (e.NewValue is LibraryVM newVM)
            {
                CollectionViewSource itemSourceList = new CollectionViewSource() { Source = newVM.Session.Songs };
                ICollectionView itemList = itemSourceList.View;

                var masteryFilter = new Predicate<object>(item => newVM.Session.SelectedMasteryLevels.Count == 0
                                                                  || newVM.Session.SelectedMasteryLevels.Any(x => x.Id == ((SongItem)item).MasteryId));
                itemSourceList.View.Filter = masteryFilter;

                Songs.ItemsSource = itemList;

                foreach (SongItem? song in newVM.Session.Songs)
                {
                    if (song != null)
                        song.PropertyChanged += Song_PropertyChanged;
                }
                
                newVM.Session.SelectedMasteryLevels.CollectionChanged += SelectedMasteryLevels_CollectionChanged;
                newVM.Session.Songs.CollectionChanged += Songs_CollectionChanged;
                newVM.Session.SelectedSongs.CollectionChanged += SelectedSongs_CollectionChanged;
            }
        }

        private bool _changingCollection = false;

        private void SelectedSongs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_changingCollection)
            {
                _changingCollection = false;
                return;
            }

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("VM SelectedSongs changed when DataContext of MasteryListBox is not LibraryVM but is: {dataContext}", DataContext?.GetType());
                return;
            }

            _changingCollection = true;
            Songs.SelectedItems.Clear();

            foreach (SongItem song in libraryVM.Session.SelectedSongs)
            {
                _changingCollection = true;
                Songs.SelectedItems.Add(song);
            }
        }

        private void Songs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingCollection)
            {
                _changingCollection = false;
                return;
            }

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("SongsGrid selected songs changed when DataContext of MasteryListBox is not LibraryVM but is: {dataContext}", DataContext?.GetType());
                return;
            }

            foreach (SongItem? item in e.AddedItems)
                if (item != null && !libraryVM.Session.SelectedSongs.Contains(item))
                {
                    _changingCollection = true;
                    libraryVM.Session.SelectedSongs.Add(item);
                }
            foreach (SongItem? item in e.RemovedItems)
                if (item != null && libraryVM.Session.SelectedSongs.Contains(item))
                {
                    _changingCollection = true;
                    libraryVM.Session.SelectedSongs.Remove(item);
                }
        }

        private void Songs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null)
                foreach (SongItem? song in e.OldItems)
                    if (song != null)
                        song.PropertyChanged -= Song_PropertyChanged;

            if(e.NewItems != null)
                foreach (SongItem? song in e.NewItems)
                   if(song != null)
                        song.PropertyChanged += Song_PropertyChanged;
        }

        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(Songs.ItemsSource is ICollectionView view)
                view.Refresh();
            else
                Log.Warning("Event SelectedMasteryLevels_CollectionChanged called when the ItemsSource of the songs grid is not an ICollectionView.");
        }

        private void Song_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(Songs.ItemsSource is ICollectionView view))
            {
                Log.Warning("Song_PropertyChanged called when the ItemsSource of the songs grid is not an ICollectionView.");
                return;
            }

            if (e.PropertyName == nameof(SongItem.MasteryId))
                view.Refresh();
        }
        #endregion

        #region Action Event

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
                Log.Warning("DataContext of SongsGrid is not LibraryVM in DataGrid_PreviewKeyDown, but is {type}", DataContext?.GetType());
                return;
            }

            if (e.Key == Key.Delete)
            {
                libraryVM.RemoveSelectedSongsCommand?.Execute(null);
                e.Handled = true;
            } 
            else if (e.Key == Key.Enter)
            {
                libraryVM.PlaySelectedSongCommand?.Execute(null);
                e.Handled = true;
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            DefaultDropHandler handler = new DefaultDropHandler();
            handler.DragOver(dropInfo);
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(DataContext is LibraryVM libraryVM))
            {
                //TODO: Popup message in small at the bottom when there's a Log.Error, followed by 'Open Console' link to see all info
                Log.Error("DataContext of SongsGrid is not LibraryVM in Songs_OnDrop, but is {type}", DataContext?.GetType());
                return;
            }

            if (!(Songs.ItemsSource is ListCollectionView view))
            {
                Log.Error("ItemSource of the SongsGrid is not a ListCollectionView, but is a {itemsSource}", Songs.ItemsSource?.GetType());
                return;
            }

            DefaultDropHandler handler = new DefaultDropHandler();
            handler.Drop(dropInfo);
            libraryVM.ResetSongsInCurrentPlaylist(view.SourceCollection.Cast<SongItem>().Select(x => x.Id));
        }
        #endregion
    }
}
