using System;
using System.IO;
using System.Windows;

namespace SheetMusicOrganizer.View.Windows
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
            ErrorTitle.Text = title;
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
                    description = $"There was an error when trying to read the file: '{cleanInput(specific.SourceUri?.LocalPath)}'.\n" +
                        $"The file might have the wrong format or be corrupt.";
                    break;
                case FileNotFoundException specific:
                    title = "File not found";
                    description = $"Could not find the file: '{cleanInput(specific.FileName)}'.";
                    //todo: add option to relocate file or erase song from library if that's the case
                    break;
                default:
                    break;
            }
        }

        private string cleanInput(string? text)
        {
            if (text == null)
                return "";
            text = text.Trim();
            if(text.Length > 200)
            {
                return text.Substring(0, 98) + "..." + text.Substring(text.Length - 99, 98);
            }
            return text;
        }
    }
}
