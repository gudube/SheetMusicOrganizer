using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class SongTable : BaseTable
    {
        public SongTable() : base()
        {
            MasteryTable masteryTable = new MasteryTable();
            MasteryID = new SqlColumn("MasteryID", EType.INT, masteryTable.TableName, masteryTable.ID.Name);
        }

        public override string TableName => "Song";

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { Directory, NumberMD, TitleMD, ArtistMD, AlbumMD, GenreMD,
               LengthMD, CodecMD, BitrateMD, RatingMD, MasteryID, PartitionDirectory};
        }

        #region Custom Columns
        public readonly SqlColumn Directory = new SqlColumn("Directory", EType.TEXT);

        //TODO: Verify the files metadata is uptodate when playing: compare each field vs store and look at the modified date?
        #region Metadata
        public readonly SqlColumn NumberMD = new SqlColumn("NumberMD", EType.INT);

        public readonly SqlColumn TitleMD = new SqlColumn("TitleMD", EType.TEXT);

        public readonly SqlColumn ArtistMD = new SqlColumn("ArtistMD", EType.TEXT);

        public readonly SqlColumn AlbumMD = new SqlColumn("AlbumMD", EType.TEXT);

        public readonly SqlColumn GenreMD = new SqlColumn("GenreMD", EType.TEXT);

        public readonly SqlColumn LengthMD = new SqlColumn("LengthMD", EType.TEXT);

        public readonly SqlColumn CodecMD = new SqlColumn("CodecMD", EType.TEXT);

        public readonly SqlColumn BitrateMD = new SqlColumn("BitrateMD", EType.TEXT);

        public readonly SqlColumn RatingMD = new SqlColumn("RatingMD", EType.TEXT);
        #endregion

        #region Custom Song Fields
        //TODO: Make it a Foreign Key field and implement FKs in SqlColumns
        public readonly SqlColumn MasteryID;

        public readonly SqlColumn PartitionDirectory = new SqlColumn("PartitionDirectory", EType.TEXT);
        #endregion
        #endregion
    }
}
