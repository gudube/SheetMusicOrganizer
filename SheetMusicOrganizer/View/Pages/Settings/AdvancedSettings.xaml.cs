using System;
using System.Collections.Generic;
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

namespace SheetMusicOrganizer.View.Pages.Settings
{
    /// <summary>
    /// Interaction logic for Advanced_Settings.xaml
    /// </summary>
    public partial class AdvancedSettings : UserControl
    {
        public AdvancedSettings()
        {
            InitializeComponent();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (currentExecutablePath == null || !currentExecutablePath.EndsWith(".exe"))
            {
                throw new InvalidOperationException("There was an error when trying to automatically reopen the application with the new library. Please reopen it manually.");
            }
            else
            {
                SheetMusicOrganizer.Settings.Default.Reset();
                Process.Start(currentExecutablePath);
                Application.Current.Shutdown();
            }
        }
    }
}
