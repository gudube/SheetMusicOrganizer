using System.ComponentModel;

namespace MusicPlayerForDrummers.ViewModel.Sync
{
    public class NoSyncVM : BaseViewModel
    {
        public NoSyncVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public override string ViewModelName => "None";
    }
}
