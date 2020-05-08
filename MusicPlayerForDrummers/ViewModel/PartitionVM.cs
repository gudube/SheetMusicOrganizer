using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.Model.Tools;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PartitionVM : BaseViewModel
    {
        public override string ViewModelName => "PARTITION";


        public PartitionVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    
}
