#nullable enable
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
//using System.Windows.Forms;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.Model.Tables;
using Serilog;

namespace MusicPlayerForDrummers.Model
{
    public static class DbHandler
    {
        //private static List<MasteryItem> _masteryItems;
        public static Dictionary<int, MasteryItem> MasteryDic;


        #region Init
        //TODO: put it in settings?
        public static string DefaultDbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Settings.Default.ApplicationName);

        private static SqliteTransaction _transaction;

        public static void InitializeDatabase(bool force = false)
        {
            //TODO: Add verification of database schema (tables have the good format)
            //TODO: Add a button to reset the database with warning that it will reset the software's content
            if(Settings.Default.RecentDBs.Count == 0)
            {
                Log.Information("Opening the default database");
                OpenDefaultDatabase();
            }
            else if (!File.Exists(Settings.Default.RecentDBs[0]))
            {
                //todo: Open warning window that the database couldn't open. Can create a new one or open one
                //for now:
                Log.Error("Could not open the database file. Now opening the default one.");
                OpenDefaultDatabase();
            }

            CreateTables(force);
            LoadAllMasteryLevels();
        }

        private static void OpenDefaultDatabase()
        {
            Directory.CreateDirectory(DefaultDbDir);
            string defaultDbPath = Path.Combine(DefaultDbDir, "Default.sqlite");
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
                //todo: Open warning window that the database couldn't open. Can create a new one or open one
                Log.Error("Could not open the database file. Now opening the default one.");
                OpenDefaultDatabase();
            }

            SaveOpenedDbSettings(databasePath);
            
