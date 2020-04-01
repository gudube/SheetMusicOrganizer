using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Interaction logic for MasteryListBox.xaml
    /// </summary>
    public partial class MasteryListBox : UserControl
    {
        public MasteryListBox()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<MasteryItem> selectedMasteryLevels = ((ListBox)sender).SelectedItems.Cast<MasteryItem>().ToList();
            if(DataContext is LibraryVM libraryVM)
            {
                libraryVM.SelectedMasteryLevels = new ObservableCollection<MasteryItem>(selectedMasteryLevels);
            }
        }
    }
}
