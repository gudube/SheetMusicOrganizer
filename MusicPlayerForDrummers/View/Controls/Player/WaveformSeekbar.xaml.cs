using System;
using System.Collections.Generic;
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
    /// Interaction logic for WaveformSeekbar.xaml
    /// </summary>
    public partial class WaveformSeekbar : Slider
    {
        public double PreviewPosition { get => (double)GetValue(PreviewPositionProperty); set => SetValue(PreviewPositionProperty, value); }
        public DependencyProperty PreviewPositionProperty = DependencyProperty.Register("PreviewPosition", typeof(double), typeof(WaveformSeekbar));

        public string PreviewTime { get => (string)GetValue(PreviewTimeProperty); set => SetValue(PreviewTimeProperty, value); }
        public DependencyProperty PreviewTimeProperty = DependencyProperty.Register("PreviewTime", typeof(string), typeof(WaveformSeekbar));

        private Track _track;

        public WaveformSeekbar()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += WaveformSeekbar_Loaded;
        }

        private void WaveformSeekbar_Loaded(object sender, RoutedEventArgs e)
        {
            _track = (Track)this.Template.FindName("PART_Track", this);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OnPreviewMouseLeftButtonDown(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent});
            else
            {
                PreviewPosition = e.GetPosition(this).X;
                
                var position = e.GetPosition(_track);
                TimeSpan ts = TimeSpan.FromMilliseconds(_track.ValueFromPoint(position));
                PreviewTime = ts.ToString(@"mm\:ss"); //TODO: Add hour if possible and there is?
            }
        }
    }
}
