using System;
using System.ComponentModel;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;

namespace SheetMusicOrganizer.ViewModel.Sync
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
            set
            {
                if (SetField(ref _syncingSong, value))
                {
                    SettingStartPageScroll = false;
                    SettingEndPageScroll = false;
                }
            }
        }

        public DelegateCommand SetStartNowCommand { get; }
        private void SetStartNow(object? obj)
        {
            if (SyncingSong != null)
            {
                double timeFromStart = Session.Player.Position;
                double endFromStart = Session.Player.Length - SyncingSong.ScrollEndTime;
                if (timeFromStart > endFromStart - 3)
                    timeFromStart = endFromStart - 3;
                SyncingSong.ScrollStartTime = (int) Math.Ceiling(timeFromStart);
            }
        }

        public DelegateCommand SetEndNowCommand { get; }
        private void SetEndNow(object? obj)
        {
            if (SyncingSong != null)
            {
                double timeFromStart = Session.Player.Position;
                if (timeFromStart < SyncingSong.ScrollStartTime + 3)
                    timeFromStart = SyncingSong.ScrollStartTime + 3;
                SyncingSong.ScrollEndTime = (int) Math.Floor(Session.Player.Length - timeFromStart);
            }
        }
        private bool _settingStartPageScroll = false;
        public bool SettingStartPageScroll
        {
            get => _settingStartPageScroll;
            set => SetField(ref _settingStartPageScroll, value);
        }

        private bool _settingEndPageScroll = false;
        public bool SettingEndPageScroll
        {
            get => _settingEndPageScroll;
            set => SetField(ref _settingEndPageScroll, value);
        }
    }
}
