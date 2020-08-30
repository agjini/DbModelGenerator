namespace DbModelGenerator.Parser.Ast
{
    public sealed class DropTable : DdlTableStatement
    {
        public DropTable(string table)
        {
            Table = table;
        }

        public override string Table { get; }
    }
}