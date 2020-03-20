using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Data
{
    public class SqlColumn
    {
        public string Name { get; private set; }
        public EType Type { get; private set; }
        public bool PrimaryKey { get; private set; }

        public SqlColumn(string name, EType type, bool primaryKey = false)
        {
            Name = name;
            Type = type;
            PrimaryKey = primaryKey;
        }

        public string GetFormatedColumnSchema()
        {
            string formatedType = "";
            switch (Type)
            {
                case EType.INT: formatedType = "INTEGER"; break;
                case EType.REAL: formatedType = "REAL"; break;
                case EType.TEXT: formatedType = "TEXT"; break;
                case EType.BOOL: formatedType = "INTEGER NOT NULL CHECK (" + Name + " IN (0,1))"; break;
            }
            string formatedPK = PrimaryKey ? "PRIMARY KEY" : "";

            return string.Join(" ", Name, formatedType, formatedPK);
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
