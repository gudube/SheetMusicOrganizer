using System.Collections.ObjectModel;
using System.ComponentModel;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Sync;

namespace SheetMusicOrganizer.ViewModel
{
    public class PartitionVM : BaseViewModel
    {
        public override string ViewModelName => "PARTITION";

        public PartitionVM(SessionContext session) : base(session)
        {
            _noSyncVM = new NoSyncVM(session);
            _scrollSyncVM = new ScrollSyncVM(session);
            _pageSyncVM = new PageSyncVM(session);
            SyncViewModels = new ObservableCollection<BaseViewModel>()
            {
                _noSyncVM, _scrollSyncVM, _pageSyncVM
            };
            SelectedSyncVM = _scrollSyncVM;
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
            {
                if(Session.PlayingSong != null)
                    ShownSong = Session.PlayingSong;
            }
        }

        private SongItem? _shownSong;
        public SongItem? ShownSong
        {
            get => _shownSong;
            set
            {
                if (SetField(ref _shownSong, value))
                    _scrollSyncVM.SyncingSong = value;
            }
        }

        #region Visual properties
        private readonly double minZoom = 0.2;
        private readonly double maxZoom = 5;
        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value < minZoom) value = minZoom;
                if (value > maxZoom) value = maxZoom;
                SetField(ref _zoom, value);
            }
        }

        public double GetSongPercentage()
        {
            if (ShownSong == null)
            {
                Log.Warning("Tried to get position in song for PartitionVM when shown song is null");
                return 0;
            }

            double updatedPos = Session.Player.Position - ShownSong.ScrollStartTime;
            double updatedLength = Session.Player.Length - ShownSong.ScrollStartTime - ShownSong.ScrollEndTime;
            if (updatedPos <= 0 || updatedLength <= 0) 
                return 0;
            return updatedPos / updatedLength;
        }
        #endregion

        #region Sync Method
        private readonly NoSyncVM _noSyncVM;
        private readonly ScrollSyncVM _scrollSyncVM;
        private readonly PageSyncVM _pageSyncVM;
        public ObservableCollection<BaseViewModel> SyncViewModels { get; }

        private BaseViewModel _selectedSyncVM;
        public BaseViewModel SelectedSyncVM
        {
            get => _selectedSyncVM;
            set
            {
                if (SetField(ref _selectedSyncVM, value))
                    UpdateShowPartitionMarkers();
            }
        }

        private bool? _showPartitionMarkers;
        public bool? ShowPartitionMarkers
        {
            get => _showPartitionMarkers;
            set => SetField(ref _showPartitionMarkers, value);
        }

        private void UpdateShowPartitionMarkers()
        {
            ShowPartitionMarkers = (SelectedSyncVM == _scrollSyncVM);
        }

        #endregion
    }
}
