using Microsoft.Data.Sqlite;

namespace MusicPlayerForDrummers.Model
{
    public class SqlColumn
    {
        //TODO: private set; property vs readonly field
        public string Name { get; }
        public SqliteType SqlType => GetSqlType();
        public EType CustomType { get; }
        public bool PrimaryKey { get; set; }
        public bool ForeignKey { get; set; }
        public string FkTableName { get; set; }
        public string FkColumnName { get; set; }
        public bool FkDeleteCascade { get; set; }
        public bool Nullable { get; set; }
        public bool Unique { get; set; }


        public SqlColumn(string name, EType type)
        {
            Name = name;
            CustomType = type;
            PrimaryKey = false;
            ForeignKey = false;
            FkTableName = "";
            FkColumnName = "";
            FkDeleteCascade = false;
            Nullable = false;
            Unique = false;
        }

        public SqlColumn(string name, EType type, string fKTableName, string fKColumnName, bool fkDeleteCascade)
        {
            Name = name;
            CustomType = type;
            PrimaryKey = false;
            ForeignKey = true;
            FkTableName = fKTableName;
            FkColumnName = fKColumnName;
            FkDeleteCascade = fkDeleteCascade;
            Nullable = false;
            Unique = false;
        }

        public string GetFormattedColumnSchema()
        {
            string formattedType = "";
            switch (CustomType)
            {
                case EType.Int: formattedType = "INTEGER"; break;
                case EType.Real: formattedType = "REAL"; break;
                case EType.Text: formattedType = "TEXT"; break;
                case EType.Bool: formattedType = $"INTEGER CHECK ({Name} IN (0,1))"; break;
            }
            string formattedPk = PrimaryKey ? "PRIMARY KEY" : "";
            string formattedFk = ForeignKey ? $"REFERENCES {FkTableName} ({FkColumnName}) ON UPDATE CASCADE ON DELETE CASCADE" : "";
            string fkCascade = FkDeleteCascade ? "ON DELETE CASCADE" : "";
            string notNull = !Nullable || PrimaryKey || ForeignKey ? "NOT NULL" : "";
            string unique = Unique && !PrimaryKey ? "UNIQUE" : "";
            return string.Join(" ", Name, formattedType, formattedPk, formattedFk, fkCascade, notNull, unique);
        }

        private SqliteType GetSqlType()
        {
            switch (CustomType)
            {
                case EType.Bool: return SqliteType.Integer;
                case EType.Int: return SqliteType.Integer;
                case EType.Real: return SqliteType.Real;
                case EType.Text: return SqliteType.Text;
                default: return SqliteType.Blob;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum EType
    {
        Int,
        Real,
        Text,
        Bool
    }
}
