using SheetMusicOrganizer.Model.Items;
using System.ComponentModel;

namespace SheetMusicOrganizer.ViewModel.Sync
{
    public class PageSyncVM : BaseSyncVM
    {
        public PageSyncVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public override double? GetPartitionPos()
        {
            return null;
        }

        public override string ViewModelName => "Scroll By Page";

        public override SongItem? SyncingSong { get; set; }
        public override double ScrollableHeight { get; set; }
        public override double ExtentHeight { get; set; }
    }
}
