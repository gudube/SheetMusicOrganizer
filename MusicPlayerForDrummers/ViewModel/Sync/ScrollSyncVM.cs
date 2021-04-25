using System;
using System.ComponentModel;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.ViewModel.Tools;

namespace MusicPlayerForDrummers.ViewModel.Sync
{
    public class ScrollSyncVM : BaseViewModel
    {
        public ScrollSyncVM(SessionContext session) : base(session)
        {
            SetStartNowCommand = new DelegateCommand(SetStartNow);
            SetEndNowCommand = new DelegateCommand(SetEndNow);
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public override string ViewModelName => "Continuous Scroll";

        private SongItem? _syncingSong;
        public SongItem? SyncingSong
        {
            get => _syncingSong;
            set => SetField(ref _syncingSong, value);
        }

        public DelegateCommand SetStartNowCommand { get; }
        private void SetStartNow(object? obj)
        {
            if (SyncingSong != null)
            {
                double timeFromStart = Session.Player.Position;
                double endFromStart = Session.Player.Length - SyncingSong.ScrollEndTime;
                if (timeFromStart > endFromStart - 10)
                    return;
                SyncingSong.ScrollStartTime = (int) Math.Ceiling(timeFromStart);
            }
        }

        public DelegateCommand SetEndNowCommand { get; }
        private void SetEndNow(object? obj)
        {
            if (SyncingSong != null)
            {
                double timeFromStart = Session.Player.Position;
                if (timeFromStart < SyncingSong.ScrollStartTime + 10)
                    return;
                SyncingSong.ScrollEndTime = (int) Math.Floor(Session.Player.Length - timeFromStart);
            }
        }

    }
}
