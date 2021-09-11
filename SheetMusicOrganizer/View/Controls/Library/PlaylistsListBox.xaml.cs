using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for LibraryListBox.xaml
    /// </summary>
    public partial class PlaylistsListBox : UserControl, IDropTarget
    {
        public PlaylistsListBox()
        {
            InitializeComponent();
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is PlaylistItem playlist))
                return;

            if (playlist.IsLocked)
            {
                //AdornerText = "Playlist locked";
                return;
            }

            bool canDrop = false;

            if (dropInfo.Data is SongItem song)
            {
                if(playlist.HasSong(song))
                {
                    //AdornerText = "Song already in playlist";
                }
                else
                    canDrop = true;
            }
            else if(dropInfo.Data is IEnumerable<object> data)
            {
                if(data.All(x => x is SongItem item && playlist.HasSong(item)))
                {
                    //AdornerText = "Song(s) already in playlist";
                }
                else 
                    canDrop = true;
            }
            
            if (canDrop)
            {
                //AdornerText = "";
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is PlaylistItem targetItem))
            {
                Log.Warning("Trying to drop on a playlist item but targetItem is not a PlaylistItem but is a {target}", dropInfo.TargetItem.GetType());
                return;
            }

            if (!(DataContext is LibraryVM libraryVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to drop on PlaylistsListBox when DataContext is not LibraryVM, but is {DataContext?.GetType()}"));
                return;
            }

            if (dropInfo.Data is SongItem song)
            {
                targetItem.AddSongs(song);
            }
            else
            {
                if (dropInfo.Data is IEnumerable<object> data)
                {
                    IEnumerable<object> songs = data as object[] ?? data.ToArray();
                    if (songs.All(x => x is SongItem))
                         targetItem.AddSongs(songs.Cast<SongItem>().ToArray());
                }
            } 
            
        }

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
