using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class SqlColumn
    {
        //TODO: private set; property vs readonly field
        public string Name { get; private set; }
        public EType Type { get; private set; }
        public bool PrimaryKey { get; private set; }
        public bool ForeignKey { get; private set; }
        public string FKTableName { get; private set; }
        public string FKColumnName { get; private set; }

        public SqlColumn(string name, EType type, bool primaryKey = false)
        {
            Name = name;
            Type = type;
            PrimaryKey = primaryKey;
            ForeignKey = false;
        }

        public SqlColumn(string name, EType type, string fKTableName, string fKColumnName)
        {
            Name = name;
            Type = type;
            PrimaryKey = false;
            ForeignKey = true;
            FKTableName = fKTableName;
            FKColumnName = fKColumnName;
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
            string formatedPK = PrimaryKey ? "PRIMARY KEY NOT NULL" : "";
            string formatedFK = ForeignKey? ("REFERENCES " + FKTableName + "(" + FKColumnName + ")") : "";
            return string.Join(" ", Name, formatedType, formatedPK, formatedFK);
        }

        /*public string GetFormatedForeignKey()
        {
            if(!ForeignKey)
            {
                return "";
            }

            return "FOREIGN KEY(" + Name + ") REFERENCES " + FKTableName + " (" + FKColumnName + ")";
        }*/
    }

    public enum EType
    {
        INT,
        REAL,
        TEXT,
        BOOL
    }
}
