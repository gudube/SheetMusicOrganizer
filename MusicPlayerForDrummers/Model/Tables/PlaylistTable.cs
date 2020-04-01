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
            return new SqlColumn[] { Name, Locked };
        }

        #region Custom Columns
        public readonly SqlColumn Name = new SqlColumn("Name", EType.TEXT);

        public readonly SqlColumn Locked = new SqlColumn("Locked", EType.BOOL);
        #endregion
    }
}
