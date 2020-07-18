﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;

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
            if(e.Key == Key.Delete)
            {
                ((LibraryVM)this.DataContext).RemoveSelectedSongsCommand.Execute(null);
            }
        }

        private void SongsGridScroll_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = e.Source;

            ScrollViewer scv = (ScrollViewer) sender;
            scv.RaiseEvent(eventArg);
            e.Handled = true;
        }
    }
}
