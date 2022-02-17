using System;
using System.ComponentModel;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;

namespace SheetMusicOrganizer.ViewModel.Sync
{
    public class ScrollSyncVM : BaseSyncVM
    {
        public ScrollSyncVM(SessionContext session) : base(session)
        {
            SetStartNowCommand = new DelegateCommand(SetStartNow);
            SetEndNowCommand = new DelegateCommand(SetEndNow);
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Session.Player))
                Session.Player.PropertyChanged += Player_PropertyChanged;
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Session.Player.Length))
                RecalculateFormula();
        }

        public override string ViewModelName => "Continuous Scroll";

        private SongItem? _syncingSong;
        public override SongItem? SyncingSong
        {
            get => _syncingSong;
            set
            {
                if(_syncingSong != null)
                    _syncingSong.PropertyChanged -= SyncingSong_PropertyChanged;
                if(value != null)
                    value.PropertyChanged += SyncingSong_PropertyChanged;
                if (SetField(ref _syncingSong, value))
                {
                    SettingStartPageScroll = false;
                    SettingEndPageScroll = false;
                    RecalculateFormula();
                }

            }
        }

        private void SyncingSong_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SongItem.ScrollStartTime) || e.PropertyName == nameof(SongItem.ScrollEndTime)
                || e.PropertyName == nameof(SongItem.PagesStartPercentage) || e.PropertyName == nameof(SongItem.PagesEndPercentage))
            {
                RecalculateFormula();
            }
        }

        #region Song Position Formula
        /**
         * x: Session.Player.Position
         * a: SyncingSong.ScrollStartTime
         * b: SyncingSong.ScrollEndTime
         * c: Session.Player.Length
         * d: SyncingSong.PagesStartPercentage
         * e: SyncingSong.PagesEndPercentage
         * f: Scrollbar.ExtentHeight (f = g + h)
         * g: Scrollbar.ScrollableHeight
         * h: Scrollbar.viewPortHeight (not used)
         * y: returned value, ScrollToVerticalOffset parameter
         * 
         * y = (g * (x - a) / (c - a - b)) * (e*f-h - (d*f))/g + (d * f) = (x - a) / (c - a - b) * (e*g - d*f) + (d * f)
         * if z = (e*g - d*f)/(c-a-b) then y = z*x - z*a + d*f
         * where slope = z and yIntercept = -z*a + d*f
         */
        private double slope = 0, yIntercept = 0, minY = 0, maxY = 0;

        public override double? GetPartitionPos()
        {
            var x = Session.Player.Position;
            var y = slope * x + yIntercept;
            return Math.Max(minY, Math.Min(maxY, y));
        }
        private void RecalculateFormula()
        {
            if(SyncingSong == null || ScrollableHeight <= 0 || ExtentHeight <= 0)
            {
                slope = 0;
                yIntercept = 0;
                return;
            }
            var a = SyncingSong.ScrollStartTime;
            var b = SyncingSong.ScrollEndTime;
            var c = Session.Player.Length;
            var d = SyncingSong.PagesStartPercentage;
            var e = SyncingSong.PagesEndPercentage;
            var f = ExtentHeight;
            var g = ScrollableHeight;

            minY = d * f;
            maxY = e * f - (f-g);
            slope = (maxY - minY) / (c - a - b);
            yIntercept = - (slope * a) + minY;

        }
        private double _scrollableHeight = 0;
        public override double ScrollableHeight
        {
            get => _scrollableHeight;
            set
            {
                if (SetField(ref _scrollableHeight, value))
                    RecalculateFormula();
            }
        }
        private double _extentHeight;
        public override double ExtentHeight
        {
            get => _extentHeight;
            set
            {
                if (SetField(ref _extentHeight, value))
                    RecalculateFormula();
            }
        }

        #endregion

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
