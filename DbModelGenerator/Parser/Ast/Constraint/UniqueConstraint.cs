using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Constraint
{
    public class UniqueConstraint : ColumnConstraint
    {
        public UniqueConstraint(ImmutableList<string> columns)
        {
            Columns = columns;
        }

        public ImmutableList<string> Columns { get; }
    }
}