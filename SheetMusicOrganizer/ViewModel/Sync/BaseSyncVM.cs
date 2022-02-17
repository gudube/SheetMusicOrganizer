using System.ComponentModel;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.Model.Tools;

namespace SheetMusicOrganizer.ViewModel.Sync
{
    public abstract class BaseSyncVM : BaseViewModel
    {
        public BaseSyncVM(SessionContext session) : base(session)
        {
        }

        public abstract double? GetPartitionPos();

        public abstract SongItem? SyncingSong { get; set; }

        public abstract double ScrollableHeight { get; set; }

        public abstract double ExtentHeight { get; set; }

    }
}
