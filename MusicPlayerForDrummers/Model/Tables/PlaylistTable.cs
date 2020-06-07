using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    //The model contains the table name and its columns
    public class PlaylistTable : BaseTable
    {
        public PlaylistTable() : base()
        {
        }

        public override string TableName => "Playlist";
        
        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { Name, IsLocked, IsSmart, SmartDir, PreviousID };
        }

        #region Custom Columns
        public readonly SqlColumn Name = new SqlColumn("Name", EType.TEXT);

        public readonly SqlColumn IsLocked = new SqlColumn("IsLocked", EType.BOOL);
        
        public readonly SqlColumn IsSmart = new SqlColumn("IsSmart", EType.BOOL);
        
        public readonly SqlColumn SmartDir = new SqlColumn("SmartDir", EType.TEXT);

        public readonly SqlColumn PreviousID = new SqlColumn("PreviousID", EType.INT);
        #endregion
    }
}
