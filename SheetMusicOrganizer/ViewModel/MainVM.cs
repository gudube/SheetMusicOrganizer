using System.ComponentModel;
using System.Threading.Tasks;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;
using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace SheetMusicOrganizer.ViewModel
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
            if(e.PropertyName == nameof(Session.PlayingSong))
            {
                if(Session.PlayingSong == null && CurrentViewModel == PartitionVM)
                    SetView(LibraryVM);
                
                PlayerVM.ShowAdvancedOptions = CurrentViewModel == PartitionVM && !String.IsNullOrWhiteSpace(Session.PlayingSong?.AudioDirectory1); 
            }
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
                {
                    if(value == PartitionVM)
                    {
                        PlayerVM.ShowAdvancedOptions = !String.IsNullOrWhiteSpace(Session.PlayingSong?.AudioDirectory1);
                    } else
                    {
                        PlayerVM.ShowAdvancedOptions = false;
                        Session.Player.IsLooping = false;
                    }
                }
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
            try
            {
                DbHandler.OpenDatabase(databasePath);
            } catch(Exception ex)
            {
                GlobalEvents.raiseErrorEvent(ex);
            }
        }

        public void GoToSong(string partitionFilename)
        {
            SongItem? song = DbHandler.GetSong(partitionFilename);
            SetView(LibraryVM);
            if(song != null) LibraryVM.GoToSong(song);
        }

        #endregion

        #region Common Tasks
        private void SetupEvents()
        {
            PlayerVM.SetSelectedSongPlaying += (o, e) => LibraryVM.SetSelectedSongPlaying(true);
            PlayerVM.PlayNextSong += (o, e) => LibraryVM.SetNextPlayingSong(LibraryVM.SongToFind.Next);
            PlayerVM.PlayPreviousSong += (o, e) => LibraryVM.SetNextPlayingSong(LibraryVM.SongToFind.Previous);
            PlayerVM.PlaySameSong += (o, e) => LibraryVM.SetNextPlayingSong(LibraryVM.SongToFind.Same);
            PlayerVM.PlayRandomSong += (o, e) => LibraryVM.SetNextPlayingSong(LibraryVM.SongToFind.Random);
            PlayerVM.StopPlayingSong += (o, e) => LibraryVM.StopPlayingSong();
        }


        #endregion
    }
}
