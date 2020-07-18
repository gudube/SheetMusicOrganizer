using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.ViewModel;

namespace MusicPlayerForDrummers.View.Controls.Library
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

        //TODO: Add adorner text/status message to explain why can't drop
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
                if(((LibraryVM)DataContext).IsSongInPlaylist(playlist, song))
                {
                    //AdornerText = "Song already in playlist";
                }
                else
                    canDrop = true;
            }
            else if(dropInfo.Data is IEnumerable<object> data)
            {
                if(data.All(x => x is SongItem item && ((LibraryVM)DataContext).IsSongInPlaylist(playlist, item)))
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
            PlaylistItem targetItem = dropInfo.TargetItem as PlaylistItem;
            
            if(dropInfo.Data is SongItem song)
            {
                ((LibraryVM)DataContext).CopySongToPlaylist(targetItem, song);
            }
            else
            {
                if (dropInfo.Data is IEnumerable<object> data)
                {
                    IEnumerable<object> songs = data as object[] ?? data.ToArray();
                    if(songs.All(x => x is SongItem))
                        ((LibraryVM)DataContext).CopySongsToPlaylist(targetItem, songs.Cast<SongItem>());
                }
            } 
            
        }
    }
}
