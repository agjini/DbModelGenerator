using System;
using System.Collections.Immutable;
using System.Linq;

namespace DbModelGenerator.Parser.Ast.Constraint;

public abstract class ColumnConstraint
{
    public abstract ImmutableSortedSet<string> Columns { get; protected set; }

    public void RenameColumn(string column, string newName)
    {
        Columns = Columns.Select(col =>
                col.Equals(column, StringComparison.InvariantCultureIgnoreCase) ? newName : col)
            .ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase);
    }
}