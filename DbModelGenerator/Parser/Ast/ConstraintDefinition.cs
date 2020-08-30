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
    }
}