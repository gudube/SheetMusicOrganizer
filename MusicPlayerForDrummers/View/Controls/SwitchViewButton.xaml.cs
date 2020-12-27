using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayerForDrummers.View.Controls
{
    /// <summary>
    /// Interaction logic for SwitchPageBar.xaml
    /// </summary>
    public partial class SwitchViewButton : UserControl
    {
        public SwitchViewButton()
        {
            InitializeComponent();
        }

        /*
         * TITLE
         */
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SwitchViewButton));

        /*
         * ANGLE
         */
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); } 
            private set { SetValue(AngleProperty, value); } 
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(SwitchViewButton));

        private EDirection _direction;
        public EDirection Direction
        {
            get { return _direction; }
            set { _direction = value; Angle = (int)value; }
        }

        public enum EDirection
        {
            Left = 90,
            Right = -90
        }

        public ICommand? SwitchViewCommand { get; set; }
        public static readonly DependencyProperty SwitchViewCommandProperty =
            DependencyProperty.Register("SwitchViewCommand", typeof(ICommand), typeof(SwitchViewButton));
    }

    
}
