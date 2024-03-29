﻿using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NAudioWrapper;
using NAudioWrapper.WaveFormRendererLib;
using Serilog;
using SheetMusicOrganizer.ViewModel;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace SheetMusicOrganizer.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for WaveFormSeekBar.xaml
    /// </summary>
    public partial class WaveFormSeekBar : UserControl
    {

        public WaveFormSeekBar()
        {
            Loaded += WaveFormSeekBar_Loaded;
            Unloaded += (s, e) =>
            {
                Settings.Default.SettingsSaving -= Default_SettingsSaving;
            };
            InitializeComponent();
            DataContextChanged += WaveFormSeekBar_DataContextChanged;
            _flagConverter = (FlagTimeConverter)FlagsCanvas.FindResource("FlagTimeConverter");
            _flagConverter.FlagCanvas = FlagsCanvas;
            FlagsCanvas.SizeChanged += (_, _) => refreshFlags();
        }

        private void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var newColorSettings = Settings.Default.Theme == "Dark" ? _darkRendererSettings : _lightRendererSettings;
            if(_colorSettings != newColorSettings)
            {
                _colorSettings = newColorSettings;
                if(DataContext is PlayerVM playerVM)
                    UpdateWaveForm(playerVM.Session.PlayingSong?.AudioDirectory1 ?? string.Empty);
            }
        }

        #region events

        private void WaveFormSeekBar_Loaded(object sender, RoutedEventArgs e)
        {
            _darkRendererSettings = new SoundCloudOriginalSettings
            {
                Width = (int)SeekBar.ActualWidth,
                TopHeight = (int)(SeekBar.ActualHeight * 0.6),
                BottomHeight = (int)(SeekBar.ActualHeight * 0.4),
                BackgroundColor = Color.Transparent,
                TopColor1 = Color.FromArgb(220, 220, 220),
                TopColor2 = Color.FromArgb(100, 100, 100),
                BottomColor1 = Color.FromArgb(60, 60, 60),
                BottomColor2 = Color.FromArgb(82, 82, 82),
                BottomColor3 = Color.FromArgb(90, 90, 90)
            };
            _lightRendererSettings = new SoundCloudOriginalSettings
            {
                Width = (int)SeekBar.ActualWidth,
                TopHeight = (int)(SeekBar.ActualHeight * 0.6),
                BottomHeight = (int)(SeekBar.ActualHeight * 0.4),
                BackgroundColor = Color.Transparent,
            };
            _colorSettings = Settings.Default.Theme == "Dark" ? _darkRendererSettings : _lightRendererSettings;
            Settings.Default.SettingsSaving += Default_SettingsSaving;
        }

        private void WaveFormSeekBar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PlayerVM oldPlayerVM)
                oldPlayerVM.Session.PropertyChanged -= Session_PropertyChanged;

            if(e.NewValue is PlayerVM playerVM)
            {
                playerVM.Session.PropertyChanged += Session_PropertyChanged;
                _flagConverter.Player = playerVM.Session.Player;
            }
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
            
            playerVM.Session.Player.PropertyChanged += SessionPlayer_PropertyChanged;
        }

        private void SessionPlayer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerVM.Session.Player.Length))
                refreshFlags();
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

        #endregion

        private readonly WaveFormRenderer _waveFormRenderer = new WaveFormRenderer();

        private WaveFormRendererSettings? _darkRendererSettings;
        private WaveFormRendererSettings? _lightRendererSettings;
        private WaveFormRendererSettings? _colorSettings;


        #region Render WaveForm

        private Task<BitmapImage?>? _createImageTask;
        private CancellationTokenSource _cancelImageCreation = new CancellationTokenSource();

        private async void UpdateWaveForm(string audioDirectory)
        {
            if (_colorSettings == null)
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"RendererSettings null when trying to create the waveform."));
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

            if (string.IsNullOrEmpty(audioDirectory))
            {
                WaveFormImage.Source = null;
                WaveFormImage.Visibility = Visibility.Hidden;
                LoadingWaveFormText.Visibility = Visibility.Hidden;
                return;
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
                        image = _waveFormRenderer.Render(audioDirectory, new AveragePeakProvider(3), _colorSettings, ct);
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
                GlobalEvents.raiseErrorEvent(ex, $"There was an error when trying to create the waveform image for the song: {audioDirectory}");
            }
        }
        #endregion

        #region Flags
        //private bool _draggingFlag = false;
        private double _minPos = 0;
        private double _maxPos = 1;
        private Grid? _flag;
        private FlagTimeConverter _flagConverter;
        private void Flag_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to evaluate the song length when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            if (sender is Grid grid)
            {
                grid.CaptureMouse();

                // minimum 2 seconds gap between flags
                double minDistancePixels = FlagsCanvas.ActualWidth * 2 / playerVM.Session.Player.Length;

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
            if(_flag != null)
            {
                _flag.ReleaseMouseCapture();
                _flag.GetBindingExpression(Canvas.LeftProperty)?.UpdateSource();
                _flag.GetBindingExpression(Canvas.RightProperty)?.UpdateSource();
                _flag = null;
            }
        }

        private void refreshFlags()
        {
            StartLoopFlag.GetBindingExpression(Canvas.LeftProperty)?.UpdateTarget();
            StartScrollFlag.GetBindingExpression(Canvas.LeftProperty)?.UpdateTarget();
            EndLoopFlag.GetBindingExpression(Canvas.LeftProperty)?.UpdateTarget();
            EndScrollFlag.GetBindingExpression(Canvas.RightProperty)?.UpdateTarget();
        }

        #endregion
    }


    public class FlagTimeConverter : IValueConverter
    {
        public AudioPlayer? Player;
        public FrameworkElement? FlagCanvas;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(FlagCanvas != null && Player != null && Player.Length > 1 && Double.TryParse(System.Convert.ToString(value), out var scrollTime) && Double.IsFinite(scrollTime))
                return scrollTime * FlagCanvas.ActualWidth / Player.Length;

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Player == null || FlagCanvas == null || !Double.TryParse(System.Convert.ToString(value), out var leftOrRight) || !Double.IsFinite(leftOrRight))
                return 0;

            if (parameter is "StartScrollFlag" or "EndScrollFlag")
                return (int)Math.Floor(Player.Length * leftOrRight / FlagCanvas.ActualWidth);
            else if (parameter is "StartLoopFlag" or "EndLoopFlag")
                return Player.Length * leftOrRight / FlagCanvas.ActualWidth;

            Log.Error("Tried setting a flag value on an inexisting flag: {flagName}", parameter);
            return 0;
        }
    }
}
