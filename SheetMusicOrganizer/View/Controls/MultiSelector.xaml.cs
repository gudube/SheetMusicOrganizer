using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SheetMusicOrganizer.View.Controls
{
    /// <summary>
    /// Interaction logic for MultiSelector.xaml
    /// </summary>
    public partial class MultiSelector : UserControl
    {
        public MultiSelector()
        {
            InitializeComponent();
            MainContent.DataContext = this;
        }

        public class SelectorItem
        {
            public string Name { get; set; } = "";
            public bool Selectable { get; set; } = true;
            public int PosIndex { get; set; } = 0;
        }

        public static readonly DependencyProperty Title1Property = DependencyProperty.Register("Title1", typeof(string), typeof(MultiSelector));
        public string Title1 { get => (string)GetValue(Title1Property); set => SetValue(Title1Property, value); }

        public static readonly DependencyProperty Title2Property = DependencyProperty.Register("Title2", typeof(string), typeof(MultiSelector));
        public string Title2 { get => (string)GetValue(Title2Property); set => SetValue(Title2Property, value); }

        public static readonly DependencyProperty Category1ItemsProperty = DependencyProperty.Register("Category1Items", typeof(ObservableCollection<SelectorItem>), typeof(MultiSelector));
        public ObservableCollection<SelectorItem> Category1Items { get => (ObservableCollection<SelectorItem>)GetValue(Category1ItemsProperty); set => SetValue(Category1ItemsProperty, value); }

        public static readonly DependencyProperty Category2ItemsProperty = DependencyProperty.Register("Category2Items", typeof(ObservableCollection<SelectorItem>), typeof(MultiSelector));
        public ObservableCollection<SelectorItem> Category2Items { get => (ObservableCollection<SelectorItem>)GetValue(Category2ItemsProperty); set => SetValue(Category2ItemsProperty, value); }

        private void Category1List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category1Switch.IsEnabled = Category1List.SelectedItems.Count > 0;
        }

        private void Category2List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category2Switch.IsEnabled = Category2List.SelectedItems.Count > 0;
        }

        private void Category1Switch_Click(object sender, RoutedEventArgs e)
        {
            foreach(SelectorItem item in Category1List.SelectedItems.OfType<SelectorItem>().ToList())
            {
                Category1Items.Remove(item);
                Category2Items.Add(item);
            }
        }

        private void Category2Switch_Click(object sender, RoutedEventArgs e)
        {
            foreach (SelectorItem item in Category2List.SelectedItems.OfType<SelectorItem>().ToList())
            {
                Category2Items.Remove(item);
                Category1Items.Add(item);
            }
        }

        private void CategoryList_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is ListBox list)
                CollectionViewSource.GetDefaultView(list.ItemsSource).SortDescriptions.Add(new SortDescription(nameof(SelectorItem.PosIndex), ListSortDirection.Ascending));
        }
    }
}
