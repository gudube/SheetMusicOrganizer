using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            DataContextChanged += SongsGrid_DataContextChanged;
            UserSettings.Default.SortDescriptions = new List<(string, int)>();
            foreach(string colName in UserSettings.Default.ColumnSorts)
            {
                DataGridColumn column = SongsDG.Columns.FirstOrDefault(x => String.Equals(x.Header, colName))
                if (column != null)
                {
                    (SongsDG.ItemsSource as ListCollectionView).SortDescriptions.Add(new SortDescription(column.Header.ToString(), direction))
                }

            }

        }

        private void SongsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedSongs, SongsDG, SongsDG.SelectedItems);
            if(DataContext is LibraryVM libraryVM)
                libraryVM.Session.SelectedMasteryLevels.CollectionChanged += SelectedMasteryLevels_CollectionChanged;
        }

        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(DataContext is LibraryVM libraryVM && SongsDG != null)
            (SongsDG.ItemsSource as ListCollectionView).Filter = new Predicate<object>(x =>
                                                  libraryVM.Session.SelectedMasteryLevels.Any(mastery => mastery.ID == ((SongItem)x).MasteryID));
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
            if(e.Key == Key.Delete)
            {
                ((LibraryVM)this.DataContext).RemoveSelectedSongsCommand.Execute(null);
            }
        }

        //TODO: Add drag and drop song to reorder it

        //TODO: Save new order to database
        //TODO: Add SortDescriptions in First place instead of clearing everything
        private void Songs_Sorting(object sender, DataGridSortingEventArgs e)
        {
            ListCollectionView source = (sender as DataGrid).ItemsSource as ListCollectionView;
            if (source == null)
            {
                return;
            }

            DataGridColumn column = e.Column;
            ListSortDirection direction = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            //ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(Songs);


            using (source.DeferRefresh())
            {
                if(source.SortDescriptions.Contains)
                source.SortDescriptions.Clear();
                source.SortDescriptions.Add(new SortDescription(column.Header.ToString(), direction));
            }
            source.Refresh();
            column.SortDirection = direction;
            e.Handled = true;
        }
    }
}
