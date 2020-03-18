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
    public partial class SwitchPageBar : Button
    {
        public SwitchPageBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private EDirection _direction = EDirection.Right;
        public EDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                Angle = (int) _direction;
            }
        }
        
        #region Label
        public double Angle { get; set; }

        private string _title = "";
        public string Title
        {
            get { return "\u25BD   " + _title + "   \u25BD"; }
            set { _title = value; }
        }
        #endregion

        #region Events
        public static readonly RoutedEvent SwitchPageEvent = EventManager.RegisterRoutedEvent(
            "SwitchPage", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SwitchPageBar));
        
        public event RoutedEventHandler SwitchPage
        {
            add { AddHandler(SwitchPageEvent, value); }
            remove { RemoveHandler(SwitchPageEvent, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new SwitchPageEventArgs(SwitchPageEvent, _direction));
        }
        #endregion
    }

    public enum EDirection
    {
        Left = 90,
        Right = -90
    }

    public class SwitchPageEventArgs : RoutedEventArgs
    {
        public EDirection Direction
        {
            get; set;
        }

        public SwitchPageEventArgs(RoutedEvent routedEvent, EDirection direction) : base(routedEvent)
        {
            this.Direction = direction;
        }
    }
}
