using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.ComponentModel;
using System.Threading.Tasks;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.ViewModel
{
    public class MainVM : BaseViewModel
    {
        public override string ViewModelName => "MAIN";

        public MainVM() : base(new SessionContext())
        {
            LibraryVM = new LibraryVM(Session);
            PartitionVM = new PartitionVM(Session);
            PlayerVM = new PlayerVM(Session);

            SwitchLibraryViewCommand = new DelegateCommand(x => SetView(LibraryVM));
            SwitchPartitionViewCommand = new DelegateCommand(x => SetView(PartitionVM));

            SetupEvents();
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        #region Child VMs
        public LibraryVM LibraryVM { get; }
        public PartitionVM PartitionVM { get; }
        public PlayerVM PlayerVM { get; }

        private BaseViewModel? _currentViewModel;

        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (SetField(ref _currentViewModel, value))
                    PlayerVM.ShowAdvancedOptions = value == PartitionVM;
            }
        }

        public DelegateCommand SwitchLibraryViewCommand { get; }
        public DelegateCommand SwitchPartitionViewCommand { get; }

        public async Task LoadData()
        {
            DbHandler.InitializeDatabase();

            await LibraryVM.InitializeData();
            
            SetView(LibraryVM);
        }

        private void SetView(BaseViewModel view)
        {
            if (CurrentViewModel == view)
                return;

            if (view == PartitionVM)
            {
                bool foundSongToPlay = LibraryVM.SetSelectedSongPlaying(false);
                if (foundSongToPlay)
                    CurrentViewModel = view;
            }
            else
            {
                CurrentViewModel = view;
            }
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

        #region Common Tasks
        private void SetupEvents()
        {
            PlayerVM.SetSelectedSongPlaying += (o, e) => LibraryVM.SetSelectedSongPlaying(true);
            PlayerVM.PlayNextSong += (o, e) => LibraryVM.SetNextPlayingSong(true);
            PlayerVM.PlayPreviousSong += (o, e) => LibraryVM.SetNextPlayingSong(false);
            PlayerVM.StopPlayingSong += (o, e) => LibraryVM.StopPlayingSong();
        }


        #endregion
    }
}
