using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for OptionalExpander.xaml
    /// </summary>
    public partial class OptionalExpander : Expander
    {
        private GridRow[] _rows;
        private readonly double _defaultOpacity = 0.5;
        private readonly double _changedOpacity = 1;

        private struct GridRow
        {
            public ToggleButton toggle;
            public bool? defaultValue;
            public List<UIElement> elements;
        }

        public OptionalExpander()
        {
            this.DataContext = this;
            this.Loaded += OptionalExpander_Loaded;
            InitializeComponent();
            HeaderOpacity = _defaultOpacity;
        }

        public static readonly DependencyProperty HeaderOpacityProperty = DependencyProperty.Register("HeaderOpacity", typeof(double), typeof(OptionalExpander));
        public double HeaderOpacity { get => (double)GetValue(HeaderOpacityProperty); set => SetValue(HeaderOpacityProperty, value); }

        private void OptionalExpander_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Content is Grid grid)
            {
                _rows = new GridRow[grid.RowDefinitions.Count];
                foreach (UIElement elem in grid.Children)
                {
                    elem.Opacity = _defaultOpacity;
                    int rowIndex = Grid.GetRow(elem);
                    if (elem is ToggleButton toggle)
                    {
                        _rows[rowIndex].toggle = toggle;
                        _rows[rowIndex].defaultValue = toggle.IsChecked;
                        toggle.Checked += Toggle_Changed; //TODO: need to unsubscribe?
                        toggle.Unchecked += Toggle_Changed; //TODO: need to unsubscribe?
                    }
                    else
                    {
                        if (_rows[rowIndex].elements == null)
                            _rows[rowIndex].elements = new List<UIElement>();
                        _rows[rowIndex].elements.Add(elem);
                    }
                }
            }
        }

        //TODO: Add (default) or (modified) to header to make it more clear
        private void Toggle_Changed(object sender, RoutedEventArgs e)
        {
            GridRow changed = _rows.First(x => x.toggle == sender);
            if(changed.toggle.IsChecked != changed.defaultValue) //togle became nondefault
            {
                changed.toggle.Opacity = _changedOpacity;
                if(changed.elements != null)
                    foreach (UIElement elem in changed.elements)
                        elem.Opacity = _changedOpacity;
                HeaderOpacity = _changedOpacity;
            }
            else //toggle became default, need to check other toggles
            {
                changed.toggle.Opacity = _defaultOpacity;
                if(changed.elements != null)
                    foreach (UIElement elem in changed.elements)
                        elem.Opacity = _defaultOpacity;

                foreach (GridRow row in _rows)
                {
                    if (row.toggle != null && row.toggle.IsChecked != row.defaultValue)
                    {
                        HeaderOpacity = _changedOpacity;
                        return;
                    }
                }
                
                HeaderOpacity = _defaultOpacity;
            }
        }
    }
}
