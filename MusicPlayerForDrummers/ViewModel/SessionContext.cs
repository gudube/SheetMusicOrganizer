using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.Model.Tools;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class SessionContext : BaseNotifyPropertyChanged
    {
        public SessionContext()
        {

        }

        #region Playlists
        private SmartCollection<BaseModelItem> _playlists = new SmartCollection<BaseModelItem>();
        public SmartCollection<BaseModelItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        public INotifyPropertyChanged _selectedPlaylist;
        public BaseModelItem SelectedPlaylist { get => (BaseModelItem) _selectedPlaylist; set => SetField(ref _selectedPlaylist, value); }
        #endregion

        #region Mastery Levels
        private SmartCollection<MasteryItem> _masteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> MasteryLevels { get => _masteryLevels; set => SetField(ref _masteryLevels, value); }

        private SmartCollection<MasteryItem> _selectedMasteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> SelectedMasteryLevels { get => _selectedMasteryLevels; set => SetField(ref _selectedMasteryLevels, value); }
        #endregion

        #region Songs
        private SmartCollection<SongItem> _songs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> Songs { get => _songs; set => SetField(ref _songs, value); }

        private SmartCollection<SongItem> _selectedSongs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> SelectedSongs { get => _selectedSongs; set => SetField(ref _selectedSongs, value); }

        private INotifyPropertyChanged _playingSong;
        public SongItem PlayingSong { get => (SongItem)_playingSong; set => SetField(ref _playingSong, value); }
        #endregion


    }
}
