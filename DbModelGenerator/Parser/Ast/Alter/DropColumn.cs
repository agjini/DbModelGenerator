namespace DbModelGenerator.Parser.Ast.Alter;

public sealed class DropColumn : DdlAlterTableStatement
{
    public DropColumn(string column)
    {
        Column = column;
    }

    public string Column { get; }
}