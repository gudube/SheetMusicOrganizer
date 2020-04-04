using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for SelectableNameItem.xaml
    /// </summary>
    public partial class SelectableTextItem : UserControl
    {
        public SelectableTextItem()
        {
            InitializeComponent();
        }

        public ICommand RenameConfirmCommand
        {
            get => (ICommand)GetValue(RenameConfirmCommandProperty);
            set => SetValue(RenameConfirmCommandProperty, value);
        }
        public static readonly DependencyProperty RenameConfirmCommandProperty =
            DependencyProperty.Register("RenameConfirmCommand", typeof(ICommand), typeof(SelectableTextItem));

        public ICommand DeleteConfirmCommand
        {
            get => (ICommand)GetValue(DeleteConfirmCommandProperty);
            set => SetValue(DeleteConfirmCommandProperty, value);
        }
        public static readonly DependencyProperty DeleteConfirmCommandProperty =
            DependencyProperty.Register("DeleteConfirmCommand", typeof(ICommand), typeof(SelectableTextItem));

        public static DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked", typeof(bool), typeof(SelectableTextItem));

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }
        
        public static DependencyProperty  TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SelectableTextItem));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void Edit()
        {
            MainTextBox.Visibility = Visibility.Visible;
            MainTextBox.Text = Text;
            MainTextBox.Focus();
        }

        private void Confirm()
        {
            if (MainTextBox.Visibility != Visibility.Visible)
                return;
            if (string.IsNullOrWhiteSpace(MainTextBox.Text))
            {
                Cancel();
                return;
            }

            MainTextBox.Visibility = Visibility.Collapsed;
            RenameConfirmCommand.Execute(MainTextBox.Text);
            MainTextBox.Clear();
        }

        private void Cancel()
        {
            MainTextBox.Visibility = Visibility.Collapsed;
            MainTextBox.Clear();
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Confirm();
            }
            else if (e.Key == Key.Escape)
            {
                Cancel();
            }
        }
    }
}
