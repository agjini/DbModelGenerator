namespace DbModelGenerator.Parser.Ast
{
    public sealed class AlterTable : DdlTableStatement
    {
        public AlterTable(string table, DdlAlterTableStatement ddlAlterTableStatement)
        {
            Table = table;
            DdlAlterTableStatement = ddlAlterTableStatement;
        }

        public override string Table { get; }
        public DdlAlterTableStatement DdlAlterTableStatement { get; }
    }
}