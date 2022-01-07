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

        public ImportLibraryVM(SessionContext session) : base(session)
        {
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public PlaylistItem? AllSongsPlaylist;
        public PlaylistItem? SelectedPlaylist;
        private bool recursive;
        private bool useAudioMD;
        private bool overwrite;

        public bool AddSong(SongItem song, bool overwrite)
        {
            this.overwrite = overwrite;
            return this.AddSong(song);
        }

        private bool AddSong(SongItem song)
        {
            if (DbHandler.IsSongExisting(song.PartitionDirectory))
            {
                if(overwrite)
                    DbHandler.DeleteSong(song.PartitionDirectory);
                else
                    return false;
            }

            MasteryItem[] selectedMasteryItems = Session.MasteryLevels.Where(x => x.IsSelected).ToArray();
            song.MasteryId = selectedMasteryItems.Length > 0 ? selectedMasteryItems[0].Id : Session.MasteryLevels[0].Id;

            DbHandler.AddSong(song);

            AllSongsPlaylist?.AddSongs(overwrite, song);

            if (SelectedPlaylist != null)
            {
                if(SelectedPlaylist.Id != AllSongsPlaylist?.Id)
                    SelectedPlaylist.AddSongs(overwrite, song);
            }
            else
            {
                Log.Warning("Trying to add a new song in the database when there are no selected PlaylistItem");
            }

            return true;
        }

        public void AddDir(bool byFolder, bool byFilename, string dir, bool recursive, bool useAudioMD, bool overwrite)
        {
            this.recursive = recursive;
            this.useAudioMD = useAudioMD;
            this.overwrite = overwrite;
            if (byFolder)
                AddDirByFolder(dir);
            else if (byFilename)
                AddDirByFilename(dir);
            else
                AddDirWithoutAudio(dir);
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
                else if (ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".m4a")
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
                AddSong(new SongItem(pdfFile, audio1, audio2, 0, useAudioMD));
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
                    AddSong(new SongItem(pdfFile, audio1, audio2, 0, useAudioMD));
                else
                    AddSong(new SongItem(pdfFile, audio2, audio1, 0, useAudioMD));
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
                return ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".m4a";
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
                    AddSong(new SongItem(fileDir, "", "", 0, false));
            }
        }
    }
}
