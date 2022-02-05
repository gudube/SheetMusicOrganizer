using Serilog;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.Model.Items;
using System.ComponentModel;
using System.IO;

namespace SheetMusicOrganizer.ViewModel.Library
{
    public class ImportLibraryVM : BaseViewModel
    {
        public override string ViewModelName => "LIBRARY";

        public ImportLibraryVM(SessionContext session, LibraryVM libraryVM) : base(session)
        {
            this.libraryVM = libraryVM;
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        private bool _isImporting = true;
        public bool IsImporting { get => _isImporting; set => SetField(ref _isImporting, value); }

        private string _status = "";
        public string Status { get => _status; set => SetField(ref _status, value); }
        private void AddStatus(string newLine)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => Status += ("\r\n" + newLine)));
        }

        private void ActionOnUI(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }

        private LibraryVM libraryVM;
        private bool recursive;
        private bool useAudioMD;
        private bool overwrite;
        public static readonly string[] SupportedFormats = { ".mp3", ".flac", ".wav", ".m4a", ".aiff"};

        public bool AddSong(SongItem song, bool overwrite)
        {
            this.overwrite = overwrite;
            return this.AddSong(song);
        }

        private bool AddSong(SongItem song)
        {
            var existingSong = DbHandler.GetSong(song.PartitionDirectory);
            if (existingSong != null)
            {
                if (overwrite)
                {
                    AddStatus($"Overwriting song... ({song})");
                }
                else
                {
                    AddStatus($"Existing song, looking for differences... ({song})");
                    if (song.IsSameMetadata(existingSong))
                    {
                        AddStatus($"No differences found, skipping. ({song})");
                        return false;
                    }
                    else
                    {
                        song.CopyUserProperties(existingSong);
                    }
                }

                DbHandler.UpdateSong(song);
                if (Session.PlayingSong != null && Session.PlayingSong.Id == song.Id)
                    ActionOnUI(() => libraryVM.StopPlayingSong());

                foreach (var playlist in libraryVM.Playlists)
                {
                    ActionOnUI(() => playlist.UpdateSong(song));
                }
                if (overwrite)
                    AddStatus($"Song overwritten. ({song})");
                else
                    AddStatus($"Some differences found, song updated. ({song})");
                return true;
            }

            AddStatus($"Adding new song... ({song})");

            MasteryItem[] selectedMasteryItems = Session.MasteryLevels.Where(x => x.IsSelected).ToArray();
            song.MasteryId = selectedMasteryItems.Length > 0 ? selectedMasteryItems[0].Id : Session.MasteryLevels[0].Id;

            DbHandler.AddSong(song);
            if(libraryVM.SelectedPlaylist != null)
                ActionOnUI(() => libraryVM.SelectedPlaylist.AddSongs(new[] { song }));

            AddStatus($"New song added. ({song})");
            return true;
        }

        public async Task AddDir(bool byFolder, bool byFilename, string dir, bool recursive, bool useAudioMD, bool overwrite)
        {
            IsImporting = true;
            _status = "";
            await Task.Run(() =>
            {
                AddStatus("Starting import...");
                this.recursive = recursive;
                this.useAudioMD = useAudioMD;
                this.overwrite = overwrite;
                if (byFolder)
                    AddDirByFolder(dir);
                else if (byFilename)
                    AddDirByFilename(dir);
                else
                    AddDirWithoutAudio(dir);
                IsImporting = false;
                AddStatus("Import finished.");
            });

        }

        private void AddDirByFolder(string dir)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    AddDirByFolder(subDir);

            List<string> pdfFiles = new List<string>();
            List<string> audioFiles = new List<string>();
            foreach (string fileDir in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(fileDir);
                if (ext == ".pdf")
                    pdfFiles.Add(fileDir);
                else if (SupportedFormats.Contains(ext))
                    audioFiles.Add(fileDir);
            }

            string audio1 = "";
            string audio2 = "";
            if (audioFiles.Count >= 1)
            {
                audio1 = audioFiles[0];
                if (audioFiles.Count >= 2)
                {
                    if (Path.GetFileNameWithoutExtension(audio1).Length >
                        Path.GetFileNameWithoutExtension(audioFiles[1]).Length)
                    {
                        audio2 = audio1;
                        audio1 = audioFiles[1];
                    }
                    else
                    {
                        audio2 = audioFiles[1];
                    }
                }
            }

            foreach (string pdfFile in pdfFiles)
            {
                AddSong(new SongItem(pdfFile, audio1, audio2, 1, useAudioMD));
            }
        }

        private void AddDirByFilename(string dir)
        {
            List<string> pdfFiles = new List<string>();
            List<string> audioFiles = new List<string>();
            GetAllFilenames(dir, recursive, pdfFiles, audioFiles);
            foreach (string pdfFile in pdfFiles)
            {
                string pdfName = Path.GetFileNameWithoutExtension(pdfFile);
                List<string> audios = audioFiles.FindAll(x => Path.GetFileNameWithoutExtension(x).StartsWith(pdfName));
                string audio1 = "";
                string audio2 = "";
                if (audios.Count >= 1)
                {
                    audio1 = audios[0];
                    audioFiles.Remove(audio1);
                    if (audios.Count >= 2)
                    {
                        audio2 = audios[1];
                        audioFiles.Remove(audio2);
                    }
                }
                if (Path.GetFileNameWithoutExtension(audio1).Length <= Path.GetFileNameWithoutExtension(audio2).Length)
                    AddSong(new SongItem(pdfFile, audio1, audio2, 1, useAudioMD));
                else
                    AddSong(new SongItem(pdfFile, audio2, audio1, 1, useAudioMD));
            }

        }

        private void GetAllFilenames(string dir, bool recursive, List<string> pdfFiles, List<string> audioFiles)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    GetAllFilenames(subDir, true, pdfFiles, audioFiles);

            string[] allFiles = Directory.GetFiles(dir);
            pdfFiles.AddRange(allFiles.Where(x => Path.GetExtension(x) == ".pdf"));
            audioFiles.AddRange(allFiles.Where(x => {
                string ext = Path.GetExtension(x);
                return SupportedFormats.Contains(ext);
            }));
        }

        private void AddDirWithoutAudio(string dir)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    AddDirWithoutAudio(subDir);

            foreach (string fileDir in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(fileDir);
                if (ext == ".pdf")
                    AddSong(new SongItem(fileDir));
            }
        }
    }
}
