using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public abstract class BaseTable
    {
        abstract public string TableName { get; }

        public readonly SqlColumn ID;

        //TODO: Or keep an array of all the fields as a field and just return it
        abstract public SqlColumn[] GetCustomColumns();

        public SqlColumn[] GetAllColumns()
        {
            SqlColumn[] customCols = GetCustomColumns();
            SqlColumn[] allCols = new SqlColumn[customCols.Length + 1];
            allCols[0] = ID;
            Array.Copy(customCols, 0, allCols, 1, customCols.Length);
            return allCols;
        }

        protected BaseTable()
        {
            ID = new SqlColumn(TableName + "ID", EType.INT, true);
        }
    }
}
