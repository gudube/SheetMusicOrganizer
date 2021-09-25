using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;

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
        }

        #region Changed Event
        private void SongsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LibraryVM oldVM)
            {
                oldVM.Session.MasteryLevels.CollectionChanged -= MasteryLevels_CollectionChanged;
            }

            if (e.NewValue is LibraryVM newVM)
            {
                Songs.SelectionController = new GridSelectionControllerExt(Songs, newVM);

                setNewFilter();
                newVM.PropertyChanged += NewVM_PropertyChanged;
                newVM.Session.MasteryLevels.CollectionChanged += MasteryLevels_CollectionChanged;
                foreach(MasteryItem newItem in newVM.Session.MasteryLevels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
            }
        }

        private void NewVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(LibraryVM.SelectedPlaylistIndex) && DataContext is LibraryVM libraryVM
                && libraryVM.Playlists[libraryVM.SelectedPlaylistIndex] is PlaylistItem selectedPlaylist)
            {
                Songs.SortColumnDescriptions.Clear();
                Songs.SortColumnDescriptions.Add(new SortColumnDescription()
                {
                    ColumnName = selectedPlaylist.SortCol,
                    SortDirection = selectedPlaylist.SortAsc ? ListSortDirection.Ascending : ListSortDirection.Descending
                });
            }
        }

        private void setNewFilter()
        {
            if (DataContext is LibraryVM libraryVM && Songs.View != null)
            {
                var gridCol = Songs.Columns["Mastery.Name"];
                var selectedMasteryLevels = libraryVM.Session.MasteryLevels.Where(x => x.IsSelected);
               
                Songs.View.BeginInit();
                gridCol.FilterPredicates.Clear();

                if (selectedMasteryLevels.Count() > 0 && selectedMasteryLevels.Count() < libraryVM.Session.MasteryLevels.Count)
                {
                    foreach (var selectedMasteryLevel in selectedMasteryLevels)
                    {
                        gridCol.FilterPredicates.Add(new FilterPredicate()
                        {
                            FilterType = FilterType.Equals,
                            FilterValue = selectedMasteryLevel.Name,
                            FilterBehavior = FilterBehavior.StronglyTyped,
                            FilterMode = ColumnFilter.Value,
                            PredicateType = PredicateType.Or,
                            IsCaseSensitive = true
                        });
                    }
                }
                Songs.View.EndInit();
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
                setNewFilter();
            }
        }

        private void SfDataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            foreach (GridRowInfo item in e.AddedItems)
                if (item.RowData is SongItem song)
                    song.IsSelected = true;
            foreach (GridRowInfo item in e.RemovedItems)
                if (item.RowData is SongItem song)
                    song.IsSelected = false;
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

        private void Songs_SortColumnsChanging(object sender, GridSortColumnsChangingEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                Songs.SortColumnDescriptions.Clear();

                if(e.AddedItems.Count > 1)
                {
                    var firstItem = e.AddedItems[0];
                    e.AddedItems.Clear();
                    e.AddedItems.Add(firstItem);
                }
            }
        }

        private void Songs_SortColumnsChanged(object sender, GridSortColumnsChangedEventArgs e)
        {
            if (DataContext is LibraryVM libraryVM && libraryVM.Playlists[libraryVM.SelectedPlaylistIndex] is PlaylistItem playlist)
            {
                var sortColumn = Songs.View.SortDescriptions[0];
                playlist.SortAsc = sortColumn.Direction == ListSortDirection.Ascending;
                playlist.SortCol = sortColumn.PropertyName;
                playlist.SortSongs();
            }
        }

        #endregion

        internal class GridSelectionControllerExt : GridSelectionController
        {
            private LibraryVM _libraryVM;
            public GridSelectionControllerExt(SfDataGrid dataGrid, LibraryVM dataContext) : base(dataGrid)
            {
                _libraryVM = dataContext;
            }

            protected override void ProcessKeyDown(KeyEventArgs args)
            {
                if (args.Key == Key.Enter)
                {
                    if(SelectedRows.Count > 0)
                    {
                        _libraryVM.PlaySelectedSongCommand?.Execute(SelectedRows[0].RowData as SongItem);
                    }
                    args.Handled = true;
                } else if (args.Key == Key.Delete) {
                    if (SelectedRows.Count > 0)
                    {
                        _libraryVM.RemoveSelectedSongsCommand?.Execute(null);
                    }
                    args.Handled = true;
                }
                else {
                    base.ProcessKeyDown(args);
                }
            }
        }


    }
}
