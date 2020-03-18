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
using System.ComponentModel;

namespace MusicPlayerForDrummers.Controls
{
    /// <summary>
    /// Interaction logic for SwitchPageBar.xaml
    /// </summary>
    public partial class SwitchPageBar : UserControl
    {
        public SwitchPageBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private Direction _labelDirection = Direction.Right;
        public Direction LabelDirection
        {
            get { return _labelDirection; }
            set
            {
                _labelDirection = value;
                Angle = (int) _labelDirection;
            }
        }

        public double Angle { get; set; }

        private string _title = "";
        public string Title
        {
            get { return "\u25BD   " + _title + "   \u25BD"; }
            set { _title = value; }
        }
    }

    public enum Direction
    {
        Left = 90,
        Right = -90
    }
}
