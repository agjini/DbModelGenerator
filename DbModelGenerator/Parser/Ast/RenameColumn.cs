namespace DbModelGenerator.Parser.Ast
{
    public sealed class RenameColumn : DdlColumnStatement
    {
        public RenameColumn(string column, string newName)
        {
            Column = column;
            NewName = newName;
        }

        public override string Column { get; }
        public string NewName { get; }
    }
}