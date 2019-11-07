using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            string type = ParseType(column.type);
            var isAutoIncrement = ((string) column.type).ToLower().Contains("serial");
            return new Column(column.name, type, column.notnull == 0, column.pk > 0, isAutoIncrement);
        }

        private static string ParseType(string datatype)
        {
            var match = Regex.Match(datatype, @"(\w+).*");
            var value = match.Groups[1].Value;

            switch (value.ToLower())
            {
                case "serial":
                case "int":
                case "integer":
                    return "int";

                case "tinyint":
                    return "byte";

                case "binary":
                case "varbinary":
                case "blob":
                    return "byte[]";

                case "smallint":
                case "smallserial":
                    return "short";

                case "bigserial":
                case "bigint":
                    return "long";

                case "real":
                case "numeric":
                case "decimal":
                case "double precision":
                case "money":
                    return "decimal";

                case "uniqueidentifier":
                    return "Guid";

                case "date":
                case "time":
                case "timestamp":
                case "datetime":
                    return "DateTime";

                case "bit":
                case "boolean":
                    return "bool";

                default:
                    return "string";
            }
        }
    }

    public sealed class Column
    {
        public Column(string name, string type, bool isNullable, bool isPrimaryKey, bool isAutoIncrement)
        {
            Name = name;
            Type = type;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
            IsAutoIncrement = isAutoIncrement;
        }

        public string Name { get; }
        public string Type { get; }
        public bool IsNullable { get; }
        public bool IsPrimaryKey { get; }
        public bool IsAutoIncrement { get; }

        public string TypeAsString()
        {
            var n = IsNullable || IsAutoIncrement ? "?" : "";
            return $"{Type}{n}";
        }

        public bool RequiresSystemUsing()
        {
            return Type.Equals("Guid") || Type.Equals("DateTime");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}