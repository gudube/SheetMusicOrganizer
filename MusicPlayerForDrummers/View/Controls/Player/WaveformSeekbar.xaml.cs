using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MusicPlayerForDrummers.View.Tools;
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

        private void Session_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
            {
                Log.Warning("DataContext not PlayerVM in WaveFormSeekBar when Session property changed");
                return;
            }
            if(e.PropertyName == nameof(playerVM.Session.PlayingSong))
                UpdateWaveForm(playerVM.Session.PlayingSong?.AudioDirectory1 ?? string.Empty);
        }

        private void WaveformSeekBar_DragStarted(object? sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
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

        private Task<BitmapImage?>? _createImageTask;
        private CancellationTokenSource _cancelImageCreation = new CancellationTokenSource();

        private async void UpdateWaveForm(string audioDirectory)
        {
            if (_darkRendererSettings == null)
            {
                Log.Error("RendererSettings null when trying to create the waveform.");
                return;
            }

            if (string.IsNullOrEmpty(audioDirectory))
            {
                WaveFormImage.Visibility = Visibility.Hidden;
                return;
            }

            LoadingWaveFormText.Visibility = Visibility.Visible;
            WaveFormImage.Visibility = Visibility.Hidden;

            if (_createImageTask != null)
            {
                if (!_createImageTask.IsCompleted) {
                    try
                    {
                        //the task is running, cancel it and wait for it to be done before continuing
                        _cancelImageCreation.Cancel();
                        await _createImageTask;
                    }
                    catch (OperationCanceledException)
                    {
                        //nothing to do here
                    }
                }
                _createImageTask.Dispose();
                _cancelImageCreation.Dispose();
                _cancelImageCreation = new CancellationTokenSource();
            }
            
            CancellationToken ct = _cancelImageCreation.Token;

            _createImageTask = Task.Run(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    Image image;
                    try
                    {
                        image = _waveFormRenderer.Render(audioDirectory, new AveragePeakProvider(3), _darkRendererSettings, ct);
                    }catch(InvalidOperationException ex)
                    {
                        _cancelImageCreation.Cancel();
                        throw new FileFormatException(new Uri(audioDirectory), ex.Message);
                    }
                    ct.ThrowIfCancellationRequested();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        ct.ThrowIfCancellationRequested();
                        memory.Position = 0;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        return bitmapImage;
                    }
                }catch(OperationCanceledException)
                {
                    //normal behaviour, nothing to do here
                    return null;
                }
            }, ct);

            try
            {
                BitmapImage? imageSource = await _createImageTask;
                WaveFormImage.Source = imageSource;
                if(imageSource != null)
                {
                    WaveFormImage.Visibility = Visibility.Visible;
                    LoadingWaveFormText.Visibility = Visibility.Hidden;
                }
            }
            catch (OperationCanceledException)
            {
                WaveFormImage.Source = null;
            }
            catch (Exception ex)
            {
                WaveFormImage.Source = null;
                WaveFormImage.Visibility = Visibility.Hidden;
                LoadingWaveFormText.Visibility = Visibility.Hidden;
                WindowManager.OpenErrorWindow(ex);
            }
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
                grid.CaptureMouse();
                if (!(DataContext is PlayerVM playerVM))
                {
                    Log.Error("Can't evaluate the song length when DataContext is not PlayerVM.");
                    return;
                }
                // minimum 1 second gap between flags
                double minDistancePixels = FlagsCanvas.ActualWidth * 1 / playerVM.Session.Player.Length;

                if (grid == StartScrollFlag)
                {
                    _minPos = 0;
                    _maxPos = FlagsCanvas.ActualWidth - Canvas.GetRight(EndScrollFlag) - minDistancePixels;
                }
                else if (grid == EndScrollFlag)
                {
                    _minPos = Canvas.GetLeft(StartScrollFlag) + minDistancePixels;
                    _maxPos = FlagsCanvas.ActualWidth;
                }
                else if (grid == StartLoopFlag)
                {
                    _minPos = 0;
                    _maxPos = Canvas.GetLeft(EndLoopFlag) - minDistancePixels;
                }
                else if (grid == EndLoopFlag)
                {
                    _minPos = Canvas.GetLeft(StartLoopFlag) + minDistancePixels;
                    _maxPos = FlagsCanvas.ActualWidth;
                }

                _flag = grid;
            }
        }

        private void Flag_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_flag != null)
            {
                double posX = e.GetPosition(FlagsCanvas).X;
                if (posX < _minPos)
                    posX = _minPos;
                else if (posX > _maxPos)
                    posX = _maxPos;
                
                if(_flag == EndScrollFlag)
                    Canvas.SetRight(_flag, FlagsCanvas.ActualWidth - posX);
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
                    playerVM.Session.PlayingSong.ScrollStartTime = (int) Math.Floor(playerVM.Session.Player.Length * Canvas.GetLeft(_flag) / FlagsCanvas.ActualWidth);
                else if (_flag == EndScrollFlag)
                    playerVM.Session.PlayingSong.ScrollEndTime = (int) Math.Floor(playerVM.Session.Player.Length * Canvas.GetRight(_flag) / FlagsCanvas.ActualWidth);
                else if (_flag == StartLoopFlag)
                    playerVM.Session.Player.LoopStart = Math.Round(playerVM.Session.Player.Length * Canvas.GetLeft(_flag) / FlagsCanvas.ActualWidth, 1, MidpointRounding.ToZero);
                else if (_flag == EndLoopFlag)
                    playerVM.Session.Player.LoopEnd = Math.Round(playerVM.Session.Player.Length * Canvas.GetLeft(_flag) / FlagsCanvas.ActualWidth, 1, MidpointRounding.ToPositiveInfinity);

                _flag = null;
            }
        }

        #endregion
    }
}
