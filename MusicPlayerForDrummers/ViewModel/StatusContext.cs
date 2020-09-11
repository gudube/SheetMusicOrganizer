using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;
using MusicPlayerForDrummers.Model.Tools;

namespace MusicPlayerForDrummers.ViewModel
{
    public class StatusContext: BaseNotifyPropertyChanged
    {
        #region Loading Status bools
        private bool _settingSongMastery;
        public bool SettingSongMastery { set => SetLoadingStatus(ref _settingSongMastery, value, "Setting song(s) mastery"); }

        private bool _sortingSongs;
        public bool SortingSongs { set => SetLoadingStatus(ref _sortingSongs, value, "Sorting songs"); }

        private bool _selectingPlaylist;
        public bool SelectingPlaylist { set => SetLoadingStatus(ref _selectingPlaylist, value, "Selecting playlist"); }
        #endregion

        #region Saving Status bools
        private bool _savingSongMastery;
        public bool SavingSongMastery { set => SetSavingStatus(ref _savingSongMastery, value, "Song(s) mastery"); }

        private bool _savingSongPlaylist;
        public bool SavingSongPlaylist { set => SetSavingStatus(ref _savingSongPlaylist, value, "Song(s) playlist(s)"); }

        private bool _savingSongOrder;
        public bool SavingSongOrder { set => SetSavingStatus(ref _savingSongOrder, value, "Songs order"); }
        #endregion

        #region Saving Message
        private string _savingMsg = "";
        public string SavingMsg
        {
            get => _savingMsg;
            private set => SetField(ref _savingMsg, value);
        }

        private readonly List<string> _savingMessages = new List<string>(2);

        private void CreateSavingMessage()
        {
            if (_savingMessages.Count == 0)
                SavingMsg = "";

            if (_savingMessages.Count == 1)
                SavingMsg = "Saving: " + _savingMessages[0];

            SavingMsg = "Saving...";
        }

        private void SetSavingStatus(ref bool field, bool value, string message)
        {
            if (field != value)
            {
                field = value;
                if (field)
                    _savingMessages.Add(message);
                else
                    _savingMessages.Remove(message);
                CreateSavingMessage();
            }
        }
        #endregion

        #region Loading Message
        private string _loadingMsg = "";
        public string LoadingMsg
        {
            get => _loadingMsg;
            private set => SetField(ref _loadingMsg, value);
        }

        private readonly List<string> _loadingMessages = new List<string>(2);

        private void CreateLoadingMessage()
        {
            if (_loadingMessages.Count == 0)
                LoadingMsg = "";
            else if (_loadingMessages.Count == 1)
                LoadingMsg = "Loading: " + _loadingMessages[0];
            else 
                LoadingMsg = "Loading...";
        }

        private void SetLoadingStatus(ref bool field, bool value, string message)
        {
            if (field != value)
            {
                field = value;
                if (field)
                {
                    _loadingMessages.Add(message);
                    CreateLoadingMessage();
                    App.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() => { _loadingMessages.Remove(message); }, DispatcherPriority.ContextIdle);
                    CreateLoadingMessage();
                }

            }
        }
        #endregion
    }
}
