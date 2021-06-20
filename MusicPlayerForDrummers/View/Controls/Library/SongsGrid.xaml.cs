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
            string? binding = (column.Binding as Binding)?.Path?.PathParameters.FirstOrDefault()?.ToString();
            if(binding is null)
            {
                Log.Error("Could not find the property to sort on when trying to sort the column: {columnName}", column.Header);
                return;
            }
            column.SortDirection = column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            libraryVM.SortSongs(binding, column.SortDirection == ListSortDirection.Ascending);
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
                CollectionViewSource itemSourceList = new CollectionViewSource() {Source = newVM.ShownSongs
                    , IsLiveFilteringRequested = true, LiveFilteringProperties = { "MasteryId" }};
                ICollectionView itemList = itemSourceList.View;

                var masteryFilter = new Predicate<object>(item => !newVM.Session.MasteryLevels.Any(x => x.IsSelected)
                                                                  || ((SongItem)item).Mastery.IsSelected);
                itemList.Filter = masteryFilter;
                Songs.ItemsSource = itemList;

                newVM.Session.MasteryLevels.CollectionChanged += MasteryLevels_CollectionChanged;
                foreach(MasteryItem newItem in newVM.Session.MasteryLevels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
            }
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

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            DefaultDropHandler handler = new DefaultDropHandler();
            handler.DragOver(dropInfo);
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(DataContext is LibraryVM libraryVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"DataContext of SongsGrid is not LibraryVM in Songs_OnDrop, but is {DataContext?.GetType()}"));
                return;
            }

            if (!(Songs.ItemsSource is ListCollectionView view))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"ItemSource of the SongsGrid is not a ListCollectionView, but is {Songs.ItemsSource?.GetType()}"));
                return;
            }

            DefaultDropHandler handler = new DefaultDropHandler();
            handler.Drop(dropInfo);
            libraryVM.ResetSongsInCurrentPlaylist(view.SourceCollection.Cast<SongItem>().Select(x => x.Id));
        }
        #endregion
    }
}
