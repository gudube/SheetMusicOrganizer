using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Windows
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            this.Owner = Application.Current.MainWindow;
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            this.Closing += SettingsWindow_Closing;
            Loaded += (s, e) =>
            {
                Settings.Default.PropertyChanged += Settings_PropertyChanged;
            };
            Unloaded += (s, e) =>
            {
                Settings.Default.PropertyChanged -= Settings_PropertyChanged;
            };
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UnsavedChange = true;
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Reload();
        }

        public static readonly DependencyProperty UnsavedChangeProperty = DependencyProperty.Register("UnsavedChange", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(false));
        public bool UnsavedChange { get => (bool)GetValue(UnsavedChangeProperty); set => SetValue(UnsavedChangeProperty, value); }
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            this.Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            UnsavedChange = false;
        }
    }
}
