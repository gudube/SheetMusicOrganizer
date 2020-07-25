using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
