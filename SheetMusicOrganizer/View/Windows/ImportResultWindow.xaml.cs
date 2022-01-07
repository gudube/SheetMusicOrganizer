using SheetMusicOrganizer.ViewModel.Library;
using System.Windows;

namespace SheetMusicOrganizer.View.Windows
{
    /// <summary>
    /// Interaction logic for ImportResultWindow.xaml
    /// </summary>
    public partial class ImportResultWindow : Window
    {
        public ImportResultWindow(ImportLibraryVM importVM, Task importTask)
        {
            this.DataContext = importVM;
            InitializeComponent();
            Loaded += ImportResultWindow_Loaded;
            this.importTask = importTask;
        }

        private Task importTask;

        private async void ImportResultWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await importTask;
        }
    }
}
