using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Constraint
{
    public class ForeignKeyConstraint : ColumnConstraint
    {
        public ForeignKeyConstraint(ImmutableList<string> columns, string attributes)
        {
            Columns = columns;
            Attributes = attributes;
        }

        public ImmutableList<string> Columns { get; }
        public string Attributes { get; }
    }
}