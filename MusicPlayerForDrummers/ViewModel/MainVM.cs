using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class MainVM : BaseViewModel
    {
        public LibraryVM LibraryVM { get; set; } = new LibraryVM();
        public PartitionVM PartitionVM { get; set; } = new PartitionVM();
        public SyncVM SyncVM { get; set; } = new SyncVM();
        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel
        { 
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (value == _currentViewModel)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public MainVM()
        {
            CurrentViewModel = LibraryVM;
        }
    }
}
