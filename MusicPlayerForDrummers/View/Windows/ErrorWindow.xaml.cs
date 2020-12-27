using System.Windows;

namespace MusicPlayerForDrummers.View.Windows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(Window owner, string errorMessage)
        {
            this.DataContext = this;
            Owner = owner;
            InitializeComponent();
            ErrorMessage.Text = errorMessage;
            ShowDialog();
        }
    }
}
