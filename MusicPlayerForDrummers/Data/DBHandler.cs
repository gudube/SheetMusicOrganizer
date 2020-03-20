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
        

        private static void DropTable(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS " + tableName;
            cmd.ExecuteNonQuery();
        }

        //Creates a TABLE with {tableName}ID as primarykey (rowid). e.g. Playlist -> PlaylistID + properties
        private static void CreateTable(SqliteConnection con, string tableName, SqlProperty[] properties)
        {
            string[] propertyStrings = new string[properties.Length];
            for (int i=0; i<properties.Length; i++)
            {
                propertyStrings[i] = string.Join(" ", properties[i].Name, properties[i].TypeStr, properties[i].PrimaryKey);
            }
            
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + tableName + "(" + string.Join(", ", propertyStrings) + ")";
            cmd.ExecuteNonQuery();
        }

        //TODO: Manage errors
        //Insert a row with the properties and their values from "properties"
        private static void InsertRow(SqliteConnection con, string tableName, SqlProperty[] properties)
        {
            string[] propertyNames = new string[properties.Length];
            string[] formatedValues = new string[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                propertyNames[i] = properties[i].Name;
                formatedValues[i] = properties[i].Type == EType.TEXT ? "'" + properties[i].Value + "'" : properties[i].Value;
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
        //TODO: Empecher le user d'entrer des chars spéciaux comme '
        private static void CreatePlaylistTable(SqliteConnection con)
        {
            PlaylistDBItem AllMusicItem = new PlaylistDBItem("All Music", true);

            DropTable(con, AllMusicItem.TableName);

            CreateTable(con, AllMusicItem.TableName, AllMusicItem.getAllProperties());

            InsertRow(con, AllMusicItem.TableName, AllMusicItem.getCustomProperties());
        }
        #endregion
    }
}
