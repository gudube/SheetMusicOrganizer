using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PartitionVM : BaseViewModel
    {
        public override string ViewModelName => "PARTITION";

        public PartitionVM(SessionContext session) : base(session)
        {
            //session.PlayerTimerUpdate += TimerUpdate;
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
                UpdateFromPlayingSong();
        }

        private void UpdateFromPlayingSong()
        {
            //todo: get info from DB
            //for now:
            //ScrollSpeed = 1;
            //if (Session.PlayingSong == null)
            //    return;

            //StartScrollTime = Session.PlayingSong.Star;
        }

        private int _pageZoom = 100;
        public int PageZoom { get => _pageZoom; set => SetField(ref _pageZoom, value); }

        private double _pagePosition = 0.0;
        public double PagePosition { get => _pagePosition; set => SetField(ref _pagePosition, value); }


        #region Sync
        public IEnumerable<SyncMethod> SyncMethods { get => Enum.GetValues(typeof(SyncMethod)).Cast<SyncMethod>(); }
        
        private SyncMethod _selectedSync = SyncMethod.None;
        public SyncMethod SelectedSync { get => _selectedSync; set => SetField(ref _selectedSync, value); }
        /*
        private double _scrollSpeed = 1;
        public double ScrollSpeed { get => _scrollSpeed; set => SetField(ref _scrollSpeed, value); }

        private int _startScrollTime = 0;
        public int StartScrollTime { get => _startScrollTime; set => SetField(ref _startScrollTime, value); }

        private int _endScrollTime = 0;
        public int EndScrollTime { get => _endScrollTime; set => SetField(ref _endScrollTime, value); }*/
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
