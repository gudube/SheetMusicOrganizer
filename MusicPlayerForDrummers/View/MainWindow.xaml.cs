using MusicPlayerForDrummers.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DBHandler.InitializeDatabase();
            DataContext = this;
        }
        /*
        #region Page

        public MainPage CurrentPage { get; set; }

        
        private void SwitchPage(MainPage page)
        {
            if (!Equals(CurrentPage, page))
            {
                RoutedEventHandler handler = new RoutedEventHandler(SwitchPageEvent);
                CurrentPage.UnsubToSwitchPageEvent(handler);
                CurrentPage = page;
                CurrentPage.SubToSwitchPageEvent(handler);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        private void SwitchPageEvent(object sender, RoutedEventArgs e)
        {
            SwitchPageEventArgs args = (SwitchPageEventArgs)e;
            if(args.Direction == EDirection.Right && CurrentPage == _libraryPage)
            {
                SwitchPage(_partitionPage);
            } else if(CurrentPage == _partitionPage)
            {
                if (args.Direction == EDirection.Left)
                    SwitchPage(_libraryPage);
                else
                    SwitchPage(_syncPage);
            } else if (args.Direction == EDirection.Left && CurrentPage == _syncPage)
            {
                SwitchPage(_partitionPage);
            }
            else
            {
                throw new Exception("Could not resolve the SwitchPageEvent received.");
            }
            e.Handled = true;
        }

        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        */
    }
}
