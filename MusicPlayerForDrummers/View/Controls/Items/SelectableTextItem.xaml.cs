using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayerForDrummers.View.Controls.Items
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

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked", typeof(bool), typeof(SelectableTextItem));
        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }
        
        public static readonly DependencyProperty  IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool),
            typeof(SelectableTextItem), new PropertyMetadata(false));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public static readonly DependencyProperty  TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SelectableTextItem));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsCustomColorProperty = DependencyProperty.Register("IsCustomColor", typeof(bool), typeof(SelectableTextItem));
        public bool IsCustomColor
        {
            get => (bool)GetValue(IsCustomColorProperty);
            set => SetValue(IsCustomColorProperty, value);
        }

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(string), typeof(SelectableTextItem));
        public string TextColor
        {
            get => (string)GetValue(TextColorProperty);
            set {
                IsCustomColor = true;
                SetValue(TextColorProperty, value);
            }
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
