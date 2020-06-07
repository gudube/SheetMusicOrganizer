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
            DBHandler.InitializeDatabase(true);

            SwitchLibraryViewCommand = new DelegateCommand(x => SetView(LibraryVM));
            SwitchPartitionViewCommand = new DelegateCommand(x => SetView(PartitionVM), x => Session.SelectedSongs.Count > 0);
            Session.SelectedSongs.CollectionChanged += (sender, args) => SwitchPartitionViewCommand.RaiseCanExecuteChanged();

            LibraryVM = new LibraryVM(Session);
            PartitionVM = new PartitionVM(Session);
            SetView(LibraryVM);
            PlayerVM = new PlayerVM(Session);
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        #region Child VMs
        public LibraryVM LibraryVM { get; }
        public PartitionVM PartitionVM { get; }
        public PlayerVM PlayerVM { get; }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetField(ref _currentViewModel, value); }

        public DelegateCommand SwitchLibraryViewCommand { get; private set; }
        public DelegateCommand SwitchPartitionViewCommand { get; private set; }

        private void SetView(BaseViewModel view)
        {
            if (CurrentViewModel == view)
                return;

            CurrentViewModel = view;
            if (view == PartitionVM)
                Session.SetSelectedSongPlaying();
        }
        #endregion

        #region Menu
        public void GoToSong(string partitionFilename)
        {
            SongItem song = DBHandler.GetSong(partitionFilename);
            SetView(LibraryVM);
            LibraryVM.GoToSong(song);
        }

        public bool AddSong(SongItem song)
        {
            if (LibraryVM.AddSong(song))
            {
                SetView(LibraryVM);
                LibraryVM.GoToSong(song);
                return true;
            }

            return false;
        }

        
        #endregion

        public enum EDirection
        {
            Left = -1,
            Right = +1
        }
    }
}
