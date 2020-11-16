using System.IO;
using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tables;
using Serilog;

namespace MusicPlayerForDrummers.Model.Items
{
    public class SongItem : BaseModelItem
    {
        #region Properties
        private string _partitionDirectory;
        public string PartitionDirectory { get => _partitionDirectory; set => SetField(ref _partitionDirectory, value); }

        private string _audioDirectory1;
        public string AudioDirectory1 { get => _audioDirectory1; set => SetField(ref _audioDirectory1, value); }

        private string _audioDirectory2;
        public string AudioDirectory2 { get => _audioDirectory2; set => SetField(ref _audioDirectory2, value); }

        private uint _number = 0;
        public uint Number { get => _number; set => SetField(ref _number, value); }

        private string _title = "";
        public string Title { get => _title; set => SetField(ref _title, value); }

        private string _artist = "";
        public string Artist { get => _artist; set => SetField(ref _artist, value); }

        private string _album = "";
        public string Album { get => _album; set => SetField(ref _album, value); }

        private string _genre = "";
        public string Genre { get => _genre; set => SetField(ref _genre, value); }

        private string _lengthMD = "00:00";
        public string LengthMD { get => _lengthMD; private set => SetField(ref _lengthMD, value); }

        private string _codecMD = "";
        public string CodecMD { get => _codecMD; private set => SetField(ref _codecMD, value); }

        private string _bitrateMD = "";
        public string BitrateMD { get => _bitrateMD; private set => SetField(ref _bitrateMD, value); }

        private uint _rating = 0;
        public uint Rating { get => _rating; set => SetField(ref _rating, value); }

        private int _masteryId = 0;
        public int MasteryId { get => _masteryId; 
            set
            {
                SetField(ref _masteryId, value);
                OnPropertyChanged(nameof(Mastery));
            }
        }

        private int _scrollStartTime;
        public int ScrollStartTime { get => _scrollStartTime; set { if (SetField(ref _scrollStartTime, value)) DbHandler.UpdateSong(this); } }

        private int _scrollEndTime;
        public int ScrollEndTime { get => _scrollEndTime; set { if (SetField(ref _scrollEndTime, value)) DbHandler.UpdateSong(this); } }
        #endregion

        #region Other Properties
        //useful to know the mastery name from SongItem (e.g. in the SongsGrid)
        public MasteryItem Mastery => DbHandler.MasteryDic[MasteryId];

        private bool _showedAsPlaying = false;
        public bool ShowedAsPlaying { get => _showedAsPlaying; set => SetField(ref _showedAsPlaying, value); }

        private bool _isSelected = false;
        public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
        #endregion

        public SongItem(string partitionDir = "", string audioDirectory1 = "", string audioDirectory2 = "", int masteryId = 0, bool useAudioMD = true) : base()
        {
            _partitionDirectory = partitionDir;
            _audioDirectory1 = audioDirectory1;
            _audioDirectory2 = audioDirectory2;
            _masteryId = masteryId;

            if (useAudioMD)
                ReadAudioMetadata();

            if(string.IsNullOrWhiteSpace(_title))
                _title = Path.GetFileNameWithoutExtension(partitionDir);

            _scrollStartTime = Settings.Default.DefaultScrollStartTime;
            _scrollEndTime = Settings.Default.DefaultScrollEndTime;
        }

        public SongItem(SqliteDataReader dataReader)
        {
            SongTable songTable = new SongTable();
            int? id = GetSafeInt(dataReader, songTable.Id.Name);
            if (!id.HasValue) //todo: should throw an error in most cases where we have a log.error or log.warning and handle it
                Log.Error("Could not find the id when reading a SongItem from the SqliteDataReader.");
            _id = id.GetValueOrDefault(-1);

            _partitionDirectory = GetSafeString(dataReader, songTable.PartitionDirectory.Name);
            _audioDirectory1 = GetSafeString(dataReader, songTable.AudioDirectory1.Name);
            _audioDirectory2 = GetSafeString(dataReader, songTable.AudioDirectory2.Name);
            _number = GetSafeUInt(dataReader, songTable.Number.Name).GetValueOrDefault(0);
            _title = GetSafeString(dataReader, songTable.Title.Name);
            _artist = GetSafeString(dataReader, songTable.Artist.Name);
            _album = GetSafeString(dataReader, songTable.Album.Name);
            _genre = GetSafeString(dataReader, songTable.Genre.Name);
            _lengthMD = GetSafeString(dataReader, songTable.LengthMD.Name);
            _codecMD = GetSafeString(dataReader, songTable.CodecMD.Name);
            _bitrateMD = GetSafeString(dataReader, songTable.BitrateMD.Name);

            uint? rating = GetSafeUInt(dataReader, songTable.Rating.Name);
            if (rating == null || rating > 255)
                Log.Warning("Invalid rating '{Rating}' read from DB for song: {Song}", rating, this);
            _rating = rating.GetValueOrDefault(0);
            _masteryId = GetSafeInt(dataReader, songTable.MasteryId.Name).GetValueOrDefault(0);
            _scrollStartTime = GetSafeInt(dataReader, songTable.ScrollStartTime.Name).GetValueOrDefault(Settings.Default.DefaultScrollStartTime);
            _scrollEndTime = GetSafeInt(dataReader, songTable.ScrollEndTime.Name).GetValueOrDefault(Settings.Default.DefaultScrollEndTime);
        }

