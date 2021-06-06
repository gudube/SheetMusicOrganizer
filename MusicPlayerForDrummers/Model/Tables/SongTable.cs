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
            return new SqlColumn[] { PartitionDirectory, AudioDirectory1, AudioDirectory2, Number, Title, Artist, Album, Genre,
               LengthMD, CodecMD, BitrateMD, Rating, Notes, MasteryId, ScrollStartTime, ScrollEndTime };
        }

        #region Custom Columns
        public readonly SqlColumn PartitionDirectory = new SqlColumn("PartitionDirectory", EType.Text) { Unique = true };
        public readonly SqlColumn AudioDirectory1 = new SqlColumn("AudioDirectory1", EType.Text) { Nullable = true };
        public readonly SqlColumn AudioDirectory2 = new SqlColumn("AudioDirectory2", EType.Text) { Nullable = true };
        
        #region Metadata
        public readonly SqlColumn Number = new SqlColumn("Number", EType.Int) { Nullable = true };
        public readonly SqlColumn Title = new SqlColumn("Title", EType.Text);
        public readonly SqlColumn Artist = new SqlColumn("Artist", EType.Text) { Nullable = true };
        public readonly SqlColumn Album = new SqlColumn("Album", EType.Text) { Nullable = true };
        public readonly SqlColumn Genre = new SqlColumn("Genre", EType.Text) { Nullable = true };
        public readonly SqlColumn LengthMD = new SqlColumn("LengthMD", EType.Text) { Nullable = true };
        public readonly SqlColumn CodecMD = new SqlColumn("CodecMD", EType.Text);
        public readonly SqlColumn BitrateMD = new SqlColumn("BitrateMD", EType.Text) { Nullable = true };
        public readonly SqlColumn Rating = new SqlColumn("Rating", EType.Int) { Nullable = true };
        public readonly SqlColumn Notes = new SqlColumn("Notes", EType.Text, "");
        #endregion

        public readonly SqlColumn MasteryId;
        public readonly SqlColumn ScrollStartTime = new SqlColumn("ScrollStartTime", EType.Int) { Nullable = true };
        public readonly SqlColumn ScrollEndTime = new SqlColumn("ScrollEndTime", EType.Int) { Nullable = true };
        #endregion
    }
}
