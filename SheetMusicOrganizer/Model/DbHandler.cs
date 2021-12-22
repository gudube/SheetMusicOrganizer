using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using SheetMusicOrganizer;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.Model.Tables;

namespace SheetMusicOrganizer.Model
{
    public static class DbHandler
    {
        //private static List<MasteryItem> _masteryItems;
        public static Dictionary<int, MasteryItem> MasteryDic = new Dictionary<int, MasteryItem>();

        public static readonly MasteryTable masteryTable = new MasteryTable();
        public static readonly PlaylistSongTable playlistSongTable = new PlaylistSongTable();
        public static readonly PlaylistTable playlistTable = new PlaylistTable();
        public static readonly SongTable songTable = new SongTable();


        #region Init
        public static void InitializeDatabase(bool force = false)
        {
            if(Settings.Default.RecentDBs.Count == 0)
            {
                Log.Information("Opening the default database");
                OpenDefaultDatabase();
            }
            else if (!File.Exists(Settings.Default.RecentDBs[0]))
            {
                throw new LibraryFileNotFoundException(Settings.Default.RecentDBs[0]);
            }

            CreateTables(force);
            LoadAllMasteryLevels();
        }

        private static void OpenDefaultDatabase()
        {
            Directory.CreateDirectory(Settings.Default.UserDir);
            string defaultDbPath = Path.Combine(Settings.Default.UserDir, "Default.sqlite");
            SaveOpenedDbSettings(defaultDbPath);
            if (!File.Exists(defaultDbPath))
            {
                File.Create(defaultDbPath).Close();
            }
        }

        public static void OpenDatabase(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException("", databasePath);
            }

            SaveOpenedDbSettings(databasePath);
            
            Log.Information("Reopening the application with new database path {path}", databasePath);
            var currentExecutablePath = Process.GetCurrentProcess().MainModule?.FileName;
            if(currentExecutablePath == null || !currentExecutablePath.EndsWith(".exe"))
            {
                Log.Error("Trying to reopen the application, but the executable path found was invalid: {currentExecutablePath}", currentExecutablePath);
                throw new InvalidOperationException("There was an error when trying to automatically reopen the application with the new library. Please reopen it manually.");
            } else
            {
                Process.Start(currentExecutablePath);
                Application.Current.Shutdown();
            }
        }

        public static void SaveOpenedDbSettings(string dBOpenedPath)
        {
            Log.Information("Saving new opened database {path}", dBOpenedPath);

            if (Settings.Default.RecentDBs.Contains(dBOpenedPath))
            {
                Settings.Default.RecentDBs.Remove(dBOpenedPath);
                Settings.Default.RecentDBs.Insert(0, dBOpenedPath);
            }
            else
            {
                Settings.Default.RecentDBs.Insert(0, dBOpenedPath);
                while(Settings.Default.RecentDBs.Count > 5)
                {
                    Settings.Default.RecentDBs.RemoveAt(Settings.Default.RecentDBs.Count - 1);
                }
            }

            Settings.Default.Save();
        }

        private static SqliteConnection CreateConnection()
        {
            return new SqliteConnection("Data Source=" + Settings.Default.RecentDBs[0]);
        }

        private static void CreateTables(bool force = false)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();

