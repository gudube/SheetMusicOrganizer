using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tables;
using Serilog;

namespace MusicPlayerForDrummers.Model.Items
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
            _name = name;
            _isLocked = locked;
            _color = color;
        }

        public MasteryItem(SqliteDataReader dataReader)
        {
            MasteryTable masteryTable = new MasteryTable();
            int? id = GetSafeInt(dataReader, masteryTable.Id.Name);
            if (!id.HasValue)
                Log.Error("Could not find the id when reading a MasteryItem from the SqliteDataReader.");
            _id = id.GetValueOrDefault(-1);

            _name = GetSafeString(dataReader, masteryTable.Name.Name);
            _isLocked = GetSafeBool(dataReader, masteryTable.IsLocked.Name);
            _color = GetSafeString(dataReader, masteryTable.Color.Name);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked, Color };
        }
    }
}
