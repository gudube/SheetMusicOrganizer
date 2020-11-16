using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MusicPlayerForDrummers.View.Controls
{
    /// <summary>
    /// Interaction logic for OptionalExpander.xaml
    /// </summary>
    public partial class OptionalExpander : Expander
    {
        private GridRow[]? _rows;
        private readonly double _defaultOpacity = 0.5;
        private readonly double _changedOpacity = 1;

        private struct GridRow
        {
            public ToggleButton Toggle;
            public bool? DefaultValue;
            public List<UIElement>? Elements;
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
                foreach (UIElement? elem in grid.Children)
                {
                    if (elem == null)
                        continue;
                    elem.Opacity = _defaultOpacity;
                    int rowIndex = Grid.GetRow(elem);
                    if (elem is ToggleButton toggle)
                    {
                        _rows[rowIndex].Toggle = toggle;
                        _rows[rowIndex].DefaultValue = toggle.IsChecked;
                        toggle.Checked += Toggle_Changed; //TODO: need to unsubscribe?
                        toggle.Unchecked += Toggle_Changed; //TODO: need to unsubscribe?
                    }
                    else
                    {
                        _rows[rowIndex].Elements = new List<UIElement> { elem };
                    }
                }
            }
        }

        //TODO: Add (default) or (modified) to header to make it more clear
        private void Toggle_Changed(object sender, RoutedEventArgs e)
        {
            if(_rows == null)
                return;

            GridRow changed = _rows.First(x => x.Toggle == sender);
            if(changed.Toggle.IsChecked != changed.DefaultValue) //togle became nondefault
            {
                changed.Toggle.Opacity = _changedOpacity;
                if(changed.Elements != null)
                    foreach (UIElement elem in changed.Elements)
                        elem.Opacity = _changedOpacity;
                HeaderOpacity = _changedOpacity;
            }
            else //toggle became default, need to check other toggles
            {
                changed.Toggle.Opacity = _defaultOpacity;
                if(changed.Elements != null)
                    foreach (UIElement elem in changed.Elements)
                        elem.Opacity = _defaultOpacity;

                foreach (GridRow row in _rows)
                {
                    if (row.Toggle != null && row.Toggle.IsChecked != row.DefaultValue)
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