                bool transactionStarted = StartTransaction(con);
                CreateMasteryTable(con, force);
                CreatePlaylistTable(con, force);
                CreateSongTable(con, force);
                CreatePlaylistSongTable(con, force);
                if (transactionStarted)
                    _transaction?.Commit();
                UpdateDBVersion(con);
            }
        }

        private static void UpdateDBVersion(SqliteConnection con)
        {
            int currentVersion = GetDBVersion(con);
            if (currentVersion == 0)
            {
                // example: AlterAddColumn(con, songTable, songTable.Notes);
                SetDBVersion(con, 1);
            }
        }
        #endregion

        #region Tools
        private static SqliteTransaction? _transaction;

        private static bool StartTransaction(SqliteConnection con)
        {
            if (_transaction?.Connection != null)
                return false;

            _transaction = con.BeginTransaction();
            return true;
        }

        private static SqliteParameter CreateParameter(string name, SqliteType type, object? value)
        {
            SqliteParameter param = new SqliteParameter(name, type);
            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value;
            return param;
        }
        #endregion

        #region Generic Query Methods
        private static void CreateTable(SqliteConnection con, BaseTable table, bool force = false)
        {
            if(force)
                DropTable(con, table.TableName);
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE " + (force ? "" : "IF NOT EXISTS ") + table.TableName + "(";
            cmd.CommandText += string.Join(", ", table.GetAllColumns().Select(x => x.GetFormattedColumnSchema())) + ")";
            cmd.ExecuteNonQuery();
        }

        private static void DropTable(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            cmd.ExecuteNonQuery();
        }

        private static int GetDBVersion(SqliteConnection con)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "PRAGMA user_version";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void SetDBVersion(SqliteConnection con, int newVersion)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "PRAGMA user_version = " + newVersion;
            cmd.ExecuteNonQuery();
        }

        private static void AlterAddColumn(SqliteConnection con, BaseTable table, SqlColumn column)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "ALTER TABLE " + table.TableName + " ADD " + column.GetFormattedColumnSchema();
            cmd.ExecuteNonQuery();
        }

        private static void CreateIndex(SqliteConnection con, BaseTable table, bool unique, bool force = false, params string[] colNames)
        {
            if (force)
            {
                SqliteCommand dropCmd = con.CreateCommand();
                dropCmd.CommandText = $"DROP INDEX IF EXISTS {string.Join("_", colNames)}Index";
                dropCmd.ExecuteNonQuery();
            }

            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = (unique ? "CREATE UNIQUE INDEX " : "CREATE INDEX ") + (force ? "" : "IF NOT EXISTS ");
            cmd.CommandText += $"{string.Join("_", colNames)}Index ON {table.TableName}({string.Join(", ", colNames)})";
            cmd.ExecuteNonQuery();
        }

        private static bool Exists(SqliteConnection con, BaseTable table, SqlColumn[] columns, params object[] values)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM {table.TableName} WHERE ";
            string[] conditions = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string paramName = $"@{columns[i]}";
                conditions[i] = $"{columns[i]} = {paramName}";
                cmd.Parameters.Add(CreateParameter(paramName, columns[i].SqlType, values[i]));
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
            string[] formattedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x).ToArray();
            cmd.CommandText = $"SELECT {string.Join(", ", formattedCols)} FROM {itemTable.TableName} {condition}";
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        private static SqliteDataReader GetItems(SqliteConnection con, BaseTable itemTable, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            string[] formattedCols = itemTable.GetAllColumns().Select(x => itemTable.TableName + "." + x).ToArray();
            cmd.CommandText = $"SELECT {string.Join(", ", formattedCols)} FROM {itemTable.TableName} {safeCondition}";
            return cmd.ExecuteReader();
        }

        private static SqliteDataReader GetAllItems(SqliteConnection con, string tableName)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {tableName}";
            return cmd.ExecuteReader();
        }

        private static void InsertRow(SqliteConnection con, BaseTable table, BaseModelItem row, bool ignoreConflict = false)
        {
            SqliteCommand cmd = con.CreateCommand();
            SqlColumn[] columns = table.GetCustomColumns();
            string[] paramNames = new string[columns.Length];
            object?[] formattedValues = row.GetCustomValues();
            for (int i = 0; i < formattedValues.Length; i++)
            {
                paramNames[i] = "@" + columns[i];
                cmd.Parameters.Add(CreateParameter(paramNames[i], columns[i].SqlType, formattedValues[i]));
            }
            cmd.CommandText = $"{(ignoreConflict ? "INSERT OR IGNORE " : "INSERT ")} INTO {table.TableName} (";
            cmd.CommandText += string.Join<SqlColumn>(", ", columns);
            cmd.CommandText += $") VALUES({string.Join(',', paramNames)})";
            cmd.ExecuteNonQuery();

            cmd = con.CreateCommand();
            cmd.CommandText = "select last_insert_rowid()";
            row.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void InsertRows(SqliteConnection con, BaseTable table, BaseModelItem[] rows, bool ignoreConflict = false)
        {
            if (rows.Length < 1)
            {
                Log.Warning("Expected to receive at least one row to insert into table {table}", table);
                return;
            }

            bool transactionStarted = StartTransaction(con);
            foreach (BaseModelItem row in rows)
            {
                InsertRow(con, table, row, ignoreConflict);
            }
            if (transactionStarted)
                _transaction?.Commit();
        }

        private static void UpdateRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            SqlColumn[] columns = table.GetCustomColumns();
            object?[] colValues = row.GetCustomValues();
            string[] preparedUpdate = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string paramName = "@" + columns[i];
                preparedUpdate[i] = $"{columns[i]} = {paramName}";
                cmd.Parameters.Add(CreateParameter(paramName, columns[i].SqlType, colValues[i]));
            }
            cmd.CommandText = $"UPDATE {table.TableName} SET {string.Join(", ", preparedUpdate)} WHERE {table.Id} = {row.Id}";
            cmd.ExecuteNonQuery();
        }

        private static void UpdateRow(SqliteConnection con, BaseTable table, BaseModelItem row, SqlColumn column, object value)
        {
            SqliteCommand cmd = con.CreateCommand();
            string paramName = "@" + column.Name;
            string preparedUpdate = $"{column} = {paramName}";
            cmd.Parameters.Add(CreateParameter(paramName, column.SqlType, value));
            cmd.CommandText = $"UPDATE {table.TableName} SET {preparedUpdate} WHERE {table.Id} = {row.Id}";
            cmd.ExecuteNonQuery();
        }

        private static void UpdateRows(SqliteConnection con, BaseTable table, IEnumerable<BaseModelItem> rows)
        {
            bool transaction = StartTransaction(con);
            foreach (BaseModelItem row in rows)
            {
                UpdateRow(con, table, row);
            }
            if(transaction)
                _transaction?.Commit();
        }

        private static void DeleteRow(SqliteConnection con, BaseTable table, SqlColumn column, object value)
        {
            string paramName = "@" + column;

            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"DELETE FROM {table.TableName} WHERE {table.Id} = {paramName}";
            cmd.Parameters.Add(CreateParameter(paramName, column.SqlType, value));
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRows(SqliteConnection con, BaseTable table, string safeCondition)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"DELETE FROM {table.TableName} {safeCondition}";
            cmd.ExecuteNonQuery();
        }

        private static bool IsEmpty(SqliteConnection con, BaseTable table)
        {
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM {table.TableName}) THEN 0 ELSE 1 END";
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }
        #endregion

        #region Playlist
        private static void CreatePlaylistTable(SqliteConnection con, bool force = false)
        {
            CreateTable(con, playlistTable, force);
            if (IsEmpty(con, playlistTable))
            {
                InsertRow(con, playlistTable, new PlaylistItem("All Music", true));
            }
        }

        public async static Task<List<PlaylistItem>> GetAllPlaylists()
        {
            List<PlaylistItem> playlists = new List<PlaylistItem>();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, playlistTable.TableName);
                while (dataReader.Read())
                {
                    PlaylistItem playlist = new PlaylistItem(dataReader); 
                    playlist.Songs = await GetSongs(playlist.Id);
                    playlist.SortSongs();
                    playlists.Add(playlist);
                }
            }
            return playlists;
        }
        public static void CreateNewPlaylist(PlaylistItem playlist)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                InsertRow(con, playlistTable, playlist);
            }
        }

        public static void UpdatePlaylist(PlaylistItem playlist, SqlColumn field, object value)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, playlistTable, playlist, field, value);
            }
        }

        public static void DeletePlaylist(PlaylistItem playlist)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                DeleteRow(con, playlistTable, playlistTable.Id, playlist.Id);
            }
        }
        #endregion

        #region Song
        private static void CreateSongTable(SqliteConnection con, bool force = false)
        {
            CreateTable(con, songTable, force);
            CreateIndex(con, songTable, true, force, songTable.PartitionDirectory.Name); 
        }

        public static bool IsSongExisting(string partitionDir)
        {
            using (var con = CreateConnection())
            {
                con.Open();
                return Exists(con, songTable, new SqlColumn[] { songTable.PartitionDirectory }, partitionDir);
            }
        }

        /*
        public static SongItem GetSong(int songId)
        {
            SqliteParameter param = CreateParameter("@" + songTable.Id, songTable.Id.SqlType, songId);
            string condition = $"WHERE {songTable.TableName}.{songTable.Id} = {param.ParameterName}";
            using (var con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetItem(con, songTable, condition, param);
                if (dataReader.Read())
                    return new SongItem(dataReader);
            }
            throw new SqliteException("Could not find the song corresponding to the id: " + songId, 1);
        }
        */
        public static SongItem GetSong(string partitionDir)
        {
            SqliteParameter param = CreateParameter("@" + songTable.PartitionDirectory, songTable.PartitionDirectory.SqlType, partitionDir);
            string condition = $"WHERE {songTable.TableName}.{songTable.PartitionDirectory} = {param.ParameterName}";
            using (var con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetItem(con, songTable, condition, param);
                if (dataReader.Read())
                    return new SongItem(dataReader);
            }
            throw new SqliteException("Could not find the song corresponding to : " + partitionDir, 1);
        }

        public static async Task<List<SongItem>> GetSongs(int playlistId)
        {
            return await Task.Run(() =>
            {
                List<SongItem> songs = new List<SongItem>();
                string psName = playlistSongTable.TableName;
                string condition = $"INNER JOIN (SELECT * FROM {psName}"
                                   + $" WHERE {psName}.{playlistSongTable.PlaylistId.Name} = {playlistId}) ps"
                                   + $" ON ps.{playlistSongTable.SongId.Name} = {songTable.TableName}.{songTable.Id.Name}";

                using (var con = CreateConnection())
                {
                    con.Open();
                    SqliteDataReader dataReader = GetItems(con, songTable, condition);
                    while (dataReader.Read())
                        songs.Add(new SongItem(dataReader));
                }

                return songs;
            });
        }

        public static SongItem? FindRandomSong(int playlistId, params int[] masteryIDs)
        {
            SongItem? songFound = null;

            string[] formattedCols = songTable.GetAllColumns().Select(x => "st." + x).ToArray();
            string query = $"SELECT {string.Join(", ", formattedCols)}" +
                           $" FROM {playlistSongTable.TableName} ps1 INNER JOIN {songTable.TableName} st" +
                           $" ON ps1.{playlistSongTable.SongId} = st.{songTable.Id}" +
                           $" WHERE ps1.{playlistSongTable.PlaylistId} = @playlistId";
            if (masteryIDs.Any())
            {
                string[] masteryParams = new string[masteryIDs.Length];
                for (int i = 0; i < masteryIDs.Length; i++)
                    masteryParams[i] = "@masteryId" + i;
                query += $" AND st.{songTable.MasteryId} IN (" + string.Join(", ", masteryParams) + ")";
            }
            query += $" ORDER BY random() LIMIT 1";

            using (var con = CreateConnection())
            {
                SqliteCommand cmd = con.CreateCommand();

                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("playlistId", playlistId);
                for (int i = 0; i < masteryIDs.Length; i++)
                    cmd.Parameters.AddWithValue("masteryId" + i, masteryIDs[i]);

                con.Open();
                SqliteDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    songFound = new SongItem(dataReader);
                }
                else
                {
                    string info = "No random song found for playlistId {playlistId} and masteryIDs {masteryIDs}";
                    Log.Information(info, playlistId, masteryIDs);
                }
                if (dataReader.Read())
                {
                    string info = "More than one random song found for playlistId {playlistId} and masteryIDs {masteryIDs}";
                    Log.Error(info, playlistId, masteryIDs);
                }
            }
            return songFound;
        }

        //we suppose the song doesn't already exist!
        //Adds the song and then adds at the end of the playlists
        public static void AddSong(SongItem song)
        {
            using (var con = CreateConnection())
            {
                con.Open();
                InsertRow(con, songTable, song);
            }
            AddSongsToPlaylist(1, song.Id);
        }

        public static void UpdateSong(SongItem song, SqlColumn field, object value)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, songTable, song, field, value);
            }
        }

        public static void DeleteSongs(int[] songIDs)
        {
            string safeCondition = $"WHERE {songTable.Id.Name} IN ( {string.Join(", ", songIDs)})";
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                DeleteRows(con, songTable, safeCondition);
            }
        }
        #endregion

        #region Mastery
        private const int DefaultMasteryId = 1;
        private static void CreateMasteryTable(SqliteConnection con, bool force = false)
        {
            CreateTable(con, masteryTable, force);

            if (IsEmpty(con, masteryTable))
            {
                MasteryItem defaultUnset = new MasteryItem("Unset", true, "#F0FDFA") { Id = DefaultMasteryId };
                MasteryItem defaultBeginner = new MasteryItem("Beginner", true, "#D8F4EF");
                MasteryItem defaultIntermediate = new MasteryItem("Intermediate", true, "#B7ECEA");
                MasteryItem defaultAdvanced = new MasteryItem("Advanced", true, "#97DEE7");
                MasteryItem defaultMastered = new MasteryItem("Mastered", true, "#78C5DC");

                InsertRows(con, masteryTable, new BaseModelItem[] { defaultUnset, defaultBeginner, defaultIntermediate, defaultAdvanced, defaultMastered });
            }
        }

        public static List<MasteryItem> GetAllMasteryLevels()
        {
            return MasteryDic.Values.ToList();
        }

        private static void LoadAllMasteryLevels()
        {
            List<MasteryItem> masteryItems = new List<MasteryItem>();

            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, masteryTable.TableName);
                while (dataReader.Read())
                {
                    masteryItems.Add(new MasteryItem(dataReader));
                }
            }
            MasteryDic = masteryItems.ToDictionary(item => item.Id);
        }

        public static void SetSongMastery(SongItem song, MasteryItem mastery, object value)
        {
            song.MasteryId = mastery.Id;
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, songTable, song, songTable.MasteryId, value);
            }
        }
        public static void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery, object value)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                bool transactionStarted = StartTransaction(con);
                foreach (SongItem song in songs)
                {
                    song.MasteryId = mastery.Id;
                    UpdateRow(con, songTable, song, songTable.MasteryId, value);
                }
                if (transactionStarted)
                    _transaction?.Commit();
            }
        }
        #endregion

        #region PlaylistSong
        private static void CreatePlaylistSongTable(SqliteConnection con, bool force = false)
        {
            CreateTable(con, playlistSongTable, force);
            CreateIndex(con, playlistSongTable, true, force, playlistSongTable.PlaylistId.Name, playlistSongTable.SongId.Name);
        }

        //public static void AddPlaylistSongLink(int playlistId, int songId)
        //{
        //    using (SqliteConnection con = CreateConnection())
        //    {
        //        con.Open();
        //        InsertRow(con, playlistSongTable, new PlaylistSongItem(playlistId, songId), true);
        //    }
        //}

        //public static bool IsSongInPlaylist(int playlistId, int songId)
        //{
        //    using (SqliteConnection con = CreateConnection())
        //    {
        //        con.Open();
        //        return Exists(con, playlistSongTable, new SqlColumn[] { playlistSongTable.PlaylistId, playlistSongTable.SongId }, playlistId, songId);
        //    }
        //}

        //Adds at the end of the playlist
        public static void AddSongsToPlaylist(int playlistId, params int[] songIDs)
        {
            int[] iDs = songIDs as int[] ?? songIDs.ToArray();
            List<BaseModelItem> items = new List<BaseModelItem>(iDs.Length);
            
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                foreach(int id in iDs)
                {
                    items.Add(new PlaylistSongItem(playlistId, id));
                }
                InsertRows(con, playlistSongTable, items.ToArray(), true);
            }
        }

        public static void RemoveSongsFromPlaylist(int playlistId, IEnumerable<int> songIDs)
        {
            string safeCondition = $"WHERE {playlistSongTable.PlaylistId.Name} = {playlistId} AND "
                + $"{playlistSongTable.SongId.Name} IN( {string.Join(", ", songIDs)})";
            
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                DeleteRows(con, playlistSongTable, safeCondition);
            }
        }

        #endregion
    }
}
