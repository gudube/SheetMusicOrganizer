using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.View.Windows;
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
            Songs.PreviewDragOver += Songs_DragOver;
            Songs.PreviewDragEnter += Songs_DragOver;
            Songs.RowDragDropController.DragStart += RowDragDropController_DragStart;
        }

        #region Changed Event
        private void SongsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LibraryVM oldVM)
            {
                oldVM.Session.MasteryLevels.CollectionChanged -= MasteryLevels_CollectionChanged;
                oldVM.SelectedPlaylist?.PrepareChange();
            }

            if (e.NewValue is LibraryVM newVM)
            {
                Songs.SelectionController = new GridSelectionControllerExt(Songs, newVM);

                updateMasteryFilter();
                updateColSort();
                newVM.Session.MasteryLevels.CollectionChanged += MasteryLevels_CollectionChanged;
                foreach(MasteryItem newItem in newVM.Session.MasteryLevels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
                newVM.SongMasteryChanged += NewVM_SongMasteryChanged;
            }
        }

        //Playlist changed
        private void Songs_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            updateColSort();
        }

        private void NewVM_SongMasteryChanged(object? sender, SongItem[] e)
        {
            Songs.View?.RefreshFilter(true);
        }

        private void MasteryLevels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(sender is IEnumerable<MasteryItem> levels)
                foreach(MasteryItem newItem in levels)
                    newItem.PropertyChanged += MasteryItem_PropertyChanged;
        }

        private void MasteryItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MasteryItem.IsSelected))
                updateMasteryFilter();
        }

        #endregion

        #region Actions on DataGrid

        private void updateMasteryFilter()
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
                            IsCaseSensitive = true,
                        });
                    }
                }
                Songs.View.EndInit();
            }
        }

        private void updateColSort()
        {
            if (DataContext is LibraryVM libraryVM && libraryVM.SelectedPlaylist != null)
            {
                Songs.SortColumnDescriptions.Clear();
                Songs.SortColumnDescriptions.Add(new SortColumnDescription()
                {
                    ColumnName = libraryVM.SelectedPlaylist.SortCol,
                    SortDirection = libraryVM.SelectedPlaylist.SortAsc ? ListSortDirection.Ascending : ListSortDirection.Descending
                });
            }
        }
        #endregion

        #region Events from the DataGrid

        private void AddNewSongButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOptionWindow(new AddNewSongWindow());
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOptionWindow(new OpenFolderWindow());
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
            if (DataContext is LibraryVM libraryVM && libraryVM.SelectedPlaylist != null)
            {
                var sortColumn = Songs.View.SortDescriptions[0];
                libraryVM.SelectedPlaylist.SortAsc = sortColumn.Direction == ListSortDirection.Ascending;
                libraryVM.SelectedPlaylist.SortCol = sortColumn.PropertyName;
                libraryVM.SelectedPlaylist.SortSongs();
            }
        }
        
        private void RowDragDropController_DragStart(object? sender, GridRowDragStartEventArgs e)
        {
            Songs.View.BeginInit();
            Songs.SelectedItems.Clear();
            foreach (object song in e.DraggingRecords)
            {
                Songs.SelectedItems.Add(song);
            }
            Songs.View.EndInit();
        }

        private void Songs_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        #endregion

        #region Selection Controller
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
        #endregion
    }
}
