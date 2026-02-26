using System.Collections.Immutable;

namespace DbModelGenerator.Parser.Ast.Constraint;

public class ForeignKeyConstraint : ColumnConstraint
{
    public ForeignKeyConstraint(ImmutableSortedSet<string> columns, string attributes)
    {
        Columns = columns;
        Attributes = attributes;
    }

    public sealed override ImmutableSortedSet<string> Columns { get; protected set; }

    public string Attributes { get; }

    protected bool Equals(ForeignKeyConstraint other)
    {
        return Equals(Columns, other.Columns) && Attributes == other.Attributes;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ForeignKeyConstraint) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Columns != null ? Columns.GetHashCode() : 0) * 397) ^
                   (Attributes != null ? Attributes.GetHashCode() : 0);
        }
    }
}