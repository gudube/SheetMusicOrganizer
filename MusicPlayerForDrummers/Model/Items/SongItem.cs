using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using ATL.AudioData;
using ATL;
using System.IO;

namespace MusicPlayerForDrummers.Model
{
    public class SongItem : BaseModelItem
    {
        #region Properties
        private string _partitionDirectory;
        public string PartitionDirectory { get => _partitionDirectory; set => SetField(ref _partitionDirectory, value); }

        private string _audioDirectory;
        public string AudioDirectory { get => _audioDirectory; set => SetField(ref _audioDirectory, value); }

        private uint? _number;
        public uint? Number { get => _number; set => SetField(ref _number, value); }

        private string _title;
        public string Title { get => _title; set => SetField(ref _title, value); }

        private string _artist;
        public string Artist { get => _artist; set => SetField(ref _artist, value); }

        private string _album;
        public string Album { get => _album; set => SetField(ref _album, value); }

        private string _genre;
        public string Genre { get => _genre; set => SetField(ref _genre, value); }

        private string _lengthMD;
        public string LengthMD { get => _lengthMD; private set => SetField(ref _lengthMD, value); }

        private string _codecMD;
        public string CodecMD { get => _codecMD; private set => SetField(ref _codecMD, value); }

        private string _bitrateMD;
        public string BitrateMD { get => _bitrateMD; private set => SetField(ref _bitrateMD, value); }

        private uint _rating;
        public uint Rating { get => _rating; set => SetField(ref _rating, value); }

        private int _masteryID;
        public int MasteryID { get => _masteryID; 
            set
            {
                SetField(ref _masteryID, value);
                OnPropertyChanged("Mastery");
            }
        }

        private int _scrollStartTime;
        public int ScrollStartTime { get => _scrollStartTime; set { if (SetField(ref _scrollStartTime, value)) DBHandler.UpdateSong(this); } }

        private int _scrollEndTime;
        public int ScrollEndTime { get => _scrollEndTime; set { if (SetField(ref _scrollEndTime, value)) DBHandler.UpdateSong(this); } }
        #endregion

        #region Other Properties
        //useful to know the mastery name from SongItem (e.g. in the SongsGrid)
        public MasteryItem Mastery { get => DBHandler.MasteryDic[MasteryID]; }
        #endregion

        public SongItem(string partitionDir = "", string audioDirectory = "", int masteryID = 0, bool useAudioMD = true) : base()
        {
            _partitionDirectory = partitionDir;
            _audioDirectory = audioDirectory;
            _masteryID = masteryID;

            if (useAudioMD && !string.IsNullOrWhiteSpace(audioDirectory))
            {
                ReadAudioMetadata();
            }
            else
            {
                _title = Path.GetFileNameWithoutExtension(partitionDir);
            }

            _scrollStartTime = 10;
            _scrollEndTime = 10;
        }

        public SongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            SongTable songTable = new SongTable();
            _partitionDirectory = GetSafeString(dataReader, songTable.PartitionDirectory.Name);
            _audioDirectory = GetSafeString(dataReader, songTable.AudioDirectory.Name);
            _number = (uint?) GetSafeInt(dataReader, songTable.Number.Name);
            _title = GetSafeString(dataReader, songTable.Title.Name);
            _artist = GetSafeString(dataReader, songTable.Artist.Name);
            _album = GetSafeString(dataReader, songTable.Album.Name);
            _genre = GetSafeString(dataReader, songTable.Genre.Name);
            _lengthMD = GetSafeString(dataReader, songTable.LengthMD.Name);
            _codecMD = GetSafeString(dataReader, songTable.CodecMD.Name);
            _bitrateMD = GetSafeString(dataReader, songTable.BitrateMD.Name);
            _rating = (uint) GetSafeInt(dataReader, songTable.Rating.Name);
            _masteryID = (int) GetSafeInt(dataReader, songTable.MasteryID.Name);
            _scrollStartTime = (int) GetSafeInt(dataReader, songTable.ScrollStartTime.Name);
            _scrollEndTime = (int) GetSafeInt(dataReader, songTable.ScrollEndTime.Name);
        }

        //TODO: Block files that are more than 99:99 minutes?
        //TODO: Better to add a ReadMetadataSilently that writes private fields instead
        //(for performance and not calling tons of events)
        //Verify ATL .NET vs TagLibSharp (performance, etc.)
        //ATL Rating tag easier to find, but Codec harder to find
        public void ReadAudioMetadata()
        {
            TagLib.File tFile = TagLib.File.Create(AudioDirectory);//, TagLib.ReadStyle.PictureLazy);
            _number = tFile.Tag.Track;
            _title = tFile.Tag.Title;
            if (tFile.Tag.Performers.Length > 0) //use song artist if exists (or album)
                _artist = tFile.Tag.JoinedPerformers;
            else
                _artist = tFile.Tag.JoinedAlbumArtists;
            _album = tFile.Tag.Album;
            _genre = tFile.Tag.JoinedGenres;
            //TODO: Make empty if Properties is null
            _lengthMD = tFile.Properties.Duration.ToString(@"mm\:ss"); //format of length: mm:ss
            string[] mimeSplits = tFile.MimeType.Split('/');
            _codecMD = mimeSplits[mimeSplits.Length - 1];
            _bitrateMD = tFile.Properties.AudioBitrate + " kbps";
            //TODO: Crashes when opening something else than mp3, make the field empty if null
            TagLib.Id3v2.Tag tagData = (TagLib.Id3v2.Tag) tFile.GetTag(TagLib.TagTypes.Id3v2);
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

            /*Track song = new Track(Directory);
            NumberMD = song.TrackNumber;
            TitleMD = song.Title;
            
            if (!string.IsNullOrWhiteSpace(song.Artist))
                ArtistMD = song.Artist;
            else if (!string.IsNullOrWhiteSpace(song.AlbumArtist))
                ArtistMD = song.AlbumArtist;
            else
                ArtistMD = song.OriginalArtist;

            if (!string.IsNullOrWhiteSpace(song.Album))
                AlbumMD = song.Album;
            else
                AlbumMD = song.OriginalAlbum;

            GenreMD = song.Genre;
            TimeSpan time = TimeSpan.FromSeconds(song.Duration);
            LengthMD = time.ToString(@"mm\:ss");*/
        }

        //TODO: Better way to do it?
        public override object[] GetCustomValues()
        {
            return new object[] { PartitionDirectory, AudioDirectory, Number, Title, Artist, Album, Genre,
            LengthMD, CodecMD, BitrateMD, Rating, MasteryID, ScrollStartTime, ScrollEndTime };
        }

        public override string ToString()
        {
            return string.Join(" - ", Artist, Title);
        }
    }
}
