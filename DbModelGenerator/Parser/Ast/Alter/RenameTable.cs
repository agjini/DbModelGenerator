namespace DbModelGenerator.Parser.Ast.Alter;

public sealed class RenameTable : DdlAlterTableStatement
{
    public RenameTable(string newName)
    {
        NewName = newName;
    }

    public string NewName { get; }
}