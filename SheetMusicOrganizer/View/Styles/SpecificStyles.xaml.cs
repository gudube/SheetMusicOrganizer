using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SheetMusicOrganizer.View.Styles
{
    public partial class SpecificStyles: ResourceDictionary 
    {
        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            if(sender is DependencyObject source)
            SystemCommands.MinimizeWindow(Window.GetWindow(source));
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DependencyObject source)
                SystemCommands.MaximizeWindow(Window.GetWindow(source));
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DependencyObject source)
                SystemCommands.RestoreWindow(Window.GetWindow(source));
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            if(sender is DependencyObject source)
                SystemCommands.CloseWindow(Window.GetWindow(source));
        }
    }
}
