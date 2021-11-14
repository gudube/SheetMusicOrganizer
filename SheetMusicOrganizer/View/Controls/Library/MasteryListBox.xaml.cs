using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Library
{
    /// <summary>
    /// Interaction logic for MasteryListBox.xaml
    /// </summary>
    public partial class MasteryListBox : UserControl
    {
        public MasteryListBox()
        {
            InitializeComponent();
            this.MainListBox.AllowDrop = true;
            //this.MainListBox.DragEnter += ListView_DragEnter;
            this.MainListBox.Drop += ListView_Drop;
            this.MainListBox.PreviewDragOver += ListView_DragOver;
            this.MainListBox.PreviewDragEnter += ListView_DragOver;
        }

        //private void ListView_DragEnter(object sender, DragEventArgs e)
        //{
        //    e.Effects = DragDropEffects.Move;
        //    e.Handled = true;
        //}

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (!((e.OriginalSource as FrameworkElement)?.DataContext is MasteryItem targetItem))
            {
                Log.Warning("Trying to drop on a mastery item but targetItem is not a MasteryItem but is a {target}", e.OriginalSource.GetType());
                return;
            }

            if (e.Data.GetDataPresent("Records") && e.Data.GetData("Records") is ObservableCollection<object> songs)
            {
                if (!(DataContext is LibraryVM libraryVM))
                {
                    GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to drop on MasteryListBox when DataContext is not LibraryVM, but is {DataContext?.GetType()}"));
                    return;
                }
                libraryVM.SetSongsMastery(songs.Cast<SongItem>(), targetItem);
            }
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            bool canDrop = false;

            if (!(DataContext is LibraryVM libraryVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to drag over MasteryListBox when DataContext is not LibraryVM, but is {DataContext?.GetType()}"));
            }
            else if ((e.OriginalSource as FrameworkElement)?.DataContext is MasteryItem mastery)
            {
                if (e.Data.GetDataPresent("Records") && e.Data.GetData("Records") is ObservableCollection<object> records)
                {
                    if (records.All(x => x is SongItem item && libraryVM.IsSongInMastery(mastery, item)))
                    {
                        //AdornerText = "Song(s) already in playlist";
                    }
                    else
                        canDrop = true;
                }
            }

            e.Effects = canDrop ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }
    }
}
