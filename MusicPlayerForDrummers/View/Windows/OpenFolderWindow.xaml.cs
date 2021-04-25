using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Windows
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
                Log.Error("Datacontext is not MainVM when trying to confirm open folder import");
                return;
            }

            if(ImportByFolder.IsChecked.HasValue && ImportByFolder.IsChecked.Value)
                mainVM.LibraryVM.AddDirByFolder(Folder, RecursiveAO, UseMetadataAO);
            else if (ImportByFilename.IsChecked.HasValue && ImportByFilename.IsChecked.Value)
                mainVM.LibraryVM.AddDirByFilename(Folder, RecursiveAO, UseMetadataAO);
            else if(ImportPdfOnly.IsChecked.HasValue && ImportPdfOnly.IsChecked.Value)
                mainVM.LibraryVM.AddDirWithoutAudio(Folder, RecursiveAO);
            this.Close();
        }
    }
}
