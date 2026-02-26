using System.Collections.Immutable;

namespace DbModelGenerator;

public sealed class Table
{
    public Table(string name, ImmutableList<Column> columns, ImmutableSortedSet<string> primaryKeys)
    {
        Name = name;
        Columns = columns;
        PrimaryKeys = primaryKeys;
    }

    public string Name { get; }
    public ImmutableList<Column> Columns { get; }
    public ImmutableSortedSet<string> PrimaryKeys { get; }

    public bool IsPrimaryKey(string column)
    {
        return PrimaryKeys.Contains(column);
    }

    public override string ToString()
    {
        return Name;
    }
}