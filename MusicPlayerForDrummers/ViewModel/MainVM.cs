using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.View;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class MainVM : BaseViewModel
    {
        public override string ViewModelName => "MAIN";

        public MainVM() : base(new SessionContext())
        {
            DBHandler.InitializeDatabase();

            SwitchLibraryViewCommand = new DelegateCommand(x => SetView(_libraryVM));
            SwitchPartitionViewCommand = new DelegateCommand(x => SetView(_partitionVM), x => Session.SelectedSongs.Count > 0);
            Session.SelectedSongs.CollectionChanged += (sender, args) => SwitchPartitionViewCommand.RaiseCanExecuteChanged();

            _libraryVM = new LibraryVM(Session);
            _partitionVM = new PartitionVM(Session);
            SetView(_libraryVM);
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        #region Child VMs
        private LibraryVM _libraryVM { get; }
        private PartitionVM _partitionVM { get; }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetField(ref _currentViewModel, value); }

        public DelegateCommand SwitchLibraryViewCommand { get; private set; }
        public DelegateCommand SwitchPartitionViewCommand { get; private set; }

        private void SetView(BaseViewModel view)
        {
            if (CurrentViewModel == view)
                return;

            CurrentViewModel = view;
            if (view == _partitionVM)
                Session.PlayingSong = Session.SelectedSongs.FirstOrDefault();
        }
        #endregion

        #region Menu
        public bool IsSongExisting(string partitionFilename)
        {
            return DBHandler.IsSongExisting(partitionFilename);
        }

        public void GoToSong(string partitionFilename)
        {
            SongItem song = DBHandler.GetSong(partitionFilename);
            SetView(_libraryVM);
            _libraryVM.GoToSong(song);
        }

        public void AddNewSong(SongItem song)
        {
            _libraryVM.AddNewSong(song);
            SetView(_libraryVM);
            _libraryVM.GoToSong(song);
        }

        
        #endregion

        public enum EDirection
        {
            Left = -1,
            Right = +1
        }
    }
}
