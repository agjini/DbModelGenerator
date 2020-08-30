using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Constraint
{
    public class PrimaryKeyConstraint : ColumnConstraint
    {
        public PrimaryKeyConstraint(ImmutableList<string> columns)
        {
            Columns = columns;
        }

        public ImmutableList<string> Columns { get; }
    }
}