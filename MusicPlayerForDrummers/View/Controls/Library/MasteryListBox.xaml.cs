using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.View.Tools;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IList = System.Collections.IList;


namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for MasteryListBox.xaml
    /// </summary>
    public partial class MasteryListBox : UserControl, IDropTarget
    {
        public MasteryListBox()
        {
            InitializeComponent();
            //BindingHelper bindingHelper = new BindingHelper();
            DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedMasteryLevels, MainListBox, MainListBox.SelectedItems);
            //DataContextChanged += (sender, args) => ((LibraryVM)DataContext).Session.SelectedMasteryLevels.CollectionChanged +=
            //    (sender, e) => bindingHelper.ObservableCollectionChanged<MasteryItem>(MainListBox.SelectedItems, sender, e);
            
        //    DataContextChanged += (sender, args) => MainListBox.SelectionChanged +=
        //        (sender, e) => bindingHelper.ListChanged(((LibraryVM)DataContext).Session.SelectedMasteryLevels, sender, e);
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
            else if (dropInfo.Data is IEnumerable<object> data && data.All(x => x is SongItem))
            {
                if (data.All(x => ((LibraryVM)DataContext).IsSongInMastery(mastery, (SongItem)x)))
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
            MasteryItem targetItem = dropInfo.TargetItem as MasteryItem;

            if (dropInfo.Data is SongItem song)
            {
                ((LibraryVM)DataContext).SetSongMastery(song, targetItem);
            }
            else
            {
                IEnumerable<object> data = dropInfo.Data as IEnumerable<object>;
                if (data != null && data.All(x => x is SongItem))
                {
                    ((LibraryVM)DataContext).SetSongsMastery(data.Cast<SongItem>(), targetItem);
                }
            }

        }
    }
}
