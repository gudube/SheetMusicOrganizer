namespace MusicPlayerForDrummers.Model.Tables
{
    public class SongTable : BaseTable
    {
        public SongTable() : base("Song")
        {
            MasteryTable masteryTable = new MasteryTable();
            MasteryId = new SqlColumn("MasteryID", EType.Int, masteryTable.TableName, masteryTable.Id.Name, false);
        }

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { PartitionDirectory, AudioDirectory, Number, Title, Artist, Album, Genre,
               LengthMD, CodecMD, BitrateMD, Rating, MasteryId, ScrollStartTime, ScrollEndTime};
        }

        #region Custom Columns
        public readonly SqlColumn PartitionDirectory = new SqlColumn("PartitionDirectory", EType.Text, false, false);

        public readonly SqlColumn AudioDirectory = new SqlColumn("AudioDirectory", EType.Text);

        //TODO: Verify the files metadata is uptodate when playing: compare each field vs store and look at the modified date?
        #region Metadata
        public readonly SqlColumn Number = new SqlColumn("Number", EType.Int);

        public readonly SqlColumn Title = new SqlColumn("Title", EType.Text);

        public readonly SqlColumn Artist = new SqlColumn("Artist", EType.Text);

        public readonly SqlColumn Album = new SqlColumn("Album", EType.Text);

        public readonly SqlColumn Genre = new SqlColumn("Genre", EType.Text);

        public readonly SqlColumn LengthMD = new SqlColumn("LengthMD", EType.Text);

        public readonly SqlColumn CodecMD = new SqlColumn("CodecMD", EType.Text);

        public readonly SqlColumn BitrateMD = new SqlColumn("BitrateMD", EType.Text);

        public readonly SqlColumn Rating = new SqlColumn("Rating", EType.Int);
        #endregion

        //TODO: Make it a Foreign Key field and implement FKs in SqlColumns
        public readonly SqlColumn MasteryId;

        public readonly SqlColumn ScrollStartTime = new SqlColumn("ScrollStartTime", EType.Int);
        public readonly SqlColumn ScrollEndTime = new SqlColumn("ScrollEndTime", EType.Int);
        #endregion
    }
}
