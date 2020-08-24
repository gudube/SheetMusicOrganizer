using System.Collections.Generic;
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
            MainListBox.SelectionChanged += MainListBox_SelectionChanged;
            //DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedMasteryLevels, MainListBox, MainListBox.SelectedItems);
            DataContextChanged += MasteryListBox_DataContextChanged;
        }

        private void MasteryListBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LibraryVM oldVM)
            {
                oldVM.Session.SelectedMasteryLevels.CollectionChanged -= SelectedMasteryLevels_CollectionChanged;
            }

            if (e.NewValue is LibraryVM newVM)
            {
                newVM.Session.SelectedMasteryLevels.CollectionChanged += SelectedMasteryLevels_CollectionChanged;
            }
        }

        private bool _changingCollection = false;

        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_changingCollection)
            {
                _changingCollection = false;
                return;
            }

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("MainListBox_SelectionChanged when DataContext of MasteryListBox is not LibraryVM but is: {dataContext}", DataContext?.GetType());
                return;
            }

            _changingCollection = true;
            MainListBox.SelectedItems.Clear();

            foreach (MasteryItem mastery in libraryVM.Session.SelectedMasteryLevels)
            {
                _changingCollection = true;
                MainListBox.SelectedItems.Add(mastery);
            }
        }

        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingCollection)
            {
                _changingCollection = false;
                return;
            }

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("MainListBox_SelectionChanged when DataContext of MasteryListBox is not LibraryVM but is: {dataContext}", DataContext?.GetType());
                return;
            }

            foreach (MasteryItem? item in e.AddedItems)
                if (item != null && !libraryVM.Session.SelectedMasteryLevels.Contains(item))
                {
                    _changingCollection = true;
                    libraryVM.Session.SelectedMasteryLevels.Add(item);
                }
            foreach (MasteryItem? item in e.RemovedItems)
                if (item != null && libraryVM.Session.SelectedMasteryLevels.Contains(item))
                {
                    _changingCollection = true;
                    libraryVM.Session.SelectedMasteryLevels.Remove(item);
                }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is MasteryItem mastery))
                return;

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("Trying to drag over MasteryListBox when DataContext is not LibraryVM , but is {type}", DataContext?.GetType());
                return;
            }

            bool canDrop = false;

            if (dropInfo.Data is SongItem song)
            {
                if (libraryVM.IsSongInMastery(mastery, song))
                {
                    //AdornerText = "Song already set to this mastery";
                }
                else
                    canDrop = true;
            }
            else if (dropInfo.Data is IEnumerable<object> data)
            {
                if (data.All(x => x is SongItem item && libraryVM.IsSongInMastery(mastery, item)))
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

            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Error("Trying to drop on MasteryListBox when DataContext is not LibraryVM , but is {type}", DataContext?.GetType());
                return;
            }

            if (dropInfo.Data is SongItem song)
            {
                libraryVM.SetSongMastery(song, targetItem);
            }
            else if (dropInfo.Data is IEnumerable<object> data)
            {
                IEnumerable<object> songs = data as object[] ?? data.ToArray();
                if (songs.All(x => x is SongItem))
                {
                    libraryVM.SetSongsMastery(songs.Cast<SongItem>(), targetItem);
                }
            }
        }
    }
}
