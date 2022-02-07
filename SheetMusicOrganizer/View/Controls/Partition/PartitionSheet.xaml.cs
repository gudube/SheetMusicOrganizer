using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Serilog;
using System.Threading;
using SheetMusicOrganizer.ViewModel;
using SheetMusicOrganizer.ViewModel.Sync;

namespace SheetMusicOrganizer.View.Controls.Partition
{
    /// <summary>
    /// Interaction logic for PartitionSheet.xaml
    /// </summary>
    public partial class PartitionSheet : UserControl
    {
        private uint resolution;
        public PartitionSheet()
        {
            DataContextChanged += PartitionSheet_DataContextChanged;
            resolution = Settings.Default.PdfResolution;
            this.KeyDown += PartitionSheet_KeyDown;
            Loaded += (s, e) =>
            {
                Settings.Default.SettingsSaving += Default_SettingsSaving;
            };
            Unloaded += (s, e) =>
            {
                Settings.Default.SettingsSaving -= Default_SettingsSaving;
            };
            InitializeComponent();
        }

        private void PartitionSheet_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(!Focus())
                Log.Warning("Could not get focus on partition sheet once loaded.");
        }

        #region Changed events
        private async void PartitionSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PartitionVM oldVM)
            {
                oldVM.Session.Player.PropertyChanged -= Player_PropertyChanged;
                oldVM.PropertyChanged -= PartitionVM_PropertyChanged;
            }

            if (e.NewValue is PartitionVM newVM) {
                newVM.Session.Player.PropertyChanged += Player_PropertyChanged;
                newVM.PropertyChanged += PartitionVM_PropertyChanged;
                await OpenShownSongPartition();
            }
        }

        private async void PartitionVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartitionVM.Zoom))
                UpdateZoom();
            else if (e.PropertyName == nameof(PartitionVM.ShownSong))
                await OpenShownSongPartition();
        }

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartitionVM.Session.Player.Position))
                UpdateScrollPos();
        }

        private async void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.Default.PdfResolution != resolution)
            {
                resolution = Settings.Default.PdfResolution;
                await OpenShownSongPartition();
            }
        }

        #endregion

        #region PDFViewer
        private Task? _createImageTask;
        private CancellationTokenSource _cancelImageCreation = new CancellationTokenSource();

        private async Task OpenShownSongPartition()
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to open partition when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            if (partitionVM.ShownSong == null)
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to open partition when the playing song (partitionVM.ShownSong) is null"));
                return;
            }

            string partitionDir = partitionVM.ShownSong.PartitionDirectory;

            if (!string.IsNullOrWhiteSpace(partitionDir))
            {
                //making sure it's an absolute path
                var path = Path.GetFullPath(partitionDir);

                if (!File.Exists(path))
                {
                    GlobalEvents.raiseErrorEvent(new FileNotFoundException("Trying to open a partition file that doesn't exist.", path));
                    return;
                }

                if (_createImageTask != null)
                {
                    if (!_createImageTask.IsCompleted)
                    {
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

                var getFileTask = StorageFile.GetFileFromPathAsync(path).AsTask(ct);
                var loadFileTask = getFileTask.ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask(ct), TaskContinuationOptions.NotOnCanceled).Unwrap();
                _createImageTask = loadFileTask.ContinueWith(t2 => PdfToImages(t2.Result, ct), ct, TaskContinuationOptions.NotOnCanceled, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

                try
                {
                    LoadingOverlay.Title = "LOADING...";
                    await _createImageTask;
                } catch(OperationCanceledException)
                {
                    PagesContainer.Items.Clear();
                }
                catch (Exception ex)
                {
                    GlobalEvents.raiseErrorEvent(new FileFormatException(new Uri(partitionDir), ex.Message));
                } finally
                {
                    LoadingOverlay.Title = "";
                }

            }
        }

        private async Task PdfToImages(PdfDocument? pdfDoc, CancellationToken? ct)
        {
            ct?.ThrowIfCancellationRequested();
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to use PdfToImages when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }
            PagesContainer.Items.Clear();

            if (pdfDoc == null) return;

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                LoadingOverlay.Title = $"LOADING...\r\n{i}/{pdfDoc.PageCount} pages";
                using (var page = pdfDoc.GetPage(i)) //get each page and convert
                {
                    ct?.ThrowIfCancellationRequested();
                    var bitmap = await PageToBitmapAsync(page);
                    var image = new Image
                    {
                        Source = bitmap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 4),
                        Width = 1200 * partitionVM.Zoom,
                    };
                    PagesContainer.Items.Add(image);
                }
            }
        }

        private async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new WrappingStream(new MemoryStream()))
            {
                await page.RenderToStreamAsync(stream.AsRandomAccessStream(), new PdfPageRenderOptions
                {
                    DestinationWidth = resolution
                });

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.DecodePixelWidth = (int) resolution;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }
        #endregion

        #region Controls
        private void UpdateScrollPos()
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to UpdateScrollPos when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            double posPercentage = partitionVM.GetSongPercentage();
            Scrollbar.ScrollToVerticalOffset(posPercentage * Scrollbar.ScrollableHeight);
        }

        private void UpdateZoom()
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to UpdateZoom when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            double verticalScrollRatio = Math.Abs(Scrollbar.ScrollableHeight) < 0.0001 ? 0 : (Scrollbar.VerticalOffset / Scrollbar.ScrollableHeight);
            double horizontalScrollRatio = Math.Abs(Scrollbar.ScrollableWidth) < 0.0001 ? 0.5 : (Scrollbar.HorizontalOffset / Scrollbar.ScrollableWidth);

            foreach (Image? image in PagesContainer.Items)
                if (image != null)
                    image.LayoutTransform = new ScaleTransform(partitionVM.Zoom, partitionVM.Zoom);

            Scrollbar.UpdateLayout();
            Scrollbar.ScrollToVerticalOffset(verticalScrollRatio * Scrollbar.ScrollableHeight);
            Scrollbar.ScrollToHorizontalOffset(horizontalScrollRatio * Scrollbar.ScrollableWidth);
        }

        private void PartitionSheet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.Subtract)
            {
                if (!(DataContext is PartitionVM partitionVM))
                {
                    GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to zoom/unzoom using keys when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                    return;
                }
                partitionVM.Zoom += (e.Key == Key.Add ? 0.1 : -0.1);
            }
        }

        private void PlusButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to zoom when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            partitionVM.Zoom += 0.1;
        }

        private void MinusButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Trying to unzoom when DataContext is not a PartitionVM, but is {DataContext?.GetType()}"));
                return;
            }

            partitionVM.Zoom -= 0.1;
        }
        #endregion

        #region Scroll Markers

        private UIElement? tempScrollMarker = null;
        private double? minValue = null;
        private double? maxValue = null;
        private void TempScrollMarker_Loaded(object sender, RoutedEventArgs e)
        {
            tempScrollMarker = sender as UIElement;
        }

        private void TempScrollMarker_Unloaded(object sender, RoutedEventArgs e)
        {
            tempScrollMarker = null;
        }

        private void TempScrollMarker_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                if (DataContext is PartitionVM partitionVM && partitionVM.SelectedSyncVM is ScrollSyncVM scrollVM && partitionVM.ShownSong != null)
                {
                    if (scrollVM.SettingStartPageScroll)
                        maxValue = partitionVM.ShownSong.PagesEndPercentage * Scrollbar.ExtentHeight - 50 * partitionVM.Zoom;
                    else if (scrollVM.SettingEndPageScroll)
                        minValue = partitionVM.ShownSong.PagesStartPercentage * Scrollbar.ExtentHeight + 50 * partitionVM.Zoom;
                }
            } else
            {
                maxValue = null;
                minValue = null;
            }
        }

        private void ScrollContent_MouseMove(object sender, MouseEventArgs e)
        {
            if(tempScrollMarker?.Visibility == Visibility.Visible)
            {
                if(minValue != null)
                    Canvas.SetTop(tempScrollMarker, Math.Max(minValue ?? double.MinValue, Scrollbar.VerticalOffset + e.GetPosition(Scrollbar).Y));
                else if(maxValue != null)
                    Canvas.SetTop(tempScrollMarker, Math.Min(maxValue ?? double.MaxValue, Scrollbar.VerticalOffset + e.GetPosition(Scrollbar).Y));
                else
                    Canvas.SetTop(tempScrollMarker, Scrollbar.VerticalOffset + e.GetPosition(Scrollbar).Y);

            }
        }

        private void ScrollContent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PartitionVM partitionVM && partitionVM.SelectedSyncVM is ScrollSyncVM scrollVM && partitionVM.ShownSong != null)
            {
                if (scrollVM.SettingStartPageScroll)
                {
                    if(tempScrollMarker?.Opacity < 1)
                        partitionVM.ShownSong.PagesStartPercentage = 0;
                    else
                        partitionVM.ShownSong.PagesStartPercentage = (float)(Canvas.GetTop(tempScrollMarker) / Scrollbar.ExtentHeight);

                    scrollVM.SettingStartPageScroll = false;
                }
                if (scrollVM.SettingEndPageScroll)
                {
                    if (tempScrollMarker?.Opacity < 1)
                        partitionVM.ShownSong.PagesEndPercentage = 1;
                    else
                        partitionVM.ShownSong.PagesEndPercentage = (float)(Canvas.GetTop(tempScrollMarker) / Scrollbar.ExtentHeight);
                    scrollVM.SettingEndPageScroll = false;
                }
            }
        }

        #endregion

    }
}
