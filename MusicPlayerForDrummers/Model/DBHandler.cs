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
                    CreateSongTable(connection);
                    CreatePlaylistSongTable(connection);

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

        private static void CreateIndex(SqliteConnection con, BaseTable table, bool unique, params string[] colNames)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = unique ? "CREATE UNIQUE INDEX " : "CREATE INDEX ";
            cmd.CommandText += string.Join("_", colNames) + "Index ON " + table.TableName + "(" + string.Join(", ", colNames) + ")";
            cmd.ExecuteNonQuery();
        }

        private static bool Exists(SqliteConnection con, BaseTable table, SqlColumn[] columns, object[] values)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM " + table.TableName + " WHERE ";
            string[] conditions = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string paramName = "@" + columns[i].Name;
                conditions[i] = columns[i].Name + " = " + paramName;
                cmd.Parameters.AddWithValue(paramName, values[i]);
            }
            cmd.CommandText += string.Join(" AND ", conditions) + ")";
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        //TODO: Manage errors
        //TODO: Block sql injections, https://stackoverflow.com/questions/33955636/using-executereader-to-return-a-primary-key
        private static void InsertRow(SqliteConnection con, BaseTable table, BaseModelItem row, bool ignoreConflict = false)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = ignoreConflict ? "INSERT OR IGNORE " : "INSERT ";
            cmd.CommandText += "INTO " + table.TableName + "(";
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
        private static void InsertRows(SqliteConnection con, BaseTable table, BaseModelItem[] rows, bool ignoreConflict = false)
        {
            if(rows.Length < 1)
            {
                throw new ArgumentException("Expected at least one row to insert in the database.");
            }

            foreach (BaseModelItem row in rows)
            {
                InsertRow(con, table, row, ignoreConflict);
            }
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string condition, string[] paramNames, object[] paramValues)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formatedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x.Name).ToArray();
            cmd.CommandText = "SELECT " + string.Join(", ", formatedCols);
            cmd.CommandText += " FROM " + itemTable.TableName + " " + condition;
            for(int i=0; i<paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }
            return cmd.ExecuteReader();
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formatedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x.Name).ToArray();
            cmd.CommandText = "SELECT " + string.Join(", ", formatedCols);
            cmd.CommandText += " FROM " + itemTable.TableName + " " + safeCondition;
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
        private static void CreateSongTable(SqliteConnection con)
        {
            SongTable songTable = new SongTable();
            CreateTable(con, songTable);
            CreateIndex(con, songTable, true, songTable.Directory.Name);
        }

        public static SongItem AddSong(string songDir, int masteryID)
        {
            //search for it -> do last insertrow : do all
            SongTable songTable = new SongTable();
            SongItem song;
            string paramName = "@" + songTable.Directory.Name;
            string condition = "WHERE " + songTable.TableName + "." + songTable.Directory.Name + " = " + paramName + " LIMIT 1";
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader itemDR = GetItems(con, songTable, condition, new string[] { paramName }, new object[] { songDir });
                if(itemDR.Read())
                {
                    song = new SongItem(itemDR);
                }
                else
                {
                    song = new SongItem(songDir, masteryID);
                    InsertRow(con, songTable, song);
                }
            }
            return song;
        }

        //TODO: Block after a certain number of songs (limit to like 100 000 songs? need to do a stress test)
        public static SongItem AddSong(string songDir, int masteryID, int playlistID)
        {
            SongItem song = AddSong(songDir, masteryID);
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRow(con, playlistSongTable, new PlaylistSongItem(playlistID, song.ID));
            }
            return song;
        }

        public static List<SongItem> GetAllSongs(params int[] masteryIDs)
        {
            List<SongItem> songs = new List<SongItem>();
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            string psName = playlistSongTable.TableName;

            string condition = "";
            if (masteryIDs.Count() > 0)
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

            InsertRows(con, masteryTable, new BaseModelItem[] { DefaultBeginner, DefaultIntermediate, DefaultAdvanced, DefaultMastered });
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

        private static void CreatePlaylistSongTable(SqliteConnection con)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            CreateTable(con, table);
            CreateIndex(con, table, true, table.PlaylistID.Name, table.SongID.Name);
        }

        public static void AddPlaylistSongLink(int playlistID, int songID)
        {
            PlaylistSongItem psItem = new PlaylistSongItem(playlistID, songID);
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRow(con, new PlaylistSongTable(), psItem, true);
            }
        }
    }
}
