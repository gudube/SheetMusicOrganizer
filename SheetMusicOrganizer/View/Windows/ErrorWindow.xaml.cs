using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Serilog;
using SheetMusicOrganizer.Model;
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
            switch (exception)
            {
                case FileFormatException specific:
                    title = "Error reading file";
                    description = $"There was an error when trying to read the file: '{cleanInput(specific.SourceUri?.LocalPath)}'.\n" +
                        "The file might have the wrong format or be corrupt.";
                    break;
                case FileNotFoundException specific:
                    title = "File not found";
                    description = $"Could not find the file: '{cleanInput(specific.FileName)}'.";
                    //todo: add option to relocate file or erase song from library if that's the case
                    break;
                case InvalidOperationException:
                    title = "An error was encountered during this operation";
                    description = "This operation doesn't seem to work at the moment. The state of the application seems unstable. Restarting the application should fix the error.";
                    break;
                case SqliteException:
                    title = "An error was encountered while accessing the database";
                    ContinueActionButton.Content = "Open another library file";
                    ContinueActionButton.Click += OpenLibrary_Click;
                    ContinueActionButton.Visibility = Visibility.Visible;
                    SecondActionButton.Content = "Create new library";
                    SecondActionButton.Click += CreateLibrary_Click;
                    SecondActionButton.Visibility = Visibility.Visible;
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

        private void OpenLibrary_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                Multiselect = false,
                InitialDirectory = Settings.Default.UserDir,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    DbHandler.OpenDatabase(openDialog.FileName);
                }
                catch (Exception ex)
                {
                    GlobalEvents.raiseErrorEvent(ex);
                }
            }
        }

        private void CreateLibrary_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Library File (*.sqlite)|*.sqlite",
                InitialDirectory = Settings.Default.UserDir
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Create(saveFileDialog.FileName);
                try
                {
                    DbHandler.OpenDatabase(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    GlobalEvents.raiseErrorEvent(ex);
                }
            }
        }
    }
}
