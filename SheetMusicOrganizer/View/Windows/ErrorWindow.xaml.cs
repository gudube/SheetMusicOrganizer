using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Serilog;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.View.Tools;
using SheetMusicOrganizer.ViewModel;
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

        public ErrorWindow(Window? owner, Exception exception, string? customMessage)
        {
            this.DataContext = this;
            if(owner != null)
                Owner = owner;
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            createMessageFromException(exception);
            if(owner == null)
            {
                BackButton.Content = "Close";
                BackButton.Click += (_, _) => this.Close();
            }
            ErrorTitle.Text = title;
            CustomMessage.Text = customMessage ?? description;
            ErrorMessage.Text = exception.Message;
            ErrorContainer.Visibility = string.IsNullOrWhiteSpace(ErrorMessage.Text) ? Visibility.Collapsed : Visibility.Visible;
            Log.Error("Error catched and displayed as error window.\n" +
                $"  Type: {exception.GetType()}\n" +
                $"  Title: {ErrorTitle.Text}\n" +
                $"  Custom Message: {CustomMessage.Text}\n" + 
                $"  Exception Message: {ErrorMessage.Text}\n");
        }

        private void createMessageFromException(Exception exception)
        {
            // add other exception types here
            if(exception is FileFormatException formatEx)
            {
                title = "Error reading file";
                description = $"There was an error when trying to read the file: '{cleanInput(formatEx.SourceUri?.LocalPath)}'.\n" +
                    "The file might have the wrong format or be corrupt.";
            }
            if (exception is FileNotFoundException fileEx)
            {
                title = "File not found";
                description = $"Could not find the file: '{cleanInput(fileEx.FileName)}'.";
                //todo: add option to relocate file or erase song from library if that's the case
            }
            if (exception is InvalidOperationException)
            {
                title = "An error was encountered during this operation";
                description = "This operation doesn't seem to work at the moment. The state of the application seems unstable. Restarting the application should fix the error.";
            }
            if (exception is InitLibraryException || exception is LibraryFileNotFoundException)
            {
                if(exception is LibraryFileNotFoundException)
                    description = $"The library file cannot be found : {cleanInput(Settings.Default.RecentDBs[0])}\nMake sure it exists or choose a new directory.";
                else
                    description = $"There was an error when trying to create/read the library file : {cleanInput(Settings.Default.RecentDBs[0])}";
                title = "An error was encountered while accessing the library";
                ContinueActionButton.Content = "Open another library file";
                ContinueActionButton.Click += (_, _) => WindowManager.OpenOpenLibraryWindow(true);
                ContinueActionButton.Visibility = Visibility.Visible;
                SecondActionButton.Content = "Create new library";
                SecondActionButton.Click += (_, _) => WindowManager.OpenCreateLibraryWindow(true);
                SecondActionButton.Visibility = Visibility.Visible;
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
