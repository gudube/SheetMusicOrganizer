using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tools;
using Serilog;

namespace MusicPlayerForDrummers.Model.Items
{
    public abstract class BaseModelItem : BaseNotifyPropertyChanged
    {
        protected BaseModelItem()
        {
            Id = -1;
        }

        protected BaseModelItem(SqliteDataReader dataReader)
        {
            Id = dataReader.GetInt32(0);
        }

        private int _id;
        public int Id { get => _id; set => SetField(ref _id, value); }

        public abstract object[] GetCustomValues();

        protected string GetSafeString(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetString(colNum);
            
            Log.Warning("Null value for String {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return "";
        }

        protected int? GetSafeInt(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetInt32(colNum);

            Log.Warning("Null value for Int {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return null;
        }

        protected uint? GetSafeUInt(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (dR.IsDBNull(colNum))
            {
                Log.Warning("Null value for Uint {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
                return null;
            }

            int safeInt = dR.GetInt32(colNum);
            if (safeInt < 0)
            {
                Log.Warning("Null value for Int {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
                return null;
            }
        
            return (uint) safeInt;
        }

        protected bool GetSafeBool(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetBoolean(colNum);

            Log.Warning("Null value for Bool {colName} for object {objName} with ID {Id}", colName, this.GetType(), Id);
            return false;
        }
        /*protected string GetSqlFormat(string value)
        {
            return "'" + value + "'";
        }
        protected string[] GetSqlFormat(string[] values)
        {
            string[] formated = new string[values.Length];
            for(int i=0; i<values.Length; i++)
            {
                formated[i] = GetSqlFormat(values[i]);
            }
            return formated;
        }
        protected string GetSqlFormat(bool value)
        {
            return value ? "1" : "0";
        }*/
    }
}
