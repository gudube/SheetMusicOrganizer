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

        private bool _locked;
        public bool Locked { get => _locked; set => SetField(ref _locked, value); }

        public MasteryItem(string name, bool locked = true) : base()
        {
            Name = name;
            Locked = locked;
        }

        public MasteryItem(SqliteDataReader dataReader) : base(dataReader)
        {
            MasteryTable masteryTable = new MasteryTable();
            Name = dataReader.GetString(dataReader.GetOrdinal(masteryTable.Name.Name));
            Locked = dataReader.GetBoolean(dataReader.GetOrdinal(masteryTable.Locked.Name));
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, Locked };
        }
    }
}