        //TODO: Block files that are more than 99:99 minutes? what about really short sounds? test it
        //TODO: Better to add a ReadMetadataSilently that writes private fields instead
        //(for performance and not calling tons of events)
        //Verify ATL .NET vs TagLibSharp (performance, etc.)
        //ATL Rating tag easier to find, but Codec harder to find
        public void ReadAudioMetadata()
        {
            if(!string.IsNullOrWhiteSpace(AudioDirectory1))
                ReadAudioMetadata(AudioDirectory1, true);
            if(!string.IsNullOrWhiteSpace(AudioDirectory2))
                ReadAudioMetadata(AudioDirectory2, true);
        }

        private void ReadAudioMetadata(string audioDir, bool updateExisting)
        {
            TagLib.File tFile = TagLib.File.Create(audioDir);//, TagLib.ReadStyle.PictureLazy);
            if(_number == 0 || updateExisting)
                _number = tFile.Tag.Track;
            if(_title == "" || updateExisting)
                _title = tFile.Tag.Title;
            if (_artist == "" || updateExisting)
            {
                if (tFile.Tag.Performers.Length > 0) //use song artist if exists (or album)
                    _artist = tFile.Tag.JoinedPerformers;
                else
                    _artist = tFile.Tag.JoinedAlbumArtists;
            }
            if(_album == "" || updateExisting)
                _album = tFile.Tag.Album;
            if(_genre == "" || updateExisting)
                _genre = tFile.Tag.JoinedGenres;
            if(_lengthMD == "00:00") //todo: or default value of Duration?
                _lengthMD = tFile.Properties.Duration.ToString(@"mm\:ss"); //format of length: mm:ss
            if (_codecMD == "" || updateExisting)
            {
                string[] mimeSplits = tFile.MimeType.Split('/');
                if (mimeSplits.Length >= 1)
                    _codecMD = mimeSplits[^1];
            }
            if(_bitrateMD == "" || updateExisting)
                _bitrateMD = tFile.Properties.AudioBitrate + " kbps";
            //TODO: Crashes when opening something else than mp3? make the field empty if null
            if (_rating == 0 || updateExisting)
            {
                TagLib.Id3v2.Tag? tagData = (TagLib.Id3v2.Tag?) tFile.GetTag(TagLib.TagTypes.Id3v2);
                if (tagData != null)
                {
                    TagLib.Id3v2.PopularimeterFrame tagInfo = TagLib.Id3v2.PopularimeterFrame.Get(tagData, "Windows Media Player 9 Series", true);
                    byte byteRating = tagInfo.Rating;
                    if (byteRating == 0)
                        _rating = 0;
                    else if (byteRating == 1)
                        _rating = 1;
                    else if (byteRating <= 64)
                        _rating = 2;
                    else if (byteRating <= 128)
                        _rating = 3;
                    else if (byteRating <= 196)
                        _rating = 4;
                    else
                        _rating = 5;
                }
            }
        }

        //TODO: Better way to do it?
        public override object?[] GetCustomValues()
        {
            return new object?[]
            {
                PartitionDirectory, AudioDirectory1, AudioDirectory2, Number, Title, Artist, Album, Genre,
                LengthMD, CodecMD, BitrateMD, Rating, MasteryId, ScrollStartTime, ScrollEndTime
            };
        }

        public override string ToString()
        {
            return string.Join(" - ", Artist, Title);
        }
    }
}
