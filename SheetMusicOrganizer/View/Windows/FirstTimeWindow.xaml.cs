using SheetMusicOrganizer.View.Tools;
using System;
using System.Diagnostics;
using System.Windows;

namespace SheetMusicOrganizer.View.Windows
{
    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class FirstTimeWindow : Window
    {
        public event EventHandler? Completed;

        public FirstTimeWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if(WindowManager.OpenCreateLibraryWindow(false)) 
                Completed?.Invoke(this, EventArgs.Empty);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if(WindowManager.OpenOpenLibraryWindow(false))
                Completed?.Invoke(this, EventArgs.Empty);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
