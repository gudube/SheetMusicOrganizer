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

        private bool AutoScroll = true;

        private void Scroller_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (Scroller.VerticalOffset == Scroller.ScrollableHeight)
                {   // Scroll bar is in bottom, Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom, Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight);
            }
        }
    }
}
