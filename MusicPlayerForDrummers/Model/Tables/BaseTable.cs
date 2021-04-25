using System;
using Serilog;

namespace MusicPlayerForDrummers.Model.Tables
{
    public abstract class BaseTable
    {
        public string TableName { get; }

        public readonly SqlColumn Id;

        public abstract SqlColumn[] GetCustomColumns();

        public SqlColumn[] GetAllColumns()
        {
            SqlColumn[] customCols = GetCustomColumns();
            SqlColumn[] allCols = new SqlColumn[customCols.Length + 1];
            allCols[0] = Id;
            Array.Copy(customCols, 0, allCols, 1, customCols.Length);
            return allCols;
        }

        protected BaseTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                Log.Error("Tried creating a table without a TableName.");
                throw new NullReferenceException("TableName is empty or null.");
            }

            TableName = tableName;
            Id = new SqlColumn($"{tableName}ID", EType.Int) { PrimaryKey = true };
        }

        public override string ToString()
        {
            return TableName;
        }
    }
}
