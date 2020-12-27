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
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Partition
{
    /// <summary>
    /// Interaction logic for PartitionSheet.xaml
    /// </summary>
    public partial class PartitionSheet : UserControl
    {
        public PartitionSheet()
        {
            InitializeComponent();
            DataContextChanged += PartitionSheet_DataContextChanged;
            this.KeyDown += PartitionSheet_KeyDown;
        }

        private void PartitionSheet_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(!Focus())
                Log.Warning("Could not get focus on partition sheet once loaded.");
        }

        #region Changed events
        private void PartitionSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PartitionVM oldVM)
            {
                oldVM.Session.Player.PropertyChanged -= Player_PropertyChanged;
                oldVM.PropertyChanged -= PartitionVM_PropertyChanged;
            }

            if (e.NewValue is PartitionVM newVM) {
                OpenShownSongPartition();
                newVM.Session.Player.PropertyChanged += Player_PropertyChanged;
                newVM.PropertyChanged += PartitionVM_PropertyChanged;
            }
        }

        private void PartitionVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartitionVM.Zoom))
                UpdateZoom();
            else if (e.PropertyName == nameof(PartitionVM.ShownSong))
                OpenShownSongPartition();
        }

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartitionVM.Session.Player.Position))
                UpdateScrollPos();
        }

        #endregion

        #region PDFViewer
        private void OpenShownSongPartition()
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to open partition when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            if (partitionVM.ShownSong == null)
            {
                Log.Error("Trying to open partition when shown song is null");
                return;
            }

            string partitionDir = partitionVM.ShownSong.PartitionDirectory;

            if (!string.IsNullOrWhiteSpace(partitionDir))
            {
                //making sure it's an absolute path
                var path = Path.GetFullPath(partitionDir);

                StorageFile.GetFileFromPathAsync(path).AsTask() //Get File as Task
                    //Then load pdf document on background thread
                    .ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask()).Unwrap()
                    //Finally display on UI Thread
                    .ContinueWith(t2 => PdfToImages(t2.Result), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async Task PdfToImages(PdfDocument? pdfDoc)
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to UpdateScrollPos when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            PagesContainer.Items.Clear();

            if (pdfDoc == null) return;

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                using (var page = pdfDoc.GetPage(i)) //get each page and convert
                {
                    var bitmap = await PageToBitmapAsync(page);
                    var image = new Image
                    {
                        Source = bitmap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 4),
                        Width = 1200 * partitionVM.Zoom
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
                await page.RenderToStreamAsync(stream.AsRandomAccessStream());

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.DecodePixelWidth = 1400;
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
                Log.Error("Trying to UpdateScrollPos when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            double posPercentage = partitionVM.GetSongPercentage();
            Scrollbar.ScrollToVerticalOffset(posPercentage * Scrollbar.ScrollableHeight);
        }

        private void UpdateZoom()
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to UpdateScrollPos when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
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
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to zoom/unzoom when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            if (e.Key == Key.Add || e.Key == Key.Subtract)
            {
                partitionVM.Zoom += (e.Key == Key.Add ? 0.1 : -0.1);
            }
        }

        private void PlusButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to zoom when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            partitionVM.Zoom += 0.1;
        }

        private void MinusButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is PartitionVM partitionVM))
            {
                Log.Error("Trying to zoom when DataContext is not a PartitionVM but is a {dataContext}", DataContext?.GetType());
                return;
            }

            partitionVM.Zoom -= 0.1;
        }
        #endregion
    }
}
