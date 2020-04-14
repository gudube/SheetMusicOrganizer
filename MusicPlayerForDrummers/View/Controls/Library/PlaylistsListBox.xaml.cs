using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
            else if(dropInfo.Data is IEnumerable<object> data && data.All(x => x is SongItem))
            {
                if(data.All(x => ((LibraryVM)DataContext).IsSongInPlaylist(playlist, (SongItem)x)))
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
                IEnumerable<object> data = dropInfo.Data as IEnumerable<object>;
                if (data != null && data.All(x => x is SongItem))
                {
                    ((LibraryVM)DataContext).CopySongsToPlaylist(targetItem, data.Cast<SongItem>());
                }
            } 
            
        }
    }
}
