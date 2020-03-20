using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace MusicPlayerForDrummers.Data
{
    public static class DBHandler
    {
        //TODO: Change Database Dir when exporting .exe
        private static string _databaseFile = @"C:\Users\Guilhem\Documents\Ecole\Ecole\Private\Project\Application.sqlite";
        private static string _dataSource = "Data Source = " + _databaseFile;

        public static void InitializeDatabase(bool forceResetDatabase = false)
        {
            //TODO: Add verification of database schema (tables have the good format)
            //TODO: Add a button to reset the database with warning that it will reset the software's content
            if (!File.Exists(_databaseFile) || forceResetDatabase)
            {
                File.Create(_databaseFile).Close();
                CreateTables();
            }
        }

        private static void CreateTables()
        {
            using (var connection = new SqliteConnection(_dataSource))
            {
                connection.Open();
                CreatePlaylistTable(connection);
            }
        }

        #region Generic Methods
        private class SqlProperty
        {
            public string Name;
            public EType Type;
            public string TypeStr;
            public string PrimaryKey;

            public SqlProperty(string name, EType type, bool primaryKey = false)
            {
                this.Name = name;
                Type = type;
                switch (type)
                {
                    case EType.INT: TypeStr = "INTEGER"; break;
                    case EType.REAL: TypeStr = "REAL"; break;
                    case EType.TEXT: TypeStr = "TEXT"; break;
                    case EType.BOOL: TypeStr = "INTEGER NOT NULL CHECK (" + Name +" IN (0,1))"; break;
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

        private static void DropTable(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS " + tableName;
            cmd.ExecuteNonQuery();
        }

        //Creates a TABLE with {tableName}ID as primarykey (rowid). e.g. Playlist -> PlaylistID + properties
        private static void CreateTableWithID(SqliteConnection con, string tableName, SqlProperty[] properties)
        {
            string[] propertyStrings = new string[properties.Length + 1];
            propertyStrings[0] = tableName + "ID INTEGER PRIMARY KEY";
            for (int i=0; i<properties.Length; i++)
            {
                propertyStrings[i+1] = string.Join(" ", properties[i].Name, properties[i].TypeStr, properties[i].PrimaryKey);
            }
            
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + tableName + "(" + string.Join(", ", propertyStrings) + ")";
            cmd.ExecuteNonQuery();
        }

        //TODO: Manage errors
        private static void InsertRow(SqliteConnection con, string tableName, SqlProperty[] properties, params string[] values)
        {
            string[] propertyNames = new string[properties.Length];
            string[] formatedValues = new string[values.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                propertyNames[i] = properties[i].Name;
                formatedValues[i] = properties[i].Type == EType.TEXT ? "'" + values[i] + "'" : values[i];
            }

            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO " + tableName + "(";
            cmd.CommandText += string.Join(", ", propertyNames);
            cmd.CommandText += ") VALUES(";
            cmd.CommandText += string.Join(", ", formatedValues) + ")";
            cmd.ExecuteNonQuery();
        }
        #endregion


        #region Playlist
        private static string _playlistTableName = "Playlist";
        private static SqlProperty _playlistName = new SqlProperty("Name", EType.TEXT);
        private static SqlProperty _playlistLocked = new SqlProperty("Locked", EType.BOOL);
        private static SqlProperty[] _playlistProperties = new SqlProperty[] {_playlistName, _playlistLocked };

        //TODO: Empecher le user d'entrer des chars spéciaux comme '
        private static void CreatePlaylistTable(SqliteConnection con)
        {
            DropTable(con, _playlistTableName);

            CreateTableWithID(con, _playlistTableName, _playlistProperties);

            InsertRow(con, _playlistTableName, _playlistProperties, "All Music", "1");
        }
        #endregion
    }
}
