using System.ComponentModel;

namespace SheetMusicOrganizer.ViewModel.Sync
{
    public class PageSyncVM : BaseViewModel
    {
        public PageSyncVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public override string ViewModelName => "Scroll By Page";
    }
}
