using System.Collections.Immutable;
using System.Linq;

namespace DbModelGenerator.Parser.Ast.Constraint
{
    public class UniqueConstraint : ColumnConstraint
    {
        public UniqueConstraint(ImmutableList<string> columns)
        {
            Columns = columns;
        }

        public ImmutableList<string> Columns { get; }

        protected bool Equals(UniqueConstraint other)
        {
            return ReferenceEquals(Columns, other.Columns) || Columns.SequenceEqual(other.Columns);
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
}