using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class SongItem : BaseModelItem
    {
        #region Properties
        private string _directory;
        public string Directory { get => _directory; set => SetField(ref _directory, value); }

        private string _numberMD;
        public string NumberMD { get => _numberMD; set => SetField(ref _numberMD, value); }

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

        public SongItem(string directory, string partitionDirectory) : base()
        {
            Directory = directory;
            PartitionDirectory = partitionDirectory;
        }

        public SongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            SongTable songTable = new SongTable();
            Directory = dataReader.GetString(dataReader.GetOrdinal(songTable.Directory.Name));
            NumberMD = dataReader.GetString(dataReader.GetOrdinal(songTable.NumberMD.Name));
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

        //TODO: Better way to do it?
        public override string[] GetFormatedCustomValues()
        {
            string[] firstValues = GetSqlFormat(new string[] { Directory, NumberMD, TitleMD, ArtistMD,
                AlbumMD, GenreMD, LengthMD, CodecMD, BitrateMD, RatingMD });
            string[] allValues = new string[firstValues.Length + 2];
            Array.Copy(firstValues, allValues, firstValues.Length);
            allValues[firstValues.Length] = NumberMD.ToString();
            allValues[firstValues.Length + 1] = GetSqlFormat(PartitionDirectory);
            return allValues;
        }
    }
}
