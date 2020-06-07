using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace MusicPlayerForDrummers.Model
{
    public abstract class BaseModelItem : BaseNotifyPropertyChanged
    {
        protected BaseModelItem()
        {
            ID = -1;
        }

        protected BaseModelItem(SqliteDataReader dataReader)
        {
            ID = dataReader.GetInt32(0);
        }
        private int _iD;
        public int ID { get => _iD; set => SetField(ref _iD, value); }

        abstract public object[] GetCustomValues();

        //TODO: Find a way to enforce the BaseModelItem to be the same type
        public void Update(BaseModelItem updatedItem)
        {
            object[] values = GetCustomValues();
            object[] updatedValues = updatedItem.GetCustomValues();
            for(int i = 0; i < values.Count(); i++)
            {
                values[i] = updatedValues[i];
            }
        }

        protected string GetSafeString(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetString(colNum);
            else
                return null;
        }

        protected int? GetSafeInt(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetInt32(colNum);
            else
                return null;
        }

        protected bool GetSafeBool(SqliteDataReader dR, string colName)
        {
            int colNum = dR.GetOrdinal(colName);
            if (!dR.IsDBNull(colNum))
                return dR.GetBoolean(colNum);
            else
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
