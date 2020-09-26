using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel.Sync
{
    public class PageSyncVM : BaseViewModel
    {
        public PageSyncVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public override string ViewModelName => "Scroll By Page";
    }
}
