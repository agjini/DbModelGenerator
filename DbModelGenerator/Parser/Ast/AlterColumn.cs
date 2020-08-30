namespace DbModelGenerator.Parser.Ast
{
    public sealed class AlterColumn : DdlColumnStatement
    {
        public AlterColumn(ColumnDefinition columnDefinition)
        {
            ColumnDefinition = columnDefinition;
        }

        public override string Column => ColumnDefinition.Identifier;
        public ColumnDefinition ColumnDefinition { get; }
    }
}