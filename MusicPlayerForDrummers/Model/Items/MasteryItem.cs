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

        private bool _isLocked;
        public bool IsLocked { get => _isLocked; set => SetField(ref _isLocked, value); }

        public MasteryItem(string name, bool locked = true) : base()
        {
            Name = name;
            IsLocked = locked;
        }

        public MasteryItem(SqliteDataReader dataReader) : base(dataReader)
        {
            MasteryTable masteryTable = new MasteryTable();
            Name = dataReader.GetString(dataReader.GetOrdinal(masteryTable.Name.Name));
            IsLocked = dataReader.GetBoolean(dataReader.GetOrdinal(masteryTable.IsLocked.Name));
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked };
        }
    }
}
