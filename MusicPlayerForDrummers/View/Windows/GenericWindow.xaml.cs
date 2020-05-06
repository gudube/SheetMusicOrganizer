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
using System.Windows.Shapes;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class GenericWindow : Window
    {
        public GenericWindow(string mainText, string continueButtonText)
        {
            MainText = mainText;
            ContinueButtonText = continueButtonText;
            this.DataContext = this;
            InitializeComponent();
        }

        public static readonly DependencyProperty MainTextProperty = DependencyProperty.Register("MainText", typeof(string), typeof(GenericWindow));
        public string MainText { get => (string)GetValue(MainTextProperty); set => SetValue(MainTextProperty, value); }

        public static readonly DependencyProperty ContinueButtonTextProperty = DependencyProperty.Register("ContinueButtonText", typeof(string), typeof(GenericWindow));
        public string ContinueButtonText { get => (string)GetValue(ContinueButtonTextProperty); set => SetValue(ContinueButtonTextProperty, value); }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
