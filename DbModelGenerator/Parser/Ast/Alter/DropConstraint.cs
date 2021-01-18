namespace DbModelGenerator.Parser.Ast.Alter
{
    public sealed class DropConstraint : DdlAlterTableStatement
    {
        public DropConstraint(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }
}