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
        //TODO: Remove lock (at least visual) of masteryitem
        public MasteryListBox()
        {
            InitializeComponent();
            MainListBox.SelectAll();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<MasteryItem> selectedMasteryLevels = ((ListBox)sender).SelectedItems.Cast<MasteryItem>().ToList();
            if(DataContext is LibraryVM libraryVM)
            {
                libraryVM.SelectedMasteryLevels = new ObservableCollection<MasteryItem>(selectedMasteryLevels);
            }
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
