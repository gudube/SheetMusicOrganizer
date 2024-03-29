﻿using Microsoft.Data.Sqlite;
using Serilog;
using SheetMusicOrganizer.Model.Tools;

namespace SheetMusicOrganizer.Model.Items
{
    public abstract class BaseModelItem : BaseNotifyPropertyChanged
    {
        protected BaseModelItem()
        {
            Id = -1;
        }

        // ReSharper disable once InconsistentNaming
        protected int _id;
        public int Id { get => _id; set => SetField(ref _id, value); }

        public abstract object?[] GetCustomValues();

        protected string GetSafeString(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetString(colNum);
            
            //Log.Warning("Null value for String {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return "";
        }

        protected int? GetSafeInt(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetInt32(colNum);

            //Log.Warning("Null value for Int {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return null;
        }

        protected uint? GetSafeUInt(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (dR.IsDBNull(colNum))
            {
                //Log.Warning("Null value for Uint {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
                return null;
            }

            int safeInt = dR.GetInt32(colNum);
            if (safeInt < 0)
            {
                Log.Warning("Tried to convert negative value to a uint for {colName} and object {objName} with ID {Id}", colName, this.GetType(), Id);
                return null;
            }
        
            return (uint) safeInt;
        }


        protected float? GetSafeFloat(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetFloat(colNum);

            //Log.Warning("Null value for Int {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return null;
        }

        protected bool GetSafeBool(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetBoolean(colNum);

            Log.Warning("Null value for Bool {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return false;
        }
    }
}
