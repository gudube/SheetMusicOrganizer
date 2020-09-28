using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using MusicPlayerForDrummers.ViewModel;

namespace MusicPlayerForDrummers.View.Windows
{
    //TODO: Add ? button with an image that explains the two ways to bind audio file to song
    /// <summary>
    /// Interaction logic for OpenFolderWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class OpenFolderWindow : Window
    {
        public OpenFolderWindow()
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();

        }

        public static readonly DependencyProperty FolderProperty = DependencyProperty.Register("Folder", typeof(string), typeof(OpenFolderWindow));
        public string Folder { get => (string)GetValue(FolderProperty); set => SetValue(FolderProperty, value); }

        public static readonly DependencyProperty UseMetadataAOProperty = DependencyProperty.Register("UseMetadataAO", typeof(bool), typeof(OpenFolderWindow), new PropertyMetadata(true));
        public bool UseMetadataAO { get => (bool)GetValue(UseMetadataAOProperty); set => SetValue(UseMetadataAOProperty, value); }

        public static readonly DependencyProperty RecursiveAOProperty = DependencyProperty.Register("RecursiveAO", typeof(bool), typeof(OpenFolderWindow), new PropertyMetadata(true));
        public bool RecursiveAO { get => (bool)GetValue(RecursiveAOProperty); set => SetValue(RecursiveAOProperty, value); }

        //TODO: Be able to import only audio
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
            ((MainVM)Owner.DataContext).LibraryVM.AddFolder(Folder, RecursiveAO, UseMetadataAO);
            this.Close();
        }
    }
}
