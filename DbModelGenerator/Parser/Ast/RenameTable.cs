namespace DbModelGenerator.Parser.Ast
{
    public sealed class RenameTable : DdlAlterTableStatement
    {
        public RenameTable(string newName)
        {
            NewName = newName;
        }

        public string NewName { get; }
    }
}