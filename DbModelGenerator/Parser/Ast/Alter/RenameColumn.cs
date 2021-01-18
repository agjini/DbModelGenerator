namespace DbModelGenerator.Parser.Ast.Alter
{
    public sealed class RenameColumn : DdlAlterTableStatement
    {
        public RenameColumn(string column, string newName)
        {
            Column = column;
            NewName = newName;
        }

        public string Column { get; }
        public string NewName { get; }
    }
}