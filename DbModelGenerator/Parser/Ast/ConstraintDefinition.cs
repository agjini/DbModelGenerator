using DbModelGenerator.Parser.Ast.Constraint;

namespace DbModelGenerator.Parser.Ast
{
    public sealed class ConstraintDefinition : CreateTableStatement
    {
        public ConstraintDefinition(string name, ColumnConstraint columnConstraint)
        {
            Name = name;
            ColumnConstraint = columnConstraint;
        }

        public string Name { get; }
        public ColumnConstraint ColumnConstraint { get; }

        private bool Equals(ConstraintDefinition other)
        {
            return Name == other.Name && Equals(ColumnConstraint, other.ColumnConstraint);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ConstraintDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (ColumnConstraint != null ? ColumnConstraint.GetHashCode() : 0);
            }
        }
    }
}