using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for WaveformSeekBar.xaml
    /// </summary>
    public partial class WaveformSeekBar : Slider
    {
        public WaveformSeekBar()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += WaveformSeekbar_Loaded;
        }
        
        private Track? _mainTrack;
        private Track? _previewTrack;
        private Popup? _previewPopup;

        private void WaveformSeekbar_Loaded(object sender, RoutedEventArgs e)
        {
            _mainTrack = (Track)this.Template.FindName("PART_Track", this);
            _previewTrack = (Track)this.Template.FindName("PreviewTrack", this);
            _previewPopup = (Popup)this.Template.FindName("PreviewPopup", this);
        }

        public double PreviewValue { get => (double) GetValue(PreviewValueProperty); set => SetValue(PreviewValueProperty, value); }
        DependencyProperty PreviewValueProperty = DependencyProperty.Register("PreviewValue", typeof(double), typeof(WaveformSeekBar));

        public string PreviewTime { get => (string)GetValue(PreviewTimeProperty); set => SetValue(PreviewTimeProperty, value); }
        DependencyProperty PreviewTimeProperty = DependencyProperty.Register("PreviewTime", typeof(string), typeof(WaveformSeekBar));

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
                //OnPreviewMouseLeftButtonDown(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent});

            if (_previewTrack == null || _previewPopup == null)
            {
                Log.Warning("OnPreviewMouseMove on waveform seekbar when the track wasn't loaded");
                return;
            }
            Point currentPos = e.GetPosition(_previewTrack);
            PreviewValue = _previewTrack.ValueFromPoint(currentPos);
            PreviewTime = TimeSpan.FromSeconds(_previewTrack.Value).ToString(@"mm\:ss"); //TODO: Add hour if possible and there is?
            _previewPopup.HorizontalOffset = currentPos.X;
            _previewPopup.VerticalOffset = currentPos.Y;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            //https://stackoverflow.com/questions/2909862/slider-does-not-drag-in-combination-with-ismovetopointenabled-behaviour
            // It's important to check `track.Thumb.IsMouseOver`, because if it's `true` then
            // the Thumb will already have its `OnMouseLeftButtonDown` method called - there's
            // no need for us to manually trigger it (and doing so would result in firing the
            // event twice, which is bad).
            if (!IsMoveToPointEnabled || _mainTrack == null || _mainTrack.Thumb.IsMouseOver)
                return;

            // When `IsMoveToPointEnabled` is true, the Slider's `OnPreviewMouseLeftButtonDown`
            // method updates the slider's value to where the user clicked. However, the Thumb
            // hasn't had its position updated yet to reflect the change. As a result, we must
            // call `UpdateLayout` on the Thumb to make sure its position is correct before we
            // trigger a `MouseLeftButtonDownEvent` on it.
            _mainTrack.Thumb.UpdateLayout();


            _mainTrack.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = MouseLeftButtonDownEvent,
                Source = _mainTrack.Thumb
            });
        }
    }
}
