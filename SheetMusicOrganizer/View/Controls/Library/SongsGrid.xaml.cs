using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for SongsGrid.xaml
    /// </summary>
    public partial class SongsGrid : UserControl
    {
        public SongsGrid()
        {
            InitializeComponent();
            DataContextChanged += SongsGrid_DataContextChanged;
            Songs.Sorting += Songs_Sorting;
        }

        #region Changed Event
        private void Songs_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (!(e.Column is DataGridBoundColumn column))
            {
                Log.Error("Trying to sort on a column type that is not a DataGridBoundColumn");
                return;
            }
            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("Trying to sort by column {column} when the dataContext is not libraryVM but is {dataContext}", column.Header, DataContext?.GetType());
                return;
            }
            string? binding = (column.Binding as Binding)?.Path?.Path;
            if(binding is null)
            {
                Log.Error("Could not find the property to sort on when trying to sort the column: {columnName}", column.Header);
                return;
            }
            if(libraryVM.Playlists[libraryVM.SelectedPlaylistIndex] is PlaylistItem playlist)
            {
                if(playlist.SortCol == binding)
                    playlist.SortAsc = !playlist.SortAsc;
                else
                    playlist.SortCol = binding;
            }
            e.Handled = true;
        }

        private void SongsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LibraryVM oldVM)
            {
                oldVM.Session.MasteryLevels.CollectionChanged -= MasteryLevels_CollectionChanged;
            }

            if (e.NewValue is LibraryVM newVM)
            {
                setSongsSource();
                newVM.PropertyChanged += NewVM_PropertyChanged;
                newVM.Session.MasteryLevels.CollectionChanged += MasteryLevels_CollectionChanged;
                foreach(MasteryItem newItem in newVM.Session.MasteryLevels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
            }
        }

        private void setSongsSource()
        {
            if(DataContext is LibraryVM libraryVM)
            {
                CollectionViewSource itemSourceList = new CollectionViewSource() {Source = libraryVM.ShownSongs
                    , IsLiveFilteringRequested = true, LiveFilteringProperties = { "MasteryId" }};
                ICollectionView itemList = itemSourceList.View;

                itemList.Filter = (Predicate<object>?)new Predicate<object>(item => !libraryVM.Session.MasteryLevels.Any(x => x.IsSelected)
                                                                  || ((SongItem)item).Mastery.IsSelected);
                Songs.ItemsSource = itemList;
            }
        }

        private void NewVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if(e.PropertyName == nameof(LibraryVM.SelectedPlaylistIndex) && DataContext is LibraryVM libraryVM)
            //{
            //    setSongsSource();
            //}
        }

        private void MasteryLevels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(DataContext is LibraryVM libraryVM)
                foreach(MasteryItem newItem in libraryVM.Session.MasteryLevels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
        }

        private void MasteryItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MasteryItem.IsSelected))
            {
                if(Songs.ItemsSource is ICollectionView view)
                    view.Refresh();
                else
                    Log.Warning("Event SelectedMasteryLevels_CollectionChanged called when the ItemsSource of the songs grid is not an ICollectionView.");
            }
        }

        #endregion

        #region Action Event

        private void AddNewSongButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow();
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenFolderWindow();
        }

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

        #endregion
    }
}
