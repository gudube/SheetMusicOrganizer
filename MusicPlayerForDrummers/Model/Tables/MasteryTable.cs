namespace MusicPlayerForDrummers.Model.Tables
{
    public class MasteryTable : BaseTable
    {
        public MasteryTable() : base("Mastery")
        {
        }

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { Name, IsLocked, Color };
        }

        #region Custom Columns
        public readonly SqlColumn Name = new SqlColumn("Name", EType.Text);
        public readonly SqlColumn IsLocked = new SqlColumn("IsLocked", EType.Bool);
        public readonly SqlColumn Color = new SqlColumn("Color", EType.Text);
        #endregion

    }
}
