using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tools;
using System;
using System.Collections.Generic;
using System.Text;

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

        virtual public string[] GetFormatedCustomValues() => null;

        protected string GetSqlFormat(string value)
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
        }
    }
}