            Log.Information("Reopening the application with new database path {path}", databasePath);
            //todo: Remove all reference in project to the win forms assembly (unless its there for a reason?)
            //Application.Restart();
            Process.Start(Application.ResourceAssembly.Location);
            // p.WaitForInputIdle();
            Application.Current.MainWindow?.Close();
        }

        private static void SaveOpenedDbSettings(string dBOpenedPath)
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
                    _transaction.Commit();
            }
        }
        #endregion

        #region Tools
        private static bool StartTransaction(SqliteConnection con)
        {
            if (_transaction?.Connection != null)
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

        //TODO: Manage errors
        //TODO: Block sql injections, https://stackoverflow.com/questions/33955636/using-executereader-to-return-a-primary-key
        private static void InsertRow(SqliteConnection con, BaseTable table, BaseModelItem row, bool ignoreConflict = false)
        {
            SqliteCommand cmd = con.CreateCommand();
            SqlColumn[] columns = table.GetCustomColumns();
            string[] paramNames = new string[columns.Length];
            object[] formattedValues = row.GetCustomValues();
            for (int i = 0; i < formattedValues.Length; i++)
            {
                paramNames[i] = "@" + columns[i].Name;
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

        //TODO: Use to same SqliteCommand instead of recreating?
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
                _transaction.Commit();
        }

        //TODO: Test if it works
        private static void UpdateRow(SqliteConnection con, BaseTable table, BaseModelItem row)
        {
            SqliteCommand cmd = con.CreateCommand();
            SqlColumn[] columns = table.GetCustomColumns();
            object[] colValues = row.GetCustomValues();
            string[] preparedUpdate = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string paramName = "@" + columns[i];
                preparedUpdate[i] = $"{columns[i]} = {paramName}";
                cmd.Parameters.Add(CreateParameter(paramName, columns[i].SqlType, colValues[i]));
            }
            cmd.CommandText = $"UPDATE {table.TableName} SET {string.Join(", ", preparedUpdate)} WHERE {table.Id.Name} = {row.Id}";
            cmd.ExecuteNonQuery();
        }

        private static void DeleteRow(SqliteConnection con, BaseTable table, SqlColumn column, object value)
        {
            string paramName = "@" + column;

            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = $"DELETE FROM {table.TableName} WHERE {table.Id.Name} = {paramName}";
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
        //TODO: Stop the user from entering special chars such as '
        private static void CreatePlaylistTable(SqliteConnection con, bool force = false)
        {
            PlaylistTable playlistTable = new PlaylistTable();
            CreateTable(con, playlistTable, force);
            PlaylistItem defaultPlaylist = new PlaylistItem("All Music", true);
            InsertRow(con, playlistTable, defaultPlaylist);

        }

        //TODO: Make sure dataReader passed by value doesn't impact perf. pass by ref?
        public static List<PlaylistItem> GetAllPlaylists()
        {
            List<PlaylistItem> playlists = new List<PlaylistItem>();
            using (SqliteConnection con = CreateConnection())
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
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                InsertRow(con, new PlaylistTable(), playlist);
            }
        }

        public static void UpdatePlaylist(PlaylistItem playlist)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, new PlaylistTable(), playlist);
            }
        }

        public static void DeletePlaylist(PlaylistItem playlist)
        {
            PlaylistTable table = new PlaylistTable();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                DeleteRow(con, table, table.Id, playlist.Id);
            }
        }
        #endregion

        #region Song
        private static void CreateSongTable(SqliteConnection con, bool force = false)
        {
            SongTable songTable = new SongTable();
            CreateTable(con, songTable, force);
            CreateIndex(con, songTable, true, force, songTable.PartitionDirectory.Name);
        }

        public static bool IsSongExisting(string partitionDir)
        {
            SongTable songTable = new SongTable();
            using (var con = CreateConnection())
            {
                con.Open();
                return Exists(con, songTable, new SqlColumn[] { songTable.PartitionDirectory }, partitionDir);
            }
        }

        public static SongItem GetSong(string partitionDir)
        {
            SongTable songTable = new SongTable();
            SqliteParameter param = CreateParameter("@" + songTable.PartitionDirectory, songTable.PartitionDirectory.SqlType, partitionDir);
            string condition = $"WHERE {songTable.TableName}.{songTable.PartitionDirectory.Name} = {param.ParameterName}";
            using (var con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetItem(con, songTable, condition, param);
                if (dataReader.Read())
                    return new SongItem(dataReader);
            }
            throw new SqliteException("Could not find the song corresponding to : " + partitionDir, 1);
        }

        //TODO: Make it better join performance (view?)
        /*
         * SELECT Song.ID, Song.Name, ... FROM Song INNER JOIN
         *  (SELECT SongID FROM PlaylistSong WHERE PlaylistSong.PlaylistID = [playlistID] ON PlaylistSong.SongID = Song.SongID)
         *  WHERE Song.MasteryID IN ([masteryIDs[0]], [masteryIDs[1]]...)
         */
        public static List<SongItem> GetSongs(int playlistId, params int[] masteryIDs)
        {
            List<SongItem> songs = new List<SongItem>();
            List<PlaylistSongItem> psItems = new List<PlaylistSongItem>();
            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();

            using (var con = CreateConnection())
            {
                con.Open();
                SqliteCommand cmd = con.CreateCommand();
                List<string> cols = songTable.GetAllColumns().Select(x => songTable.TableName + "." + x.Name).ToList();
                cols.AddRange(playlistSongTable.GetAllColumns().Select(x => playlistSongTable.TableName + "." + x.Name));
                cmd.CommandText = $"SELECT {string.Join(", ", cols)} FROM {playlistSongTable.TableName} LEFT JOIN {songTable.TableName}"
                //cmd.CommandText += $" INNER JOIN (SELECT * FROM {playlistSongTable.TableName}"
                    + $" ON {playlistSongTable.TableName}.{playlistSongTable.SongId.Name} = {songTable.TableName}.{songTable.Id.Name}"
                    + $" WHERE {playlistSongTable.TableName}.{playlistSongTable.PlaylistId.Name} = {playlistId}";
                SqliteDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    songs.Add(new SongItem(dataReader));
                    psItems.Add(new PlaylistSongItem(dataReader));
                }
            }

            PlaylistSongItem sortingSong = psItems.Find(x => x.PreviousPSId.HasValue == false);
            List<SongItem> sortedSongs = new List<SongItem>(songs.Count);
            while (sortingSong != null)
            {
                SongItem toTransfer = songs.Find(song => song.Id == sortingSong.SongId);
                sortedSongs.Add(toTransfer);
                songs.Remove(toTransfer); //TODO: better perf with or without this line?
                if (sortingSong.NextPSId.HasValue)
                    sortingSong = psItems.Find(x => x.Id == sortingSong.NextPSId);
                else
                    sortingSong = null;
            }

            if (masteryIDs.Any())
                sortedSongs.RemoveAll(x => !masteryIDs.Contains(x.MasteryId));

            return sortedSongs;
        }

        /*public static List<SongItem> GetAllSongs(params int[] masteryIDs)
        {
            List<SongItem> songs = new List<SongItem>();
            SongTable songTable = new SongTable();

            string condition;
            if (masteryIDs.Any())
                condition = $" WHERE {songTable.TableName}.{songTable.MasteryId} IN ({string.Join(", ", masteryIDs)})";
            else
                condition = "";

            using (var con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetItems(con, songTable, condition);
                while (dataReader.Read())
                    songs.Add(new SongItem(dataReader));
            }
            return songs;
        }*/

        private static SongItem FindPlayingSong(bool next, int currentSongId, int playlistId, params int[] masteryIDs)
        {
            if (masteryIDs.Length == 0)
                masteryIDs = MasteryDic.Keys.ToArray();

            SongTable songTable = new SongTable();
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();

            using (var con = CreateConnection())
            {
                con.Open();

                string psIdCondition =
                    $" WHERE {playlistSongTable.TableName}.{playlistSongTable.SongId.Name} = {currentSongId}"
                    + $" AND {playlistSongTable.TableName}.{playlistSongTable.PlaylistId.Name} = {playlistId}";
                SqliteDataReader reader = DbHandler.GetItem(con, playlistSongTable, psIdCondition);
                reader.Read();
                int? nextPSId = new PlaylistSongItem(reader).NextPSId;

                while(nextPSId.HasValue)
                {
                    SqliteCommand cmd = con.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM {songTable.TableName} JOIN {playlistSongTable.TableName}";
                    cmd.CommandText += $" WHERE {playlistSongTable.TableName}.{playlistSongTable.Id.Name} = {nextPSId}";
                    //cmd.CommandText += $" WHERE {playlistSongTable.TableName}.{playlistSongTable.PlaylistId} == {playlistId}";
                    //cmd.CommandText += $" AND {songTable.TableName}.{songTable.MasteryId} IN ({string.Join(", ", masteryIDs)})";
                    //cmd.CommandText += $" AND {songTable.TableName}.{songTable.Id} {(next ? ">" : "<")} {currentSongId}"; //TODO: Replace with ID in playlist from playlistSongTable
                    SqliteDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        SongItem song = new SongItem(dataReader);
                        if (masteryIDs.Contains(song.MasteryId))
                            return song;
                        nextPSId = new PlaylistSongItem(dataReader).NextPSId;
                    }
                }
            }
            return null;
        }

        public static SongItem FindNextSong(int currentSongId, int playlistId, params int[] masteryIDs)
        {
            return FindPlayingSong(true, currentSongId, playlistId, masteryIDs);
        }

        public static SongItem FindPreviousSong(int currentSongId, int playlistId, params int[] masteryIDs)
        {
            return FindPlayingSong(false, currentSongId, playlistId, masteryIDs);
        }

        //we suppose the song doesn't already exist!
        //TODO: Block after a certain number of songs (limit to like 100 000 songs? need to do a stress test)
        public static void AddSong(SongItem song, params int[] playlistIds)
        {
            SongTable songTable = new SongTable();
            using (var con = CreateConnection())
            {
                con.Open();
                InsertRow(con, songTable, song);
            }

            foreach(int playlistId in playlistIds)
            {
                AddSongToPlaylist(playlistId, song.Id);
            }
        }

        //TODO: Would be better to update only the fields necessary?
        public static void UpdateSong(SongItem song)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, new SongTable(), song);
            }
        }

        public static void DeleteSongs(int[] songIDs)
        {
            SongTable songTable = new SongTable();
            string safeCondition = $"WHERE {songTable.Id.Name} IN ( {string.Join(", ", songIDs)})";
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                DeleteRows(con, songTable, safeCondition);
            }
        }
        #endregion

        #region Mastery
        private const int DefaultMasteryId = 0;
        private static void CreateMasteryTable(SqliteConnection con, bool force = false)
        {
            MasteryTable masteryTable = new MasteryTable();
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
            //return _masteryItems;
        }

        private static void LoadAllMasteryLevels()
        {
            List<MasteryItem> masteryItems = new List<MasteryItem>();

            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                SqliteDataReader dataReader = GetAllItems(con, new MasteryTable().TableName);
                while (dataReader.Read())
                {
                    masteryItems.Add(new MasteryItem(dataReader));
                }
            }
            MasteryDic = masteryItems.ToDictionary(item => item.Id);
            //_masteryItems = masteryItems;
        }

        public static bool IsSongInMastery(int masteryId, int songId)
        {
            SongTable table = new SongTable();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                return Exists(con, table, new SqlColumn[] { table.Id, table.MasteryId }, songId, masteryId);
            }
        }

        public static void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            song.MasteryId = mastery.Id;
            //song.Mastery
            SongTable table = new SongTable();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                UpdateRow(con, table, song);
            }
        }
        public static void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            SongTable table = new SongTable();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                bool transactionStarted = StartTransaction(con);
                foreach (SongItem song in songs)
                {
                    song.MasteryId = mastery.Id;
                    UpdateRow(con, table, song);
                }
                if (transactionStarted)
                    _transaction.Commit();
            }
        }
        #endregion

        #region PlaylistSong
        private static void CreatePlaylistSongTable(SqliteConnection con, bool force = false)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            CreateTable(con, table, force);
            CreateIndex(con, table, true, force, table.PlaylistId.Name, table.SongId.Name); //TODO: Is this index usefull?
        }

        public static PlaylistSongItem? GetPlaylistSongItem(SqliteConnection con, int playlistId, int songId)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            string condition = $" WHERE {table.TableName}.{table.SongId.Name} = {songId}" +
                               $" AND {table.TableName}.{table.PlaylistId.Name} = {playlistId}";
            SqliteDataReader reader = GetItem(con, table, condition);
            if (reader.Read())
                return new PlaylistSongItem(reader);
            return null;
        }

        private static PlaylistSongItem? GetLastPlaylistSongItem(SqliteConnection con, int playlistId)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            string condition = $" WHERE {table.TableName}.{table.NextPSId.Name} IS NULL" +
                               $" AND {table.TableName}.{table.PlaylistId.Name} = {playlistId}";
            SqliteDataReader reader = GetItem(con, table, condition);
            if (reader.Read())
                return new PlaylistSongItem(reader);
            return null;
        }

        public static bool IsSongInPlaylist(int playlistId, int songId)
        {
            PlaylistSongTable table = new PlaylistSongTable();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                return Exists(con, table, new SqlColumn[] { table.PlaylistId, table.SongId }, playlistId, songId);
            }
        }

        //We suppose that this is a link at the end.
        public static void AddSongToPlaylist(int playlistId, int songId)
        {
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();

                PlaylistSongItem lastItem = GetLastPlaylistSongItem(con, playlistId);
                int? lastPSId = null;
                if (lastItem != null)
                    lastPSId = lastItem.Id;

                PlaylistSongItem psItem = new PlaylistSongItem(playlistId, songId, lastPSId, null);
                InsertRow(con, new PlaylistSongTable(), psItem);

                if (lastItem != null)
                {
                    lastItem.NextPSId = psItem.Id;
                    UpdateRow(con, new PlaylistSongTable(), lastItem);
                }
            }
        }

        public static void AddSongsToPlaylist(int playlistId, IEnumerable<int> songsIDs)
        {
            PlaylistSongTable psTable = new PlaylistSongTable();
            int[] songIDsArray = songsIDs as int[] ?? songsIDs.ToArray();
            using (SqliteConnection con = CreateConnection())
            {
                con.Open();

                PlaylistSongItem? lastItem = GetLastPlaylistSongItem(con, playlistId);
                int? lastPSId = null;
                if (lastItem != null)
                    lastPSId = lastItem.Id;

                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = $"SELECT MAX({psTable.TableName}.{psTable.Id.Name}) FROM {psTable.TableName}";
                int? nextPSId = ((int?)cmd.ExecuteScalar()).GetValueOrDefault(0);
                nextPSId = nextPSId + 2;

                for (int i = 0; i < songIDsArray.Length; i++)
                {
                    PlaylistSongItem psItem = new PlaylistSongItem(playlistId, songIDsArray[i], lastPSId, nextPSId);
                    InsertRow(con, psTable, psItem);
                    lastPSId = psItem.Id;
                    if (i < songIDsArray.Length - 1)
                        nextPSId = psItem.Id + 2;
                    else
                        nextPSId = null;
                    //Update previous row
                    if (i == 0 && lastItem != null)
                    {
                        lastItem.NextPSId = psItem.Id;
                        UpdateRow(con, psTable, lastItem);
                    }
                }
                
            }
        }

        public static void RemoveSongsFromPlaylist(int playlistId, int[] songIDs)
        {
            PlaylistSongTable psTable = new PlaylistSongTable();

            using (SqliteConnection con = CreateConnection())
            {
                con.Open();
                foreach (int songId in songIDs)
                {
                    PlaylistSongItem? psItem = GetPlaylistSongItem(con, playlistId, songId);
                    if (psItem == null)
                    {
                        Log.Error("Could not find the playlistSongItem associated to the playlistId {playlistId} and the songId {songId}", playlistId, songId);
                        continue;
                    }

                    if (psItem.PreviousPSId.HasValue)
                    {
                        SqliteDataReader reader = GetItem(con, psTable, $" WHERE {psTable.TableName}.{psTable.Id.Name} = {psItem.PreviousPSId}");
                        reader.Read();
                        PlaylistSongItem beforeItem = new PlaylistSongItem(reader);
                        beforeItem.NextPSId = psItem.NextPSId;
                        UpdateRow(con, psTable, beforeItem);
                    }

                    if (psItem.NextPSId.HasValue)
                    {
                        SqliteDataReader reader = GetItem(con, psTable, $" WHERE {psTable.TableName}.{psTable.Id.Name} = {psItem.NextPSId}");
                        reader.Read();
                        PlaylistSongItem afterItem = new PlaylistSongItem(reader);
                        afterItem.PreviousPSId = psItem.PreviousPSId;
                        UpdateRow(con, psTable, afterItem);
                    }


                    string safeCondition = $"WHERE {psTable.TableName}.{psTable.PlaylistId.Name} = {playlistId} AND "
                                           + $"{psTable.TableName}.{psTable.SongId.Name} = {songId}";
                    DeleteRows(con, psTable, safeCondition);
                }
            }
        }
        #endregion
    }
}
