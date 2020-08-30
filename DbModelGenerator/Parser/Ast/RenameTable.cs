namespace DbModelGenerator.Parser.Ast
{
    public sealed class RenameTable : DdlTableStatement
    {
        public RenameTable(string table, string newName)
        {
            Table = table;
            NewName = newName;
        }

        public override string Table { get; }
        public string NewName { get; }
    }
}