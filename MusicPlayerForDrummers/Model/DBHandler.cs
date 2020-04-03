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
            using (SqliteConnection connection = new SqliteConnection(_dataSource))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    CreateMasteryTable(connection);
                    CreatePlaylistTable(connection);
                    CreateTable(connection, new SongTable());
                    CreateTable(connection, new PlaylistSongTable());

                    transaction.Commit();
                }
                connection.Close();
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

        private static void CreateTable(SqliteConnection con, BaseTable table)
        {
            DropTable(con, table.TableName);
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + table.TableName + "(";
            cmd.CommandText += string.Join(", ", table.GetAllColumns().Select(x=>x.GetFormatedColumnSchema())) + ")";
            cmd.ExecuteNonQuery();
        }

        //TODO: Manage errors
        //TODO: Block sql injections, https://stackoverflow.com/questions/33955636/using-executereader-to-return-a-primary-key
        private static void InsertRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO " + table.TableName + "(";
            string[] colNames = table.GetCustomColumns().Select(x => x.Name).ToArray();
            cmd.CommandText += string.Join(", ", colNames);
            //string[] paramNames = table.GetCustomColumns().Select(x => "@" + x.Name).ToArray();
            
            string[] preparedNames = new string[colNames.Length]; 
            object[] formatedValues = row.GetCustomValues();
            for (int i = 0; i < formatedValues.Length; i++)
            {
                preparedNames[i] = "@" + colNames[i];
                cmd.Parameters.AddWithValue(preparedNames[i], formatedValues[i]);

            }
            cmd.CommandText += ") VALUES(" + string.Join(',', preparedNames) + ")"; //INSERT INTO car(name, price) VALUES(@name, @price)
            cmd.ExecuteNonQuery();

            cmd.CommandText = "select last_insert_rowid()";
            row.ID = Convert.ToInt32(cmd.ExecuteScalar());
        }

        //TODO: Use to same SqliteCommand instead of recreating?
        private static void InsertRows(SqliteConnection con, BaseTable table, params BaseModelItem[] rows)
        {
            if(rows.Length < 1)
            {
                throw new ArgumentException("Expected at least one row to insert in the database.");
            }

            foreach (BaseModelItem row in rows)
            {
                InsertRow(con, table, row);
            }
            /*SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO " + table.TableName + "(";
            cmd.CommandText += string.Join(", ", table.GetCustomColumns().Select(x=>x.Name));
            cmd.CommandText += ") VALUES";
            string[] rowValues = new string[rows.Length];
            for(int i=0; i<rows.Length; i++)
            {
                rowValues[i] = "(" + string.Join(", ", rows[i].GetFormatedCustomValues()) + ")";
            }
            cmd.CommandText += string.Join(", ", rowValues);
            cmd.ExecuteNonQuery();
            
            //cmd.CommandText = "select last_insert_rowid()";
            //return (Int64)cmd.ExecuteScalar();*/
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string condition)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formatedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x.Name).ToArray();
            cmd.CommandText = "SELECT " + string.Join(", ", formatedCols);
            cmd.CommandText += " FROM " + itemTable.TableName + " " + condition;
            return cmd.ExecuteReader();
        }

        private static SqliteDataReader GetAllItems(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM " + tableName;
            return cmd.ExecuteReader();
        }

        private static void DeleteRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " WHERE ";
            cmd.CommandText += table.ID.Name + " = " + row.ID;
            cmd.ExecuteNonQuery();
        }

        //TODO: Test if it works
        private static void UpdateRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE " + table.TableName + " SET ";
            string[] colNames = table.GetCustomColumns().Select(x => x.Name).ToArray();
            object[] colValues = row.GetCustomValues();
            string[] preparedUpdate = new string[colNames.Length];
            for(int i=0; i<colNames.Length; i++)
            {
                string preparedName = "@" + colNames[i];
                preparedUpdate[i] = colNames[i] + " = " + preparedName;
                cmd.Parameters.AddWithValue(preparedName, colValues[i]);
            }
            cmd.CommandText += string.Join(", ", preparedUpdate);
            cmd.CommandText += " WHERE " + table.ID.Name + " = " + row.ID;
            cmd.ExecuteNonQuery();
        }

        private static bool IsAlreadyInTransaction(SqliteConnection con)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "select @@TRANCOUNT";
            return (Int64) cmd.ExecuteScalar() > 0;
        }
        #endregion

        #region Playlist
        //TODO: Stop the user from entering special chars such as '
        private static void CreatePlaylistTable(SqliteConnection con)
        {
            PlaylistTable playlistTable = new PlaylistTable();
            CreateTable(con, playlistTable);

            PlaylistItem DefaultItem = new PlaylistItem("All Music", true);
            InsertRow(con, playlistTable, DefaultItem);
        }

        //TODO: Make sure dataReader passed by value doesnt impact perf. pass by ref?
        public static List<PlaylistItem> GetAllPlaylists()
        {
            List <PlaylistItem> playlists = new List<PlaylistItem>();

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, new PlaylistTable().TableName);
                while (dataReader.Read())
                {
                    playlists.Add(new PlaylistItem(dataReader));
                }
            }
            return playlists;
        }

        public static void DeletePlaylist(PlaylistItem playlist)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRow(con, new PlaylistTable(), playlist);
            }
        }

        public static void UpdatePlaylist(PlaylistItem playlist)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                UpdateRow(con, new PlaylistTable(), playlist);
            }
        }

        public static void CreateNewPlaylist(PlaylistItem playlist)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRow(con, new PlaylistTable(), playlist);
            }
        }
        #endregion

        #region Song
        //TODO: Block after a certain number of songs (limit to like 100 000 songs? need to do a stress test)
        public static void AddSong(SongItem song)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                //TODO: Find if song already exists, then just update song with ID
                InsertRow(con, new SongTable(), song);
            }
        }

        //TODO: GetSongs(int playlistID)
        //TODO: Make it better join performance (view?)
        /*
         * SELECT Song.ID, Song.Name, ... FROM Song INNER JOIN
         *  (SELECT SongID FROM PlaylistSong WHERE PlaylistSong.PlaylistID = [playlistID] ON PlaylistSong.SongID = Song.SongID)
         *  WHERE Song.MasteryID IN ([masteryIDs[0]], [masteryIDs[1]]...)
         */
        public static List<SongItem> GetSongs(int playlistID, params int[] masteryIDs)
        {
            List<SongItem> songs = new List<SongItem>();
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            string psName = playlistSongTable.TableName;
            string condition = "INNER JOIN (SELECT " + playlistSongTable.SongID.Name + " FROM " + psName
                + " WHERE " + psName + "." + playlistSongTable.PlaylistID.Name + " = " + playlistID + ") ps"
                + " ON ps." + playlistSongTable.SongID.Name + " = " + songTable.TableName + "." + songTable.ID.Name;

            if(masteryIDs.Count() > 0)
                condition += " WHERE " + songTable.TableName + "." + songTable.MasteryID.Name + " IN (" + string.Join(", ", masteryIDs) + ")";

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetItems(con, songTable, condition);
                while (dataReader.Read())
                {
                    songs.Add(new SongItem(dataReader));
                }
            }
            return songs;
        }

        //TODO: For when it's removed of All Music
        public static void RemoveSong(SongItem song)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRow(con, new SongTable(), song);
            }
        }

        //TODO: Would be better to update only the fields necessary?
        public static void UpdateSong(SongItem song)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                UpdateRow(con, new SongTable(), song);
            }
        }
        #endregion

        #region Mastery
        private static void CreateMasteryTable(SqliteConnection con)
        {
            MasteryTable masteryTable = new MasteryTable();
            CreateTable(con, masteryTable);

            MasteryItem DefaultBeginner = new MasteryItem("Beginner", true);
            MasteryItem DefaultIntermediate = new MasteryItem("Intermediate", true);
            MasteryItem DefaultAdvanced = new MasteryItem("Advanced", true);
            MasteryItem DefaultMastered = new MasteryItem("Mastered", true);

            InsertRows(con, masteryTable, DefaultBeginner, DefaultIntermediate, DefaultAdvanced, DefaultMastered);
        }

        public static List<MasteryItem> GetAllMasteryLevels()
        {
            List<MasteryItem> masteryItems = new List<MasteryItem>();

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, new MasteryTable().TableName);
                while (dataReader.Read())
                {
                    masteryItems.Add(new MasteryItem(dataReader));
                }
            }
            return masteryItems;
        }
        #endregion
    }
}
