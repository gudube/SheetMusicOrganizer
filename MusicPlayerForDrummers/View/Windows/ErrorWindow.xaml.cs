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
            //todo: Add: you can send the file log.txt to ... to help us resolve this bug in a future version
            //Or even better, make it send automatically when clicking a "report" button
            ErrorMessage.Text = errorMessage;
            ShowDialog();
        }
    }
}
