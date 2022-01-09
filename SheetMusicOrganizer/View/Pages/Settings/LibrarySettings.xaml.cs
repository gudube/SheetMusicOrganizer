using System;
using System.Collections.Generic;
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
            InitializeComponent();
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
    }
}
