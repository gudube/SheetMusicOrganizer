using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for LibraryListBox.xaml
    /// </summary>
    public partial class PlaylistsListBox : UserControl
    {
        public PlaylistsListBox()
        {
            InitializeComponent();
            this.MainListBox.AllowDrop = true;
            //this.MainListBox.DragEnter += ListView_DragEnter;
            this.MainListBox.Drop += ListView_Drop;
            this.MainListBox.PreviewDragEnter += ListView_DragOver;
            this.MainListBox.PreviewDragOver += ListView_DragOver;
        }

        #region Drag and drop

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (!((e.OriginalSource as FrameworkElement)?.DataContext is PlaylistItem targetItem))
            {
                Log.Warning("Trying to drop on a playlist item but targetItem is not a PlaylistItem but is a {target}", e.OriginalSource.GetType());
                return;
            }

            if (e.Data.GetDataPresent("Records") && e.Data.GetData("Records") is ObservableCollection<object> songs)
            {
                targetItem.AddSongs(songs.Cast<SongItem>());
            }
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            bool canDrop = false;

            if ((e.OriginalSource as FrameworkElement)?.DataContext is PlaylistItem playlist)
            {
                if (playlist.IsLocked)
                {
                    //AdornerText = "Playlist locked";
                }
                else if (e.Data.GetDataPresent("Records") && e.Data.GetData("Records") is ObservableCollection<object> records)
                {
                    if (records.All(x => x is SongItem song && playlist.HasSong(song)))
                    {
                        //AdornerText = "Song(s) already in playlist";
                    }
                    else
                        canDrop = true;
                }
            }
            
            e.Effects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        #endregion

        private void MainListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Warning("DataContext is not LibraryVM when OnKeyDown of PlaylistsListBox");
                return;
            }

            if (e.Key == Key.F2)
            {
                libraryVM.EditSelectedPlaylistCommand?.Execute(null);
                e.Handled = true;
            }else if(e.Key == Key.Delete)
            {
                libraryVM.DeleteSelectedPlaylistCommand?.Execute(null);
                e.Handled = true;
            }
        }
    }
}
