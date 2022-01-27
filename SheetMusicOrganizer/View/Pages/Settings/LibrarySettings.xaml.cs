using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SheetMusicOrganizer.View.Controls.MultiSelector;

namespace SheetMusicOrganizer.View.Pages.Settings
{
    /// <summary>
    /// Interaction logic for LibrarySettings.xaml
    /// </summary>
    public partial class LibrarySettings : UserControl
    {
        public LibrarySettings()
        {
            DataContext = this;
            VisibleColumns = new ObservableCollection<SelectorItem>();
            VisibleColumns.Add(new SelectorItem { Name = "Title", Selectable = false, PosIndex = -3 });
            VisibleColumns.Add(new SelectorItem { Name = "Artist", Selectable = false, PosIndex = -2 });
            VisibleColumns.Add(new SelectorItem { Name = "Mastery", Selectable = false, PosIndex = -1 });

            HiddenColumns = new ObservableCollection<SelectorItem>();
            var cols = new string[] { "#", "Album", "Genre", "Year", "Length", "Codec", "Bitrate", "Rating", "Date Added", "Music Sheet Filename", "Audio Filename", "2nd Audio Filename", "Notes" };
            for(int i=0; i<cols.Length; i++)
            {
                if (SheetMusicOrganizer.Settings.Default.HiddenColumns.Contains(cols[i]))
                    HiddenColumns.Add(new SelectorItem { Name = cols[i], PosIndex = i });
                else
                    VisibleColumns.Add(new SelectorItem { Name = cols[i], PosIndex = i });
            }
            HiddenColumns.CollectionChanged += HiddenColumns_CollectionChanged;
            var partitionMode = SheetMusicOrganizer.Settings.Default.PartitionSelectionMode;
            if (partitionMode == 0)
                PartitionSelectionMode = 0;
            else
                PartitionSelectionMode = 1;
            Loaded += (s, e) =>
            {
                SheetMusicOrganizer.Settings.Default.PropertyChanged += Settings_PropertyChanged;
            };
            Unloaded += (s, e) =>
            {
                SheetMusicOrganizer.Settings.Default.PropertyChanged -= Settings_PropertyChanged;
            };
            InitializeComponent();
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetMusicOrganizer.Settings.Default.PartitionSelectionMode))
            {
                var partitionMode = SheetMusicOrganizer.Settings.Default.PartitionSelectionMode;
                if (partitionMode == 0)
                    PartitionSelectionMode = 0;
                else
                    PartitionSelectionMode = 1;
            }
        }

        #region Columns
        public ObservableCollection<SelectorItem> VisibleColumns { get; }
        public ObservableCollection<SelectorItem> HiddenColumns { get; }

        private void HiddenColumns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SheetMusicOrganizer.Settings.Default.HiddenColumns = new System.Collections.Specialized.StringCollection();
            SheetMusicOrganizer.Settings.Default.HiddenColumns.AddRange(HiddenColumns.Select(x => x.Name).ToArray());
        }
        #endregion

        #region Partition Mode
        public static readonly DependencyProperty PartitionSelectionModeProperty = DependencyProperty.Register("PartitionSelectionMode", typeof(int), typeof(LibrarySettings), new PropertyMetadata(-1, onPartitionModeChanged));
        public int PartitionSelectionMode { get => (int)GetValue(PartitionSelectionModeProperty); set { if (value != PartitionSelectionMode) SetValue(PartitionSelectionModeProperty, value); } }

        private static void onPartitionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int value)
            {
                if (SheetMusicOrganizer.Settings.Default.PartitionSelectionMode != value)
                    SheetMusicOrganizer.Settings.Default.PartitionSelectionMode = value;
            }
        }
        #endregion

    }
}
