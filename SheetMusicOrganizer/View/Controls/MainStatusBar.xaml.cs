using SheetMusicOrganizer.ViewModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SheetMusicOrganizer.View.Controls
{
    /// <summary>
    /// Interaction logic for MainStatusBar.xaml
    /// </summary>
    public partial class MainStatusBar : UserControl
    {
        public MainStatusBar()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                StatusContext.LoadingMessage += StatusContext_LoadingMessage;
                StatusContext.SavingMessage += StatusContext_SavingMessage;
            };
            Unloaded += (s, e) =>
            {
                StatusContext.LoadingMessage -= StatusContext_LoadingMessage;
                StatusContext.SavingMessage -= StatusContext_SavingMessage;
            };
        }

        private void StatusContext_LoadingMessage(object? sender, string e)
        {
            LoadingMessage.Content = e;
            forceUpdateUI();
        }

        private void StatusContext_SavingMessage(object? sender, string e)
        {
            SavingMessage.Content = e;
            forceUpdateUI();
        }

        private void forceUpdateUI()
        {
            // todo: not clean. eventually find a better way. check 
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate (object parameter) {
                frame.Continue = false;
                return null;
            }), null);
        }

        private void ContentControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(DataContext is MainVM mainVM && mainVM.Session.PlayingSong != null)
            {
                mainVM.GoToSong(mainVM.Session.PlayingSong, true);
            }
        }
    }
}
