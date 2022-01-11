using System.Windows;

namespace SheetMusicOrganizer.View.Windows
{
    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class GenericWindow : Window
    {
        public GenericWindow(Window owner, string mainText, string continueButtonText = "")
        {
            this.DataContext = this;
            Owner = owner;
            Loaded += GenericWindow_Loaded;
            InitializeComponent();
            MainText.Text = mainText;
            if (string.IsNullOrWhiteSpace(continueButtonText))
                ContinueButton.Visibility = Visibility.Hidden;
            else
                ContinueButton.Content = continueButtonText;
            ShowDialog();
        }

        private void GenericWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        //public static readonly DependencyProperty MainTextProperty = DependencyProperty.Register("MainText", typeof(string), typeof(GenericWindow));
        //public string MainText { get => (string)GetValue(MainTextProperty); set => SetValue(MainTextProperty, value); }

        //public static readonly DependencyProperty ContinueButtonTextProperty = DependencyProperty.Register("ContinueButtonText", typeof(string), typeof(GenericWindow));
        //public string ContinueButtonText { get => (string)GetValue(ContinueButtonTextProperty); set => SetValue(ContinueButtonTextProperty, value); }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
