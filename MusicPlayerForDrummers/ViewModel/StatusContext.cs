using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using SheetMusicOrganizer.Model.Tools;
using SheetMusicOrganizer.ViewModel.Tools;

namespace SheetMusicOrganizer.ViewModel
{
    public enum LoadingStatus
    {
        SettingSongMastery,
        SelectingPlaylist,
        SortingSongs,
    }

    public enum SavingStatus
    {
        SongMastery,
        SongPlaylist,
        SongsOrder,
    }

    public static class StatusContext
    {

        #region Loading Status
        private static readonly List<string> _loadingMessages = new List<string>(4);

        public static void addLoadingStatus(LoadingStatus status)
        {
            _loadingMessages.Add(getMessage(status));
            CreateLoadingMessage();
        }

        public static void removeLoadingStatus(LoadingStatus status)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                _loadingMessages.Remove(getMessage(status));
                CreateLoadingMessage();
            }, DispatcherPriority.ContextIdle);
        }

        private static string getMessage(LoadingStatus status)
        {
            switch(status)
            {
                case LoadingStatus.SettingSongMastery: return "Setting song(s) mastery";
                case LoadingStatus.SelectingPlaylist: return "Selecting playlist";
                case LoadingStatus.SortingSongs: return "Sorting songs";
                default: return "";
            }
        }

        public static event EventHandler<string>? LoadingMessage;

        private static void CreateLoadingMessage()
        {
            string newMessage = "";
            if (_loadingMessages.Count == 1)
                newMessage = "Loading: " + _loadingMessages[0];
            else if(_loadingMessages.Count > 1)
                newMessage = "Loading...";
            LoadingMessage?.Invoke(null, newMessage);
        }

        #endregion

        #region Saving status
        private static readonly List<string> _savingMessages = new List<string>(2);

        public static void addSavingStatus(SavingStatus status)
        {
            _savingMessages.Add(getMessage(status));
            CreateSavingMessage();
        }

        public static void removeSavingStatus(SavingStatus status)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                _savingMessages.Remove(getMessage(status));
                CreateSavingMessage();
            }, DispatcherPriority.ContextIdle);
        }

        private static string getMessage(SavingStatus status)
        {
            switch (status)
            {
                case SavingStatus.SongMastery: return "Song(s) mastery";
                case SavingStatus.SongPlaylist: return "Song(s) playlist(s)";
                case SavingStatus.SongsOrder: return "Songs order";
                default: return "";
            }
        }

        public static event EventHandler<string>? SavingMessage;

        private static void CreateSavingMessage()
        {
            string newMessage = "";
            if (_savingMessages.Count == 1)
                newMessage = "Saving: " + _savingMessages[0];
            else if (_savingMessages.Count > 1)
                newMessage = "Saving...";
            SavingMessage?.Invoke(null, newMessage);
        }

        #endregion
    }
}
