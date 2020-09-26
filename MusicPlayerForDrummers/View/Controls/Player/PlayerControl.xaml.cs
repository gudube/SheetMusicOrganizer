using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MusicPlayerForDrummers.ViewModel;
using NAudioWrapper.WaveFormRendererLib;
using Serilog;
using Image = System.Drawing.Image;

namespace MusicPlayerForDrummers.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    /// TODO: Add button (on the right of the player?) to remove the waveform part (only keep buttons)
    /// on top of the LibraryMenu or PartitionMenu, leaving more place for the rest 
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
            DataContextChanged += PlayerControl_DataContextChanged;
            Loaded += PlayerControl_Loaded;
        }

        private void PlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = ((SolidColorBrush) Control.Background).Color;
            _darkRendererSettings = new SoundCloudOriginalSettings
            {
                Width = (int) SeekBar.ActualWidth,
                TopHeight = (int) (SeekBar.ActualHeight*0.6), 
                BottomHeight = (int) (SeekBar.ActualHeight*0.4),
                BackgroundColor = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B)
            };
        }

        private void PlayerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
                Log.Warning("DataContext not PlayerVM in PlayerControl when Session property changed");
                return;
            }
            if(e.PropertyName == nameof(playerVM.Session.PlayingSong))
                UpdateWaveForm(playerVM.Session.PlayingSong?.AudioDirectory ?? string.Empty);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            if (playerVM.Session.PlayingSong == null)
                return;

            string? dir = Path.GetDirectoryName(playerVM.Session.PlayingSong.PartitionDirectory);
            if(dir != null)
                Process.Start("explorer.exe", dir);
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

        
        #region WaveForm Render
        private readonly WaveFormRenderer _waveFormRenderer = new WaveFormRenderer();

        private WaveFormRendererSettings? _darkRendererSettings;

        private void UpdateWaveForm(string audioDirectory)
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
            
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                //TODO: add more invokeasync like that (or using thread?) to improve performance
                Image image = _waveFormRenderer.Render(audioDirectory, new AveragePeakProvider(3), _darkRendererSettings);
                using (MemoryStream memory = new MemoryStream())
                {
                    image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    WaveFormImage.Source = bitmapImage;
                }

                WaveFormImage.Visibility = Visibility.Visible;
                LoadingWaveFormText.Visibility = Visibility.Hidden;
            }, DispatcherPriority.Background);
        }

        #endregion

    }
}
