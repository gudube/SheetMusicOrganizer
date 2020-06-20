using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.Model.Tools;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PartitionVM : BaseViewModel
    {
        public override string ViewModelName => "PARTITION";

        public PartitionVM(SessionContext session) : base(session)
        {
            session.PlayerTimerUpdate += TimerUpdate;
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private int _pageZoom = 100;
        public int PageZoom { get => _pageZoom; set => SetField(ref _pageZoom, value); }

        private double _pagePosition = 0.0;
        public double PagePosition { get => _pagePosition; set => SetField(ref _pagePosition, value); }


        #region Sync
        public IEnumerable<SyncMethod> SyncMethods { get => Enum.GetValues(typeof(SyncMethod)).Cast<SyncMethod>(); }
        
        private SyncMethod _selectedSync = SyncMethod.None;
        public SyncMethod SelectedSync { get => _selectedSync; set => SetField(ref _selectedSync, value); }

        public void TimerUpdate()
        {

        }
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
