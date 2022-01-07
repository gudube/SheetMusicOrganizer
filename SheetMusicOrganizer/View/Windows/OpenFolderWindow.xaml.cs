using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;
using SheetMusicOrganizer.ViewModel.Library;

namespace SheetMusicOrganizer.View.Windows
{
    /// <summary>
    /// Interaction logic for OpenFolderWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class OpenFolderWindow : Window
    {
        public OpenFolderWindow()
        {
            this.Owner = Application.Current.MainWindow;
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();

        }

        public static readonly DependencyProperty FolderProperty = DependencyProperty.Register("Folder", typeof(string), typeof(OpenFolderWindow));
        public string Folder { get => (string)GetValue(FolderProperty); set => SetValue(FolderProperty, value); }
        
        public static readonly DependencyProperty UseMetadataAOProperty = DependencyProperty.Register("UseMetadataAO", typeof(bool), typeof(OpenFolderWindow), new PropertyMetadata(true));
        public bool UseMetadataAO { get => (bool)GetValue(UseMetadataAOProperty); set => SetValue(UseMetadataAOProperty, value); }

        public static readonly DependencyProperty RecursiveAOProperty = DependencyProperty.Register("RecursiveAO", typeof(bool), typeof(OpenFolderWindow), new PropertyMetadata(true));
        public bool RecursiveAO { get => (bool)GetValue(RecursiveAOProperty); set => SetValue(RecursiveAOProperty, value); }

        public static readonly DependencyProperty OverwriteAOProperty = DependencyProperty.Register("OverwriteAO", typeof(bool), typeof(OpenFolderWindow), new PropertyMetadata(false));
        public bool OverwriteAO { get => (bool)GetValue(OverwriteAOProperty); set => SetValue(OverwriteAOProperty, value); }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Folder = dialog.FileName;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(Owner.DataContext is MainVM mainVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to confirm open folder import when DataContext is not a MainVM, but is {DataContext?.GetType()}"));
                return;
            }

            var allPlaylist = mainVM.LibraryVM.Playlists[0];
            var selectedPlaylist = mainVM.LibraryVM.Playlists.ElementAtOrDefault(mainVM.LibraryVM.SelectedPlaylistIndex);
            var importLibraryVM = new ImportLibraryVM(mainVM.Session, allPlaylist, selectedPlaylist);
            Task importAction = importLibraryVM.AddDir(
                    ImportByFolder.IsChecked.HasValue && ImportByFolder.IsChecked.Value,
                    ImportByFilename.IsChecked.HasValue && ImportByFilename.IsChecked.Value,
                    Folder, RecursiveAO, UseMetadataAO, OverwriteAO);
            WindowManager.OpenOptionWindow(new ImportResultWindow(importLibraryVM, importAction));
        }
    }
}
