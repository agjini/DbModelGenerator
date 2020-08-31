namespace DbModelGenerator.Parser.Ast
{
    public sealed class DropColumn : DdlAlterTableStatement
    {
        public DropColumn(string column)
        {
            Column = column;
        }

        public string Column { get; }
    }
}