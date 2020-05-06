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
            MasteryID = new SqlColumn("MasteryID", EType.INT, masteryTable.TableName, masteryTable.ID.Name, false);
        }

        public override string TableName => "Song";

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { PartitionDirectory, AudioDirectory, Number, Title, Artist, Album, Genre,
               LengthMD, CodecMD, BitrateMD, Rating, MasteryID};
        }

        #region Custom Columns
        public readonly SqlColumn PartitionDirectory = new SqlColumn("PartitionDirectory", EType.TEXT, false, false);

        public readonly SqlColumn AudioDirectory = new SqlColumn("AudioDirectory", EType.TEXT);

        //TODO: Verify the files metadata is uptodate when playing: compare each field vs store and look at the modified date?
        #region Metadata
        public readonly SqlColumn Number = new SqlColumn("Number", EType.INT);

        public readonly SqlColumn Title = new SqlColumn("Title", EType.TEXT);

        public readonly SqlColumn Artist = new SqlColumn("Artist", EType.TEXT);

        public readonly SqlColumn Album = new SqlColumn("Album", EType.TEXT);

        public readonly SqlColumn Genre = new SqlColumn("Genre", EType.TEXT);

        public readonly SqlColumn LengthMD = new SqlColumn("LengthMD", EType.TEXT);

        public readonly SqlColumn CodecMD = new SqlColumn("CodecMD", EType.TEXT);

        public readonly SqlColumn BitrateMD = new SqlColumn("BitrateMD", EType.TEXT);

        public readonly SqlColumn Rating = new SqlColumn("Rating", EType.INT);
        #endregion

        //TODO: Make it a Foreign Key field and implement FKs in SqlColumns
        public readonly SqlColumn MasteryID;
        #endregion
    }
}
