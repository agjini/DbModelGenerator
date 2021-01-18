using DbModelGenerator.Parser.Ast.Create;

namespace DbModelGenerator.Parser.Ast.Alter
{
    public sealed class AddConstraint : DdlAlterTableStatement
    {
        public AddConstraint(ConstraintDefinition constraintDefinition)
        {
            ConstraintDefinition = constraintDefinition;
        }

        public ConstraintDefinition ConstraintDefinition { get; }
    }
}