using System.Collections.Immutable;

namespace DbModelGenerator
{
    public sealed class Table
    {
        public Table(string name, ImmutableList<Column> columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; }
        public ImmutableList<Column> Columns { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}