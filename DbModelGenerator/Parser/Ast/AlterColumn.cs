namespace DbModelGenerator.Parser.Ast
{
    public enum NotNullAction
    {
        SetNotNull,
        DropNotNull
    }

    public sealed class AlterColumn : DdlAlterTableStatement
    {
        public AlterColumn(string column, NotNullAction notNullAction)
        {
            Column = column;
            NotNullAction = notNullAction;
        }

        public string Column { get; }
        public NotNullAction NotNullAction { get; }
    }
}