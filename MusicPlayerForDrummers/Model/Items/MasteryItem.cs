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

        private string _color;
        public string Color { get => _color; set => SetField(ref _color, value); }

        public MasteryItem(string name, bool locked, string color) : base()
        {
            Name = name;
            IsLocked = locked;
            Color = color;
        }

        public MasteryItem(SqliteDataReader dataReader) : base(dataReader)
        {
            MasteryTable masteryTable = new MasteryTable();
            Name = dataReader.GetString(dataReader.GetOrdinal(masteryTable.Name.Name));
            IsLocked = dataReader.GetBoolean(dataReader.GetOrdinal(masteryTable.IsLocked.Name));
            Color = dataReader.GetString(dataReader.GetOrdinal(masteryTable.Color.Name));
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked, Color };
        }
    }
}
