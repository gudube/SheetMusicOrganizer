using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MusicPlayerForDrummers.ViewModel;
using NAudioWrapper.WaveFormRendererLib;
using Serilog;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace MusicPlayerForDrummers.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for WaveFormSeekBar.xaml
    /// </summary>
    public partial class WaveFormSeekBar : UserControl
    {
        public WaveFormSeekBar()
        {
            InitializeComponent();
            Loaded += WaveFormSeekBar_Loaded;
            DataContextChanged += WaveFormSeekBar_DataContextChanged;
        }
        
        private void WaveFormSeekBar_Loaded(object sender, RoutedEventArgs e)
        {

            //Color color = ((SolidColorBrush) Control.Background).Color;
            _darkRendererSettings = new SoundCloudOriginalSettings
            {
                Width = (int) SeekBar.ActualWidth,
                TopHeight = (int) (SeekBar.ActualHeight*0.6), 
                BottomHeight = (int) (SeekBar.ActualHeight*0.4),
             //   BackgroundColor = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B)
                BackgroundColor = Color.Transparent
            };
        }

        private void WaveFormSeekBar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PlayerVM oldPlayerVM)
                oldPlayerVM.Session.PropertyChanged -= Session_PropertyChanged;

            if(e.NewValue is PlayerVM playerVM)
                playerVM.Session.PropertyChanged += Session_PropertyChanged;
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
            {
                Log.Warning("DataContext not PlayerVM in WaveFormSeekBar when Session property changed");
                return;
            }
            if(e.PropertyName == nameof(playerVM.Session.PlayingSong))
                UpdateWaveForm(playerVM.Session.PlayingSong?.AudioDirectory1 ?? string.Empty);
        }

        private void WaveformSeekBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            playerVM.StartedSeekCommand.Execute(null);
        }

        private void WaveformSeekBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            playerVM.StoppedSeekCommand.Execute(SeekBar.Value);
        }

        private readonly WaveFormRenderer _waveFormRenderer = new WaveFormRenderer();

        private WaveFormRendererSettings? _darkRendererSettings;

        #region Render WaveForm
        private async void UpdateWaveForm(string audioDirectory)
        {
            if (string.IsNullOrEmpty(audioDirectory))
            {
                WaveFormImage.Visibility = Visibility.Hidden;
                return;
            }

            if (_darkRendererSettings == null)
            {
                Log.Warning("WaveFormRendererSettings is null when trying to update the waveform.");
                _darkRendererSettings = new SoundCloudOriginalSettings
                {
                    Width = 1500,
                    TopHeight = 45, 
                    BottomHeight = 30
                };
            }

            LoadingWaveFormText.Visibility = Visibility.Visible;
            WaveFormImage.Visibility = Visibility.Hidden;
            //todo: add cancelation token for when changing songs fast
            //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation
            BitmapImage imageSource = await Task.Run(() =>
            {
                //TODO: do this as often as possible as the performance is a game changer!
                Image image = _waveFormRenderer.Render(audioDirectory, new AveragePeakProvider(3), _darkRendererSettings);
                using (MemoryStream memory = new MemoryStream())
                {
                    image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            });

            WaveFormImage.Source = imageSource;

            WaveFormImage.Visibility = Visibility.Visible;
            LoadingWaveFormText.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Flags
        //private bool _draggingFlag = false;
        private double _minPos = 0;
        private double _maxPos = 1;
        private Grid? _flag;
        private void Flag_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                double percentageGap = 0.02 * Canvas.ActualWidth;
                grid.CaptureMouse();
                if (grid == StartScrollFlag)
                {
                    _minPos = 0;
                    _maxPos = Canvas.ActualWidth - Canvas.GetRight(EndScrollFlag) - percentageGap;
                }
                else if (grid == EndScrollFlag)
                {
                    _minPos = Canvas.GetLeft(StartScrollFlag) + percentageGap;
                    _maxPos = Canvas.ActualWidth;
                }

                _flag = grid;
            }
        }

        private void Flag_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_flag != null)
            {
                double posX = e.GetPosition(Canvas).X;
                if (posX < _minPos)
                    posX = _minPos;
                else if (posX > _maxPos)
                    posX = _maxPos;
                
                if(_flag == EndScrollFlag)
                    Canvas.SetRight(_flag, Canvas.ActualWidth - posX);
                else
                    Canvas.SetLeft(_flag, posX);
            }
        }

        private void Flag_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_flag != null)
            {
                _flag.ReleaseMouseCapture();
                if (!(DataContext is PlayerVM playerVM))
                {
                    Log.Error("Can't save the flag position because WaveFormSeekBar DataContext is not PlayerVM.");
                    return;
                }
                if (playerVM.Session.PlayingSong == null)
                {
                    Log.Error("Can't save the flag position because PlayingSong is not PlayerVM.");
                    return;
                }
                if (_flag == StartScrollFlag)
                    playerVM.Session.PlayingSong.ScrollStartTime = (int) Math.Floor(playerVM.Session.Player.Length * Canvas.GetLeft(StartScrollFlag) / Canvas.ActualWidth);
                else if (_flag == EndScrollFlag)
                    playerVM.Session.PlayingSong.ScrollEndTime = (int) Math.Floor(playerVM.Session.Player.Length * Canvas.GetRight(EndScrollFlag) / Canvas.ActualWidth);
                _flag = null;
            }
        }

        #endregion
    }
}
