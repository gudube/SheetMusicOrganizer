using System;
using System.IO;
using System.Windows;

namespace MusicPlayerForDrummers.View.Windows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(Window owner, Exception exception, string? customMessage)
        {
            this.DataContext = this;
            Owner = owner;
            InitializeComponent();
            ErrorMessage.Text = customMessage ?? createErrorMessage(exception);
        }

        private string createErrorMessage(Exception exception)
        {
            switch (exception)
            {
                case FileNotFoundException specific:
                    return $"Could not find the file: '{specific.FileName}'";
                default:
                    return exception.Message;
            }

        }
    }
}
