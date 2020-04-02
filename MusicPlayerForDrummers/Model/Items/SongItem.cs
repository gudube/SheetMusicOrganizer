using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using ATL.AudioData;
using ATL;

namespace MusicPlayerForDrummers.Model
{
    public class SongItem : BaseModelItem
    {
        #region Properties
        private string _directory;
        public string Directory { get => _directory; set => SetField(ref _directory, value); }

        private uint _numberMD;
        public uint NumberMD { get => _numberMD; set => SetField(ref _numberMD, value); }

        private string _titleMD;
        public string TitleMD { get => _titleMD; set => SetField(ref _titleMD, value); }

        private string _artistMD;
        public string ArtistMD { get => _artistMD; set => SetField(ref _artistMD, value); }

        private string _albumMD;
        public string AlbumMD { get => _albumMD; set => SetField(ref _albumMD, value); }

        private string _genreMD;
        public string GenreMD { get => _genreMD; set => SetField(ref _genreMD, value); }

        private string _lengthMD;
        public string LengthMD { get => _lengthMD; set => SetField(ref _lengthMD, value); }

        private string _codecMD;
        public string CodecMD { get => _codecMD; set => SetField(ref _codecMD, value); }

        private string _bitrateMD;
        public string BitrateMD { get => _bitrateMD; set => SetField(ref _bitrateMD, value); }

        private string _ratingMD;
        public string RatingMD { get => _ratingMD; set => SetField(ref _ratingMD, value); }

        private int _masteryID;
        public int MasteryID { get => _masteryID; set => SetField(ref _masteryID, value); }

        private string _partitionDirectory;
        public string PartitionDirectory { get => _partitionDirectory; set => SetField(ref _partitionDirectory, value); }

        #endregion

        public SongItem(string directory, int masteryID) : base()
        {
            Directory = directory;
            //PartitionDirectory = partitionDirectory;
            MasteryID = masteryID;
            ReadMetadata();
        }

        public SongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            SongTable songTable = new SongTable();
            Directory = dataReader.GetString(dataReader.GetOrdinal(songTable.Directory.Name));
            NumberMD = (uint) dataReader.GetInt32(dataReader.GetOrdinal(songTable.NumberMD.Name));
            TitleMD = dataReader.GetString(dataReader.GetOrdinal(songTable.TitleMD.Name));
            ArtistMD = dataReader.GetString(dataReader.GetOrdinal(songTable.ArtistMD.Name));
            AlbumMD = dataReader.GetString(dataReader.GetOrdinal(songTable.AlbumMD.Name));
            GenreMD = dataReader.GetString(dataReader.GetOrdinal(songTable.GenreMD.Name));
            LengthMD = dataReader.GetString(dataReader.GetOrdinal(songTable.LengthMD.Name));
            CodecMD = dataReader.GetString(dataReader.GetOrdinal(songTable.CodecMD.Name));
            BitrateMD = dataReader.GetString(dataReader.GetOrdinal(songTable.BitrateMD.Name));
            RatingMD = dataReader.GetString(dataReader.GetOrdinal(songTable.RatingMD.Name));
            MasteryID = dataReader.GetInt32(dataReader.GetOrdinal(songTable.MasteryID.Name));
            PartitionDirectory = dataReader.GetString(dataReader.GetOrdinal(songTable.PartitionDirectory.Name));
        }

        //TODO: Block files that are more than 99:99 minutes?
        //TODO: Better to add a ReadMetadataSilently that writes private fields instead
        //(for performance and not calling tons of events)
        //Verify ATL .NET vs TagLibSharp (performance, etc.)
        //ATL Rating tag easier to find, but Codec harder to find
        private void ReadMetadata()
        {
            TagLib.File tFile = TagLib.File.Create(Directory);//, TagLib.ReadStyle.PictureLazy);
            NumberMD = tFile.Tag.Track;
            TitleMD = tFile.Tag.Title;
            if (tFile.Tag.Performers.Length > 0) //use song artist if exists (or album)
                ArtistMD = tFile.Tag.JoinedPerformers;
            else
                ArtistMD = tFile.Tag.JoinedAlbumArtists;
            AlbumMD = tFile.Tag.Album;
            GenreMD = tFile.Tag.JoinedGenres;
            //TODO: Make empty if Properties is null
            LengthMD = tFile.Properties.Duration.ToString(@"mm\:ss"); //format of length: mm:ss
            CodecMD = tFile.MimeType;
            BitrateMD = tFile.Properties.AudioBitrate + " kbps";
            //TODO: Crashes when opening something else than mp3, make the field empty if null
            TagLib.Id3v2.Tag tagData = (TagLib.Id3v2.Tag) tFile.GetTag(TagLib.TagTypes.Id3v2);
            TagLib.Id3v2.PopularimeterFrame tagInfo = TagLib.Id3v2.PopularimeterFrame.Get(tagData, "Windows Media Player 9 Series", true);
            RatingMD = tagInfo.Rating.ToString(); //TODO: Transform RatingMD to bytes if it works
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
        public override string[] GetFormatedCustomValues()
        {
            return new string[] { GetSqlFormat(Directory), NumberMD.ToString(),
            GetSqlFormat(TitleMD), GetSqlFormat(ArtistMD), GetSqlFormat(AlbumMD), GetSqlFormat(GenreMD),
            GetSqlFormat(LengthMD), GetSqlFormat(CodecMD), GetSqlFormat(BitrateMD), GetSqlFormat(RatingMD),
            MasteryID.ToString(), GetSqlFormat(PartitionDirectory) };
        }
    }
}
