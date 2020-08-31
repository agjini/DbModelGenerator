namespace DbModelGenerator.Parser.Ast
{
    public sealed class AlterColumn : DdlAlterTableStatement
    {
        public AlterColumn(ColumnDefinition columnDefinition)
        {
            ColumnDefinition = columnDefinition;
        }

        public string Column => ColumnDefinition.Identifier;
        public ColumnDefinition ColumnDefinition { get; }
    }
}