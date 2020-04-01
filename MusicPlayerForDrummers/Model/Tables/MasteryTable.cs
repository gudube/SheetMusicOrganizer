using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class MasteryTable : BaseTable
    {
        public MasteryTable() : base()
        {
        }

        public override string TableName => "Mastery";

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
