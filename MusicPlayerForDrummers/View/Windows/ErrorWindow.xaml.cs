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
        private string title = "Oops, you found a bug!";
        private string description = "";

        public ErrorWindow(Window owner, Exception exception, string? customMessage)
        {
            this.DataContext = this;
            Owner = owner;
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            createMessageFromException(exception);
            Title.Text = title;
            CustomMessage.Text = customMessage ?? description;
            ErrorMessage.Text = exception.Message;
            ErrorContainer.Visibility = string.IsNullOrWhiteSpace(ErrorMessage.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void createMessageFromException(Exception exception)
        {
            // add other exception types here
            switch (exception)
            {
                case FileFormatException specific:
                    title = "Error reading file";
                    description = $"There was an error when trying to read the file: {specific.SourceUri.AbsoluteUri}.\n" +
                        $"The file might have the wrong format or be corrupt.";
                    break;
                case FileNotFoundException specific:
                    title = "File not found";
                    description = $"Could not find the file: '{specific.FileName}'.";
                    //todo: add option to relocate file or erase song from library if that's the case
                    break;
                default:
                    break;
            }

        }
    }
}
