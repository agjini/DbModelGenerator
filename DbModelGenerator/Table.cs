using System.Collections.Generic;

namespace DbModelGenerator
{
    public sealed class Table
    {
        public Table(string name, IEnumerable<Column> columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; }
        public IEnumerable<Column> Columns { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}