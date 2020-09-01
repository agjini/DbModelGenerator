using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast
{
    public sealed class AlterTable : DdlTableStatement
    {
        public AlterTable(string table, ImmutableList<DdlAlterTableStatement> ddlAlterTableStatements)
        {
            Table = table;
            DdlAlterTableStatements = ddlAlterTableStatements;
        }

        public override string Table { get; }
        public ImmutableList<DdlAlterTableStatement> DdlAlterTableStatements { get; }
    }
}