using System.ComponentModel;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.ViewModel.Sync
{
    public class ScrollSyncVM : BaseViewModel
    {
        public ScrollSyncVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public override string ViewModelName => "Continuous Scroll";

        private SongItem? _syncingSong;
        public SongItem? SyncingSong
        {
            get => _syncingSong;
            set => SetField(ref _syncingSong, value);
        }
    }
}
