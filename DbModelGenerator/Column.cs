using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbModelGenerator;

public static class ColumnParser
{
    private static ImmutableDictionary<string, ParameterType> _mappings =
        ImmutableDictionary<string, ParameterType>.Empty;

    public static void SetMappings(ImmutableDictionary<string, ParameterType> mappings) => _mappings = mappings;

    public static string GetUsing(ImmutableList<Column> columns)
    {
        var usings = ImmutableList.Create<string>();
        if (columns.Any(column => column.RequiresSystemUsing()))
        {
            usings = usings.Add("using System;");
        }

        if (columns.Any(column => column.RequiresPgVectorUsing()))
        {
            usings = usings.Add("using Pgvector;");
        }

        if (columns.Any(column => column.RequiresUsingJson()))
        {
            usings = usings.Add($"using {GetLibrary("jsonb")!};");
        }

        return usings.Any() ? $"{string.Join("\n", usings)}\n" : string.Empty;
    }

    private static bool IsArrayType(string dataType)
    {
        var regex = new Regex(@"(\[(?:\d)*\])+");
        return regex.IsMatch(dataType);
    }

    public static string ParseType(string datatype)
    {
        var type = ParseGenericType(datatype);

        return !IsArrayType(datatype) ? type : $"ImmutableList<{type}>";
    }

    private static string ParseGenericType(string datatype)
    {
        var match = Regex.Match(datatype, @"(\w+).*(\[(?:\d)*\])*");
        var value = match.Groups[1].Value.ToLower();

        var typeMapped = GetType(value);
        if (typeMapped is not null)
        {
            return typeMapped;
        }

        switch (value)
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

            case "uuid":
            case "uniqueidentifier":
                return "Guid";

            case "date":
                return "DateOnly";
            case "time":
                return "TimeOnly";
            case "timestamp":
            case "datetime":
                return "DateTime";

            case "bit":
            case "boolean":
                return "bool";

            case "vector":
                return "Vector";

            default:
                return "string";
        }
    }

    private static ParameterType? GetParameterType(string datatype)
    {
        return _mappings.TryGetValue(datatype, out var parameterType) ? parameterType : null;
    }

    public static string? GetType(string datatype)
    {
        var parameterType = GetParameterType(datatype);
        return parameterType?.Type;
    }

    private static string? GetLibrary(string datatype)
    {
        var parameterType = GetParameterType(datatype);
        return parameterType?.Library;
    }
}

public sealed class Column
{
    public Column(string name, string type, bool isNullable, bool isAutoIncrementByDefinition,
        bool isAutoIncrementByType)
    {
        Name = name;
        Type = type;
        IsNullable = isNullable;
        IsAutoIncrementByDefinition = isAutoIncrementByDefinition;
        IsAutoIncrementByType = isAutoIncrementByType;
    }

    public string Name { get; }
    public string Type { get; }
    public bool IsNullable { get; }
    public bool IsAutoIncrementByDefinition { get; }
    public bool IsAutoIncrementByType { get; }

    public bool IsAutoIncrement => IsAutoIncrementByDefinition || IsAutoIncrementByType;

    public string TypeAsString()
    {
        var n = IsNullable || IsAutoIncrement ? "?" : "";
        return $"{Type}{n}";
    }

    public bool RequiresSystemUsing()
    {
        return Type.Equals("Guid") || Type.Equals("DateTime") || Type.Equals("TimeOnly") || Type.Equals("DateOnly");
    }

    public bool RequiresPgVectorUsing()
    {
        return Type.Equals("Vector");
    }

    public bool RequiresUsingJson()
    {
        return Type.Equals(ColumnParser.GetType("jsonb"));
    }

    public override string ToString()
    {
        return Name;
    }
}