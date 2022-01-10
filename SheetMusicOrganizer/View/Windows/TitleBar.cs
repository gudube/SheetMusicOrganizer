using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SheetMusicOrganizer.View.Windows
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public class TitleBar : ContentControl
    {
        public TitleBar()
        {
            Loaded += TitleBar_Loaded;
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject source)
            {
                IsMaximized = Window.GetWindow(source).WindowState == WindowState.Maximized;
                Window.GetWindow(source).StateChanged += MainWindowStateChangeRaised;
            }
        }

        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register("IsMaximized", typeof(bool), typeof(TitleBar));
        public bool IsMaximized { get => (bool)GetValue(IsMaximizedProperty); set => SetValue(IsMaximizedProperty, value); }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(Window.GetWindow(this));
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(Window.GetWindow(this));
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(Window.GetWindow(this));
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(Window.GetWindow(this));
        }

        // State change
        private void MainWindowStateChangeRaised(object? sender, EventArgs e)
        {
            IsMaximized = Window.GetWindow(this).WindowState == WindowState.Maximized;
        }
    }

}
