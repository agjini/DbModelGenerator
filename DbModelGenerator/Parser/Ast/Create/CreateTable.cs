using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Create
{
    public sealed class CreateTable : DdlTableStatement
    {
        public CreateTable(ImmutableList<ColumnDefinition> columnDefinitions,
            ImmutableList<ConstraintDefinition> constraintDefinitions,
            string table)
        {
            ColumnDefinitions = columnDefinitions;
            ConstraintDefinitions = constraintDefinitions;
            Table = table;
        }

        public ImmutableList<ColumnDefinition> ColumnDefinitions { get; }
        public ImmutableList<ConstraintDefinition> ConstraintDefinitions { get; }
        public override string Table { get; }
    }
}