namespace DbModelGenerator.Parser.Ast
{
    public sealed class AddColumn : DdlColumnStatement
    {
        public AddColumn(ColumnDefinition columnDefinition)
        {
            ColumnDefinition = columnDefinition;
        }

        public override string Column => ColumnDefinition.Identifier;
        public ColumnDefinition ColumnDefinition { get; }
    }
}