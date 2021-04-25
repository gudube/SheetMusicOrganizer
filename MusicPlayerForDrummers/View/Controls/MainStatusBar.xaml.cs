using MusicPlayerForDrummers.ViewModel;
using System.Windows.Controls;

namespace MusicPlayerForDrummers.View.Controls
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
        }

        private void StatusContext_SavingMessage(object? sender, string e)
        {
            SavingMessage.Content = e;
        }
    }
}
