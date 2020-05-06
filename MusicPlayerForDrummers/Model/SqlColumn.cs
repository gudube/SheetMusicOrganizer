using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class SqlColumn
    {
        //TODO: private set; property vs readonly field
        public string Name { get; private set; }
        public SqliteType SQLType {
            get { return GetSQLType(); }
        }
        public EType CustomType { get; private set; }
        public bool PrimaryKey { get; private set; }
        public bool ForeignKey { get; private set; }
        public string FKTableName { get; private set; }
        public string FKColumnName { get; private set; }
        public bool FKDeleteCascade { get; private set; }
        public bool Nullable { get; private set; }


        public SqlColumn(string name, EType type, bool primaryKey = false, bool nullable = true)
        {
            Name = name;
            CustomType = type;
            PrimaryKey = primaryKey;
            ForeignKey = false;
            FKDeleteCascade = false;
            Nullable = nullable && !primaryKey;
        }

        public SqlColumn(string name, EType type, string fKTableName, string fKColumnName, bool fkDeleteCascade)
        {
            Name = name;
            CustomType = type;
            PrimaryKey = false;
            ForeignKey = true;
            FKTableName = fKTableName;
            FKColumnName = fKColumnName;
            FKDeleteCascade = fkDeleteCascade;
        }

        public string GetFormatedColumnSchema()
        {
            string formatedType = "";
            switch (CustomType)
            {
                case EType.INT: formatedType = "INTEGER"; break;
                case EType.REAL: formatedType = "REAL"; break;
                case EType.TEXT: formatedType = "TEXT"; break;
                case EType.BOOL: formatedType = "INTEGER CHECK (" + Name + " IN (0,1))"; break;
            }
            string formatedPK = PrimaryKey ? "PRIMARY KEY" : "";
            string formatedFK = ForeignKey? ("REFERENCES " + FKTableName + "(" + FKColumnName + ")") : "";
            string fkCascade = FKDeleteCascade ? "ON DELETE CASCADE" : "";
            string notNull = Nullable ? "" : "NOT NULL";
            return string.Join(" ", Name, formatedType, formatedPK, formatedFK, fkCascade, notNull);
        }

        private SqliteType GetSQLType()
        {
            switch (CustomType)
            {
                case EType.BOOL: return SqliteType.Integer;
                case EType.INT: return SqliteType.Integer;
                case EType.REAL: return SqliteType.Real;
                case EType.TEXT: return SqliteType.Text;
                default: return SqliteType.Blob;
            }
        }
    }

    public enum EType
    {
        INT,
        REAL,
        TEXT,
        BOOL
    }
}
