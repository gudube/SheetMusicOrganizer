using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class MasteryItem : BaseModelItem
    {
        private string _name;
        public string Name { get => _name; set => SetField(ref _name, value); }

        public MasteryItem(string name) : base()
        {
            Name = name;
        }

        public MasteryItem(SqliteDataReader dataReader) : base(dataReader)
        {
            MasteryTable masteryTable = new MasteryTable();
            Name = dataReader.GetString(dataReader.GetOrdinal(masteryTable.Name.Name));
        }

        public override string[] GetFormatedCustomValues()
        {
            return new string[] { GetSqlFormat(Name) };
        }
    }
}
