namespace DbModelGenerator.Parser.Ast
{
    public sealed class AlterTable : DdlTableStatement
    {
        public AlterTable(string table, DdlColumnStatement ddlColumnStatement)
        {
            Table = table;
            DdlColumnStatement = ddlColumnStatement;
        }

        public override string Table { get; }
        public DdlColumnStatement DdlColumnStatement { get; }
    }
}