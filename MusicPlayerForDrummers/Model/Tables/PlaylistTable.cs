namespace MusicPlayerForDrummers.Model.Tables
{
    //The model contains the table name and its columns
    public class PlaylistTable : BaseTable
    {
        public PlaylistTable() : base("Playlist")
        {
        }

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { Name, IsLocked, IsSmart, SmartDir };
        }

        #region Custom Columns
        public readonly SqlColumn Name = new SqlColumn("Name", EType.Text);

        public readonly SqlColumn IsLocked = new SqlColumn("IsLocked", EType.Bool);
        
        public readonly SqlColumn IsSmart = new SqlColumn("IsSmart", EType.Bool);
        
        public readonly SqlColumn SmartDir = new SqlColumn("SmartDir", EType.Text);
        #endregion
    }
}
