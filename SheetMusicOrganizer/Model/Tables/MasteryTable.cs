namespace SheetMusicOrganizer.Model.Tables
{
    public class MasteryTable : BaseTable
    {
        public MasteryTable() : base("Mastery")
        {
        }

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { Name, IsLocked };
        }

        #region Custom Columns
        public readonly SqlColumn Name = new SqlColumn("Name", EType.Text) {Unique = true };
        public readonly SqlColumn IsLocked = new SqlColumn("IsLocked", EType.Bool);
        #endregion

    }
}
