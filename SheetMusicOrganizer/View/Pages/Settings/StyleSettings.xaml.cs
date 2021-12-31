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
    /// Interaction logic for StyleSettings.xaml
    /// </summary>
    public partial class StyleSettings : UserControl
    {
        public StyleSettings()
        {
            DataContext = this;
            InitializeComponent();
            ThemeIndex = SheetMusicOrganizer.Settings.Default.Theme == "Light" ? 1 : 0;
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
            if(e.PropertyName == nameof(SheetMusicOrganizer.Settings.Default.Theme))
            {
                ThemeIndex = SheetMusicOrganizer.Settings.Default.Theme == "Light" ? 1 : 0;
            }
        }

        public static readonly DependencyProperty ThemeIndexProperty = DependencyProperty.Register("ThemeIndex", typeof(int), typeof(StyleSettings), new PropertyMetadata(-1));
        public int ThemeIndex { get => (int)GetValue(ThemeIndexProperty); set => SetValue(ThemeIndexProperty, value); }

        private void DarkTheme_Selected(object sender, RoutedEventArgs e)
        {
            if(SheetMusicOrganizer.Settings.Default.Theme != "Dark")
                SheetMusicOrganizer.Settings.Default.Theme = "Dark";
        }

        private void LightTheme_Selected(object sender, RoutedEventArgs e)
        {
            if (SheetMusicOrganizer.Settings.Default.Theme != "Light")
                SheetMusicOrganizer.Settings.Default.Theme = "Light";
        }
    }
}
