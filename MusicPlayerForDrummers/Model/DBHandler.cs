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
                if (transactionStarted)
                    _transaction.Commit();
                con.Close();
            }
        }
        #endregion

        #region Tools
        private static bool StartTransaction(SqliteConnection con)
        {
            if (_transaction != null && _transaction.Connection != null)
                return false;

            _transaction = con.BeginTransaction();
            return true;
        }

        private static SqliteParameter CreateParameter(string name, SqliteType type, object value)
        {
            SqliteParameter param = new SqliteParameter(name, type);
            if (value == null || (value is string strValue && string.IsNullOrWhiteSpace(strValue)))
                param.Value = DBNull.Value;
            else
                param.Value = value;
            return param;
        }
        #endregion

        #region Generic Query Methods
        private static void CreateTable(SqliteConnection con, BaseTable table)
        {
            DropTable(con, table.TableName);
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + table.TableName + "(";
            cmd.CommandText += string.Join(", ", table.GetAllColumns().Select(x => x.GetFormatedColumnSchema())) + ")";
            cmd.ExecuteNonQuery();
        }

        private static void DropTable(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS " + tableName;
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
                string paramName = "@" + columns[i];
                conditions[i] = columns[i] + " = " + paramName;
                cmd.Parameters.Add(CreateParameter(paramName, columns[i].SQLType, values[i]));
            }
            cmd.CommandText += string.Join(" AND ", conditions) + ")";
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        private static SqliteDataReader GetItem(SqliteConnection con, BaseTable itemTable, string condition, params SqliteParameter[] parameters)
        {
            return GetItems(con, itemTable, condition + " LIMIT 1", parameters);
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string condition, params SqliteParameter[] parameters)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formatedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x).ToArray();
            cmd.CommandText = "SELECT " + string.Join(", ", formatedCols);
            cmd.CommandText += " FROM " + itemTable.TableName + " " + condition;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formatedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x).ToArray();
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

        //TODO: Manage errors
        //TODO: Block sql injections, https://stackoverflow.com/questions/33955636/using-executereader-to-return-a-primary-key
        private static void InsertRow(SqliteConnection con, BaseTable table, BaseModelItem row, bool ignoreConflict = false)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = ignoreConflict ? "INSERT OR IGNORE " : "INSERT ";
            cmd.CommandText += "INTO " + table.TableName + "(";

            SqlColumn[] columns = table.GetCustomColumns();
            string[] paramNames = new string[columns.Length];
            object[] formatedValues = row.GetCustomValues();
            for (int i = 0; i < formatedValues.Length; i++)
            {
                paramNames[i] = "@" + columns[i];
                cmd.Parameters.Add(CreateParameter(paramNames[i], columns[i].SQLType, formatedValues[i]));
            }
            cmd.CommandText += string.Join(", ", (object[]) columns);
            cmd.CommandText += ") VALUES(" + string.Join(',', paramNames) + ")"; //INSERT INTO car(name, price) VALUES(@name, @price)
            cmd.ExecuteNonQuery();

            cmd.CommandText = "select last_insert_rowid()";
            row.ID = Convert.ToInt32(cmd.ExecuteScalar());
        }

        //TODO: Use to same SqliteCommand instead of recreating?
        private static void InsertRows(SqliteConnection con, BaseTable table, BaseModelItem[] rows, bool ignoreConflict = false)
        {
            if (rows.Length < 1)
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

        //TODO: Test if it works
        private static void UpdateRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE " + table.TableName + " SET ";

            SqlColumn[] columns = table.GetCustomColumns();
            object[] colValues = row.GetCustomValues();
            string[] preparedUpdate = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string paramName = "@" + columns[i];
                preparedUpdate[i] = columns[i] + " = " + paramName;
                cmd.Parameters.Add(CreateParameter(paramName, columns[i].SQLType, colValues[i]));
            }
            cmd.CommandText += string.Join(", ", preparedUpdate);
            cmd.CommandText += " WHERE " + table.ID + " = " + row.ID;
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRow(SqliteConnection con, BaseTable table, SqlColumn column, object value)
        {
            string paramName = "@" + column;

            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " WHERE ";
            cmd.CommandText += table.ID + " = " + paramName;
            cmd.Parameters.Add(CreateParameter(paramName, column.SQLType, value));
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRows(SqliteConnection con, BaseTable table, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " " + safeCondition;
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRows(SqliteConnection con, BaseTable table, string condition, params SqliteParameter[] parameters)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM " + table.TableName + " " + condition;
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
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
            List<PlaylistItem> playlists = new List<PlaylistItem>();
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
        public static void CreateNewPlaylist(PlaylistItem playlist)
        {
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRow(con, new PlaylistTable(), playlist);
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

        public static void DeletePlaylist(PlaylistItem playlist)
        {
            PlaylistTable table = new PlaylistTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                DeleteRow(con, table, table.ID, playlist.ID);
            }
        }
        #endregion

        #region Song
        private static void CreateSongTable(SqliteConnection con)
        {
            SongTable songTable = new SongTable();
            CreateTable(con, songTable);
            CreateIndex(con, songTable, true, songTable.PartitionDirectory.Name);
        }

        public static bool IsSongExisting(string partitionDir)
        {
            SongTable songTable = new SongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                return Exists(con, songTable, new SqlColumn[] { songTable.PartitionDirectory }, partitionDir);
            }
        }

        public static SongItem GetSong(string partitionDir)
        {
            SongTable songTable = new SongTable();
            SqliteParameter param = CreateParameter("@" + songTable.PartitionDirectory, songTable.PartitionDirectory.SQLType, partitionDir);
            string condition = "WHERE " + songTable.TableName + "." + songTable.PartitionDirectory + " = " + param.ParameterName;
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetItem(con, songTable, condition, param);
                if (dataReader.Read())
                    return new SongItem(dataReader);
            }
            throw new SqliteException("Could not find the song corresponding to : " + partitionDir, 1);
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
            string condition = "INNER JOIN (SELECT " + playlistSongTable.SongID + " FROM " + psName
                + " WHERE " + psName + "." + playlistSongTable.PlaylistID + " = " + playlistID + ") ps"
                + " ON ps." + playlistSongTable.SongID + " = " + songTable.TableName + "." + songTable.ID;

            if (masteryIDs.Count() > 0)
                condition += " WHERE " + songTable.TableName + "." + songTable.MasteryID + " IN (" + string.Join(", ", masteryIDs) + ")";

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetItems(con, songTable, condition);
                while (dataReader.Read())
                    songs.Add(new SongItem(dataReader));
            }
            return songs;
        }

        public static List<SongItem> GetAllSongs(params int[] masteryIDs)
        {
            List<SongItem> songs = new List<SongItem>();
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            string psName = playlistSongTable.TableName;

            string condition = "";
            if (masteryIDs.Count() > 0)
                condition += " WHERE " + songTable.TableName + "." + songTable.MasteryID + " IN (" + string.Join(", ", masteryIDs) + ")";

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                SqliteDataReader dataReader = GetItems(con, songTable, condition);
                while (dataReader.Read())
                    songs.Add(new SongItem(dataReader));
            }
            return songs;
        }

        public static SongItem FindNextSong(int currentSongID, int playlistID, params int[] masteryIDs)
        {
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();

            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT min(" + songTable.TableName + "." + songTable.ID + ")";
                foreach (SqlColumn col in songTable.GetCustomColumns())
                    cmd.CommandText += ", " + songTable.TableName + "." + col;
                cmd.CommandText += " FROM " + songTable.TableName + " UNION " + playlistSongTable.TableName;
                cmd.CommandText += " WHERE " + playlistSongTable.TableName + "." + playlistSongTable.PlaylistID + " == " + playlistID;
                cmd.CommandText += " AND " + songTable.TableName + "." + songTable.MasteryID + " IN " + "(" + string.Join(", ", masteryIDs) + ")";
                cmd.CommandText += " AND " + songTable.TableName + "." + songTable.ID + " > " + currentSongID; //TODO: Replace with ID in playlist from playlistSongTable
                SqliteDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                    return new SongItem(dataReader);
            }
            return null;
        }

        //we suppose the song doesnt already exist!
        //TODO: Block after a certain number of songs (limit to like 100 000 songs? need to do a stress test)
        public static void AddSong(SongItem song, params int[] playlistIDs)
        {
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRow(con, songTable, song);
                foreach(int playlistID in playlistIDs)
                    InsertRow(con, playlistSongTable, new PlaylistSongItem(playlistID, song.ID));
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

        public static bool IsSongInPlaylist(int playlistID, int songID)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                return Exists(con, table, new SqlColumn[] { table.PlaylistID, table.SongID }, playlistID, songID);
            }
        }

        public static void AddSongsToPlaylist(int playlistID, IEnumerable<int> songsIDs)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            PlaylistSongItem[] items = new PlaylistSongItem[songsIDs.Count()];
            for (int i = 0; i < songsIDs.Count(); i++)
            {
                items[i] = new PlaylistSongItem(playlistID, songsIDs.ElementAt(i));
            }
            using (var con = new SqliteConnection(_dataSource))
            {
                con.Open();
                InsertRows(con, table, items, true);
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
        #endregion
    }
}
