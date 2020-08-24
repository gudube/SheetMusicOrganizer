﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for MasteryListBox.xaml
    /// </summary>
    public partial class MasteryListBox : UserControl, IDropTarget
    {
        public MasteryListBox()
        {
            InitializeComponent();
            DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedMasteryLevels, MainListBox, MainListBox.SelectedItems);
        }


        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is MasteryItem mastery))
                return;

            bool canDrop = false;

            if (dropInfo.Data is SongItem song)
            {
                if (((LibraryVM)DataContext).IsSongInMastery(mastery, song))
                {
                    //AdornerText = "Song already set to this mastery";
                }
                else
                    canDrop = true;
            }
            else if (dropInfo.Data is IEnumerable<object> data)
            {
                if (data.All(x => x is SongItem item && ((LibraryVM)DataContext).IsSongInMastery(mastery, item)))
                {
                    //AdornerText = "Songs already set to this mastery";
                }
                else
                    canDrop = true;
            }

            if (canDrop)
            {
                //AdornerText = "";
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is MasteryItem targetItem))
            {
                Log.Warning("Trying to drop on a mastery item but targetItem is not a MasteryItem but is a {target}", dropInfo.TargetItem.GetType());
                return;
            }

            if (dropInfo.Data is SongItem song)
            {
                ((LibraryVM)DataContext).SetSongMastery(song, targetItem);
            }
            else if (dropInfo.Data is IEnumerable<object> data)
            {
                IEnumerable<object> songs = data as object[] ?? data.ToArray();
                if (songs.All(x => x is SongItem))
                {
                    ((LibraryVM)DataContext).SetSongsMastery(songs.Cast<SongItem>(), targetItem);
                }
            }
        }
    }
}
