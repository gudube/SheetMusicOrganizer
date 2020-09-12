using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MusicPlayerForDrummers.Model.Items;
using Serilog;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PartitionVM : BaseViewModel
    {
        public override string ViewModelName => "PARTITION";

        public PartitionVM(SessionContext session) : base(session)
        {
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
            set => SetField(ref _shownSong, value);
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
        public IEnumerable<SyncMethod> SyncMethods => Enum.GetValues(typeof(SyncMethod)).Cast<SyncMethod>();

        private SyncMethod _selectedSync = SyncMethod.None;
        public SyncMethod SelectedSync { get => _selectedSync; set => SetField(ref _selectedSync, value); }
        #endregion

    }

    public enum SyncMethod
    {
        None,
        Scrolling,
        Page,
        Staff
    }
    
}
