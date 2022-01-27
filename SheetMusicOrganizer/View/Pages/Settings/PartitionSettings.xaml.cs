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
    /// Interaction logic for PartitionSettings.xaml
    /// </summary>
    public partial class PartitionSettings : UserControl
    {
        const int highPdfResolution = 2400;
        const int mediumPdfResolution = 1200;
        const int lowPdfResolution = 600;

        public PartitionSettings()
        {
            DataContext = this;
            InitializeComponent();
            var resolution = SheetMusicOrganizer.Settings.Default.PdfResolution;
            if (resolution >= highPdfResolution)
                PdfResolutionIndex = 0;
            else if (resolution <= lowPdfResolution)
                PdfResolutionIndex = 2;
            else
                PdfResolutionIndex = 1;
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
            if (e.PropertyName == nameof(SheetMusicOrganizer.Settings.Default.PdfResolution))
            {
                var resolution = SheetMusicOrganizer.Settings.Default.PdfResolution;
                if (resolution >= highPdfResolution)
                    PdfResolutionIndex = 0;
                else if(resolution <= lowPdfResolution)
                    PdfResolutionIndex = 2;
                else
                    PdfResolutionIndex = 1;
            }
        }

        public static readonly DependencyProperty PdfResolutionIndexProperty = DependencyProperty.Register("PdfResolutionIndex", typeof(int), typeof(PartitionSettings), new PropertyMetadata(-1, onPdfResolutionIndexChanged));
        public int PdfResolutionIndex { get => (int)GetValue(PdfResolutionIndexProperty); set { if (value != PdfResolutionIndex) SetValue(PdfResolutionIndexProperty, value); } }

        private static void onPdfResolutionIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is int value)
            {
                uint newResolution;
                if (value == 0)
                    newResolution = highPdfResolution;
                else if (value == 2)
                    newResolution = lowPdfResolution;
                else
                    newResolution = mediumPdfResolution;
                if(SheetMusicOrganizer.Settings.Default.PdfResolution != newResolution)
                    SheetMusicOrganizer.Settings.Default.PdfResolution = newResolution;
            }
        }
    }
}
