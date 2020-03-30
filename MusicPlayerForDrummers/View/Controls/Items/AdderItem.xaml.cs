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
    /// Interaction logic for Adder.xaml
    /// </summary>
    public partial class AdderItem : UserControl
    {
        public AdderItem()
        {
            InitializeComponent();
        }

        public ICommand ConfirmCommand
        {
            get => (ICommand)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }
        public static readonly DependencyProperty ConfirmCommandProperty =
            DependencyProperty.Register("ConfirmCommand", typeof(ICommand), typeof(AdderItem));

        private void Edit()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AdderTextBox.Visibility = Visibility.Visible;
                AdderTextBox.Focus();
            }));
        }

        private void Confirm()
        {
            if (AdderTextBox.Visibility != Visibility.Visible)
                return;
            if (string.IsNullOrWhiteSpace(AdderTextBox.Text))
            {
                Cancel();
                return;
            }

            AdderTextBox.Visibility = Visibility.Collapsed;
            ConfirmCommand.Execute(AdderTextBox.Text);
            AdderTextBox.Clear();
        }

        private void Cancel()
        {
            AdderTextBox.Visibility = Visibility.Collapsed;
            AdderTextBox.Clear();
        }

        //TODO: Error:AdderItem stays focused when clicking return, but not when losing focus
        private void AdderTextBox_KeyDown(object sender, KeyEventArgs e)
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

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Edit();
        }

        private void AdderTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Confirm();
        }
    }
}
