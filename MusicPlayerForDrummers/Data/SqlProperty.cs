using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Data
{
    public class SqlProperty
    {
        public string Name;
        public EType Type;
        public string TypeStr;
        public string PrimaryKey;
        public string Value;

        public SqlProperty(string name, EType type, bool primaryKey = false)
        {
            this.Name = name;
            Type = type;
            switch (type)
            {
                case EType.INT: TypeStr = "INTEGER"; break;
                case EType.REAL: TypeStr = "REAL"; break;
                case EType.TEXT: TypeStr = "TEXT"; break;
                case EType.BOOL: TypeStr = "INTEGER NOT NULL CHECK (" + Name + " IN (0,1))"; break;
            }
            this.PrimaryKey = primaryKey ? "PRIMARY KEY" : "";
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
