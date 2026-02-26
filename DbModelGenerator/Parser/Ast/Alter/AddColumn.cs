using DbModelGenerator.Parser.Ast.Create;

namespace DbModelGenerator.Parser.Ast.Alter;

public sealed class AddColumn : DdlAlterTableStatement
{
    public AddColumn(ColumnDefinition columnDefinition)
    {
        ColumnDefinition = columnDefinition;
    }

    public string Column => ColumnDefinition.Identifier;
    public ColumnDefinition ColumnDefinition { get; }
}