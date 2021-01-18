using DbModelGenerator.Parser.Ast.Constraint;
using DbModelGenerator.Util;

namespace DbModelGenerator.Parser.Ast.Create
{
    public sealed class ConstraintDefinition : CreateTableStatement
    {
        public ConstraintDefinition(Option<string> identifier, ColumnConstraint columnConstraint)
        {
            Identifier = identifier;
            ColumnConstraint = columnConstraint;
        }

        public Option<string> Identifier { get; }

        public ColumnConstraint ColumnConstraint { get; }

        private bool Equals(ConstraintDefinition other)
        {
            return Equals(Identifier, other.Identifier) && Equals(ColumnConstraint, other.ColumnConstraint);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ConstraintDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Identifier != null ? Identifier.GetHashCode() : 0) * 397) ^
                       (ColumnConstraint != null ? ColumnConstraint.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}