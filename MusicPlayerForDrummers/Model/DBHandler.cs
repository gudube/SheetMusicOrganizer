using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Linq;

namespace MusicPlayerForDrummers.Model
{
    public static class DBHandler
    {

        #region Init
        //TODO: Change Database Dir when exporting .exe
        private const string _databaseFile = @"C:\Users\Guilhem\Documents\Ecole\Ecole\Private\Project\Application.sqlite";
        private const string _dataSource = "Data Source = " + _databaseFile;

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
        #endregion

        #region Generic Query Methods
        private static void DropTable(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS " + tableName;
            cmd.ExecuteNonQuery();
        }

        private static void CreateTable(SqliteConnection con, string tableName, string[] colSchemas)
        {
            
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + tableName + "(" + string.Join(", ", colSchemas) + ")";
            cmd.ExecuteNonQuery();
        }

        //TODO: Manage errors
        //TODO: Block sql injections, https://stackoverflow.com/questions/33955636/using-executereader-to-return-a-primary-key
        //Returns the rowid of the inserted row
        private static Int64 InsertRow(SqliteConnection con, string tableName, string[] colNames, string[] formatedValues)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO " + tableName + "(";
            cmd.CommandText += string.Join(", ", colNames);
            cmd.CommandText += ") VALUES(";
            cmd.CommandText += string.Join(", ", formatedValues) + ")";
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = "select last_insert_rowid()";
            return (Int64)cmd.ExecuteScalar();
        }

        private static SqliteDataReader GetAllItems(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM " + tableName;
            return cmd.ExecuteReader();
        }
        #endregion

        #region Playlist
        //TODO: Stop the user from entering special chars such as '
        private static void CreatePlaylistTable(SqliteConnection con)
        {
            DropTable(con, PlaylistDBModel.TableName);

            CreateTable(con, PlaylistDBModel.TableName, GetColumnsSchemas(PlaylistDBModel.GetAllColumns()));

            PlaylistItem item = new PlaylistItem("All Music", true);
            InsertRow(con, PlaylistDBModel.TableName, GetColumnsNames(PlaylistDBModel.GetCustomColumns()), item.GetFormatedCustomValues());
        }

        //TODO: Make sure dataReader passed by value doesnt impact perf. pass by ref?
        public static List<PlaylistItem> GetAllPlaylists()
        {
            List <PlaylistItem> list = new List<PlaylistItem>();

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, PlaylistDBModel.TableName);
                while (dataReader.Read())
                {
                    list.Add(new PlaylistItem(dataReader));
                }
            }
            return list;
        }
        #endregion

        #region Tools
        private static string[] GetColumnsNames(SqlColumn[] cols)
        {
            return cols.Select(col => col.Name).ToArray();
        }

        private static string[] GetColumnsSchemas(SqlColumn[] cols)
        {
            return cols.Select(col => col.GetFormatedColumnSchema()).ToArray();
        }
        #endregion
    }
}
