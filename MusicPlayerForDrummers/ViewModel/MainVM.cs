using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.ComponentModel;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.ViewModel
{
    public class MainVM : BaseViewModel
    {
        public override string ViewModelName => "MAIN";

        public MainVM() : base(new SessionContext())
        {
            DbHandler.InitializeDatabase(true);

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

        public DelegateCommand SwitchLibraryViewCommand { get; }
        public DelegateCommand SwitchPartitionViewCommand { get; }

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
        public void LoadDatabase(string databasePath)
        {
            DbHandler.OpenDatabase(databasePath);
        }

        public void GoToSong(string partitionFilename)
        {
            SongItem song = DbHandler.GetSong(partitionFilename);
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
    }
}
