namespace DbModelGenerator.Parser.Ast
{
    public sealed class DropColumn : DdlColumnStatement
    {
        public DropColumn(string column)
        {
            Column = column;
        }

        public override string Column { get; }
    }
}