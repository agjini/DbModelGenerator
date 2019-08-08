using System.Collections.Generic;
using System.Linq;

namespace DbModelGenerator
{
    public static class ColumnParser
    {
        public static bool RequiresSystemUsing(IEnumerable<Column> columns)
        {
            return columns.Any(column => column.RequiresSystemUsing());
        }

        public static Column Parse(dynamic column)
        {
            return new Column(column.name, ParseType(column.type), column.notnull.Equals("1"), column.pk.Equals("1"));
        }

        private static string ParseType(string datatype)
        {
            switch (datatype.ToLower())
            {
                case "int": return "int";

                case "decimal": return "decimal";

                case "money": return "decimal";

                case "uniqueidentifier": return "Guid";

                case "datetime": return "DateTime";

                case "bit": return "bool";

                default:
                    return "string";
            }
        }
    }

    public sealed class Column
    {
        public Column(string name, string type, bool isNullable, bool isPrimaryKey)
        {
            Name = name;
            Type = type;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
        }

        public string Name { get; }
        public string Type { get; }
        public bool IsNullable { get; }
        public bool IsPrimaryKey { get; }

        public string TypeAsString()
        {
            var n = IsNullable ? "?" : "";
            return $"{Type}{n}";
        }

        public bool RequiresSystemUsing()
        {
            return Type.Equals("Guid") || Type.Equals("DateTime");
        }
    }
}