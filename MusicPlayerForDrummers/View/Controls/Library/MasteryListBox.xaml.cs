using GongSolutions.Wpf.DragDrop;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MasteryListBox.xaml
    /// </summary>
    public partial class MasteryListBox : UserControl, IDropTarget
    {
        public MasteryListBox()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ((LibraryVM)DataContext).SelectedMasteryLevels.CollectionChanged += SelectedMasteryLevels_CollectionChanged;
        }

        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (MasteryItem mastery in e.NewItems)
                        if (!MainListBox.SelectedItems.Contains(mastery))
                            MainListBox.SelectedItems.Add(mastery);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (MasteryItem mastery in e.OldItems)
                        if (MainListBox.SelectedItems.Contains(mastery))
                            MainListBox.SelectedItems.Remove(mastery);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    MainListBox.SelectedItems.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    foreach (MasteryItem mastery in e.OldItems)
                        if (MainListBox.SelectedItems.Contains(mastery))
                            MainListBox.SelectedItems.Remove(mastery);
                    foreach (MasteryItem mastery in e.NewItems)
                        if (!MainListBox.SelectedItems.Contains(mastery))
                            MainListBox.SelectedItems.Add(mastery);
                    break;
                default: break;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (MasteryItem mastery in e.AddedItems)
                ((LibraryVM)DataContext).SelectedMasteryLevels.Add(mastery);
            foreach (MasteryItem mastery in e.RemovedItems)
                ((LibraryVM)DataContext).SelectedMasteryLevels.Remove(mastery);
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
