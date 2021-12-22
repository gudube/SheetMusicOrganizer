using Microsoft.Win32;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace SheetMusicOrganizer.View.Controls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
            DataContextChanged += MenuBar_DataContextChanged;
        }

        private void MenuBar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is MainVM mainVM)) return;

            RecentMenuItem.Items.Clear();

            foreach (string? recentDB in Settings.Default.RecentDBs)
            {
                if (RecentMenuItem.Items.Count >= 5) break;
                if (recentDB != null)
                {
                    MenuItem recentDBItem = new MenuItem { Header = recentDB };
                    recentDBItem.Click += (s, e) => mainVM.LoadDatabase(recentDB);
                    RecentMenuItem.Items.Add(recentDBItem);
                }
            }
        }

        private void AddNewSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenAddNewSongWindow();
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenFolderWindow();
        }

        private void LoadDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenOpenLibraryWindow(true);
        }

        private void NewDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenCreateLibraryWindow(true);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (currentExecutablePath == null || !currentExecutablePath.EndsWith(".exe"))
            {
                throw new InvalidOperationException("There was an error when trying to automatically reopen the application with the new library. Please reopen it manually.");
            }
            else
            {
                Settings.Default.Reset();
                Process.Start(currentExecutablePath);
                Application.Current.Shutdown();
            }
        }
    }
}
