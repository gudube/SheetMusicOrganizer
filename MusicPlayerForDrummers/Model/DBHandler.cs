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
        //private static List<MasteryItem> _masteryItems;
        public static Dictionary<int, MasteryItem> MasteryDic;


        #region Init
        //TODO: Change Database Dir when exporting .exe
        private const string _databaseFile = @"C:\Users\Guilhem\Documents\Ecole\Ecole\Private\Project\Application.sqlite";
        private const string _dataSource = "Data Source = " + _databaseFile;
        private static SqliteTransaction _transaction;

        public static void InitializeDatabase(bool forceResetDatabase = false)
        {
            //TODO: Add verification of database schema (tables have the good format)
            //TODO: Add a button to reset the database with warning that it will reset the software's content
            if (!File.Exists(_databaseFile) || forceResetDatabase)
            {
                File.Create(_databaseFile).Close();
                CreateTables();
            }
            LoadAllMasteryLevels();
        }

        private static void CreateTables()
        {
            using (SqliteConnection con = new SqliteConnection(_dataSource))
            {
                con.Open();

                bool transactionStarted = StartTransaction(con);
                CreateMasteryTable(con);
                CreatePlaylistTable(con);
                CreateSongTable(con);
                CreatePlaylistSongTable(con);

                con.Close();
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

        private static bool Exists(SqliteConnection con, BaseTable table, SqlColumn[] columns, params object[] values)
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

            bool transactionStarted = StartTransaction(con);
            foreach (BaseModelItem row in rows)
            {
                InsertRow(con, table, row, ignoreConflict);
            }
            if (transactionStarted)
                _transaction.Commit();
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

        private static void DeleteRow(SqliteConnection con, BaseTable table, string colName, object value)
        {
            string paramName = "@" + colName;
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " WHERE ";
            cmd.CommandText += table.ID.Name + " = " + paramName;
            cmd.Parameters.AddWithValue(paramName, value);
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRows(SqliteConnection con, BaseTable table, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " " + safeCondition;
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRows(SqliteConnection con, BaseTable table, string condition, string[] paramNames, string[] paramValues)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " " + condition;
            for (int i = 1; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }
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

        private static bool StartTransaction(SqliteConnection con)
        {
            if (_transaction != null && _transaction.Connection != null)
                return false;

            _transaction = con.BeginTransaction();
            return true;
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
            PlaylistTable table = new PlaylistTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRow(con, table, table.ID.Name, playlist.ID);
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
                if (itemDR.Read())
                {
                    song = new SongItem(itemDR);
                    //song.Mastery = _masteryDic[song.MasteryID];
                }
                else
                {
                    song = new SongItem(songDir, masteryID);
                    //song.Mastery = _masteryDic[song.MasteryID];
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
                    var song = new SongItem(dataReader);
                    //song.Mastery = _masteryDic[song.MasteryID];
                    songs.Add(song);
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
                    SongItem song = new SongItem(dataReader);
                    //song.Mastery = _masteryDic[song.MasteryID];
                    songs.Add(song);
                }
            }
            return songs;
        }

        public static void DeleteSongs(int[] songIDs)
        {
            SongTable songTable = new SongTable();
            string safeCondition = "WHERE " + songTable.ID.Name + " IN (" + string.Join(", ", songIDs) + ")";
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRows(con, songTable, safeCondition);
            }
        }

        public static void RemoveSongsFromPlaylist(int playlistID, int[] songIDs)
        {
            PlaylistSongTable psTable = new PlaylistSongTable();
            string safeCondition = "WHERE " + psTable.PlaylistID.Name + " = " + playlistID + " AND "
                + psTable.SongID.Name + " IN(" + string.Join(", ", songIDs) + ")";
            
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRows(con, psTable, safeCondition);
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
        public const int DefaultMasteryID = 0;
        private static void CreateMasteryTable(SqliteConnection con)
        {
            MasteryTable masteryTable = new MasteryTable();
            CreateTable(con, masteryTable);

            MasteryItem DefaultUnset = new MasteryItem("Unset", true, "#F0FDFA");
            DefaultUnset.ID = DefaultMasteryID;
            MasteryItem DefaultBeginner = new MasteryItem("Beginner", true, "#D8F4EF");
            MasteryItem DefaultIntermediate = new MasteryItem("Intermediate", true, "#B7ECEA");
            MasteryItem DefaultAdvanced = new MasteryItem("Advanced", true, "#97DEE7");
            MasteryItem DefaultMastered = new MasteryItem("Mastered", true, "#78C5DC");

            InsertRows(con, masteryTable, new BaseModelItem[] { DefaultUnset, DefaultBeginner, DefaultIntermediate, DefaultAdvanced, DefaultMastered });
        }

        public static List<MasteryItem> GetAllMasteryLevels()
        {
            return MasteryDic.Values.ToList();
            //return _masteryItems;
        }

        private static void LoadAllMasteryLevels()
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
            MasteryDic = masteryItems.ToDictionary(item => item.ID);
            //_masteryItems = masteryItems;
        }

        public static bool IsSongInMastery(int masteryID, int songID)
        {
            SongTable table = new SongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                return Exists(con, table, new SqlColumn[] { table.ID, table.MasteryID }, songID, masteryID);
            }
        }

        public static void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            song.MasteryID = mastery.ID;
            //song.Mastery
            SongTable table = new SongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                UpdateRow(con, table, song);
            }
        }
        public static void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            SongTable table = new SongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                bool transactionStarted = StartTransaction(con);
                foreach (SongItem song in songs)
                {
                    song.MasteryID = mastery.ID;
                    UpdateRow(con, table, song);
                }
                if (transactionStarted)
                    _transaction.Commit();
            }
        }
        #endregion

        #region PlaylistSong
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

        public static void AddPlaylistSongLinks(int playlistID, IEnumerable<int> songsIDs)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            PlaylistSongItem[] items = new PlaylistSongItem[songsIDs.Count()];
            for(int i=0; i<songsIDs.Count(); i++)
            {
                items[i] = new PlaylistSongItem(playlistID, songsIDs.ElementAt(i));
            }
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRows(con, table, items, true);
            }
        }

        public static bool IsSongInPlaylist(int playlistID, int songID)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                return Exists(con, table, new SqlColumn[] { table.PlaylistID, table.SongID }, playlistID, songID);
            }
        }
        #endregion
    }
}
