using System.Collections.ObjectModel;
using System.ComponentModel;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.ViewModel.Sync;
using Serilog;

namespace MusicPlayerForDrummers.ViewModel
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
            _selectedSyncVM = _scrollSyncVM;
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
        //todo: can keep these properties in the settings and let the user modify them?
        private readonly double minZoom = 0.01;
        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value < minZoom) value = minZoom;
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
            set => SetField(ref _selectedSyncVM, value);
        }
        #endregion

        #region Playback settings

        public bool UseCustomSpeed => Session.Player.UseCustomSpeed;

        public double CustomSpeed => Session.Player.CustomSpeed;
        public bool KeepPitch => Session.Player.KeepPitch;

        private bool _isLooping = false;
        public bool IsLooping
        {
            get => _isLooping;
            set => SetField(ref _isLooping, value);
        }

        private double _loopStart = 1.0;
        public double LoopStart
        {
            get => _loopStart ;
            set => SetField(ref _loopStart , value);
        }

        private double _loopEnd = 1.0;
        public double LoopEnd
        {
            get => _loopEnd;
            set => SetField(ref _loopEnd, value);
        }
        #endregion
    }
}
