using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
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

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            SongItem sourceItem = dropInfo.Data as SongItem;
            PlaylistItem targetItem = dropInfo.TargetItem as PlaylistItem;

            if(sourceItem != null && targetItem != null && !targetItem.IsLocked)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            SongItem sourceItem = dropInfo.Data as SongItem;
            PlaylistItem targetItem = dropInfo.TargetItem as PlaylistItem;
            ((LibraryVM)this.DataContext).AddSongToPlaylist(targetItem, sourceItem);
        }
    }
}
