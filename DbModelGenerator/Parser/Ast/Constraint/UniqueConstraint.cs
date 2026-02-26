using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Constraint;

public class UniqueConstraint : ColumnConstraint
{
    public UniqueConstraint(ImmutableSortedSet<string> columns)
    {
        Columns = columns;
    }

    public sealed override ImmutableSortedSet<string> Columns { get; protected set; }

    protected bool Equals(UniqueConstraint other)
    {
        return Equals(Columns, other.Columns);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UniqueConstraint) obj);
    }

    public override int GetHashCode()
    {
        return (Columns != null ? Columns.GetHashCode() : 0);
    }
}