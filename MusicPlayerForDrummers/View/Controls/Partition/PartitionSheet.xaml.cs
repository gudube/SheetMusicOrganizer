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
using Windows.Storage.Streams;
using MusicPlayerForDrummers.Model.Items;
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

        private void PartitionSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PartitionVM partitionVM) {
                PlayingSong_PropertyChanged(partitionVM.Session.PlayingSong);
                partitionVM.Session.PropertyChanged += Session_PropertyChanged;
                //partitionVM.Session.PlayerTimerUpdate += TimerUpdate;
                partitionVM.Session.Player.PropertyChanged += Player_PropertyChanged;
            }
        }
        
        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartitionVM.Session.Player.Position))
                UpdateScrollPos();

        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SessionContext sessionContext = (SessionContext)sender;
            if (e.PropertyName == nameof(sessionContext.PlayingSong))
                PlayingSong_PropertyChanged(sessionContext.PlayingSong);
        }

        #region PDFViewer
        //public static readonly DependencyProperty PartitionDirProperty = DependencyProperty.Register("PdfPath", typeof(string), typeof(PartitionSheet),
        //       new PropertyMetadata(null, propertyChangedCallback: OnPartitionDirChanged));
        //public string PartitionDir { get => (string)GetValue(PartitionDirProperty); set => SetValue(PartitionDirProperty, value); }
        private void PlayingSong_PropertyChanged(SongItem song)
        {
            if (song == null)
                return;

            string partitionDir = song.PartitionDirectory;

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

        private async Task PdfToImages(PdfDocument pdfDoc)
        {
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
                        MaxWidth = 800
                    };
                    PagesContainer.Items.Add(image);
                }
            }
        }

        private async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                await page.RenderToStreamAsync(stream);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }
        #endregion

        private void UpdateScrollPos()
        {
            if (DataContext is PartitionVM partitionVM)
            {

                double updatedPos = partitionVM.Session.Player.Position - partitionVM.Session.PlayingSong.ScrollStartTime;
                double updatedLength = partitionVM.Session.Player.Length - partitionVM.Session.PlayingSong.ScrollStartTime - partitionVM.Session.PlayingSong.ScrollEndTime;
                if(updatedPos <= 0 || updatedLength <= 0)
                {
                    Scrollbar.ScrollToVerticalOffset(0);
                }
                else
                {
                    updatedPos = (updatedPos / updatedLength) * Scrollbar.ScrollableHeight;
                    Scrollbar.ScrollToVerticalOffset(updatedPos);
                }
            }
        }

        private double _zoom = 1.0;
        private void PartitionSheet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.Subtract)
            {
                double verticalScrollRatio = Math.Abs(Scrollbar.ScrollableHeight) < 0.0001 ? 0 : (Scrollbar.VerticalOffset / Scrollbar.ScrollableHeight);
                double horizontalScrollRatio = Math.Abs(Scrollbar.ScrollableWidth) < 0.0001 ? 0.5 : (Scrollbar.HorizontalOffset / Scrollbar.ScrollableWidth);
                double newZoom = _zoom + (e.Key == Key.Add ? 0.1 : -0.1);
                if (newZoom <= 0.01)
                    return;

                _zoom = newZoom;
                foreach (Image image in PagesContainer.Items)
                    image.LayoutTransform = new ScaleTransform(_zoom, _zoom);

                Scrollbar.UpdateLayout();
                Scrollbar.ScrollToVerticalOffset(verticalScrollRatio * Scrollbar.ScrollableHeight);
                Scrollbar.ScrollToHorizontalOffset(horizontalScrollRatio * Scrollbar.ScrollableWidth);
            }
        }
    }
}
