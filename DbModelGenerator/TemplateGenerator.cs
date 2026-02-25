using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DbModelGenerator;

public sealed class TemplateGenerator
{
    public static ImmutableDictionary<string, string> Generate(ProjectInfo projectInfo, Schema schema, Parameters parameters)
    {
        if (!schema.Tables.Any())
        {
            return [];
        }

        var scriptNamespace = Path.GetFileName(schema.ScriptDirectory);

        if (scriptNamespace == null)
        {
            throw new ArgumentException($"Project script namespace not found for '{schema.ScriptDirectory}' !");
        }

        var generatedPath = Path.Combine(projectInfo.ProjectName, "Generated", "Db", scriptNamespace);

        var generatedFiles = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (var table in schema.Tables)
        {
            var className = GetClassName(table, parameters.Suffix);

            var ns = $"{Path.GetFileName(projectInfo.ProjectName)}.Generated.Db.{scriptNamespace}";

            var content = GenerateClass(ns, table, parameters.Interfaces, parameters.PrimaryKeyAttribute,
                parameters.AutoIncrementAttribute, parameters.Suffix);

            generatedFiles.Add($"{Path.Combine(generatedPath, $"{className}.cs")}", content);
        }

        return generatedFiles.ToImmutable();
    }

    public static string GenerateClass(string ns, Table table, ImmutableList<string> entityInterface,
        string primaryKeyAttribute,
        string autoIncrementAttribute, string suffix)
    {
        var className = GetClassName(table, suffix);

        var entityInterfaces = entityInterface.Select(e => ParseEntityInterface(e.Trim()));
        var primaryKeyAttributeClass = ParseClassName(primaryKeyAttribute);
        var autoIncrementAttributeClass = ParseClassName(autoIncrementAttribute);
        var contentBuilder = new StringBuilder();

        if (ColumnParser.RequiresSystemUsing(table.Columns))
        {
            contentBuilder.Append("using System;\n");
        }

        var hasPrimaryKeys = table.Columns
            .Any(c => table.IsPrimaryKey(c.Name));

        var matchingInterfaces = entityInterfaces.Where(e => e.Properties.All(p =>
                table.Columns.Exists(c =>
                    string.Equals(p.Item1, c.Name, StringComparison.CurrentCultureIgnoreCase))))
            .ToImmutableList();

        foreach (var matchingInterfaceNamespace in matchingInterfaces.Select(m => m.Namespace).Distinct())
        {
            contentBuilder.Append($"using {matchingInterfaceNamespace};\n");
        }

        if (primaryKeyAttributeClass != null && hasPrimaryKeys)
        {
            if (!matchingInterfaces.Exists(e => string.Equals($"{e.Namespace}",
                    primaryKeyAttributeClass.Item1, StringComparison.CurrentCultureIgnoreCase)))
            {
                contentBuilder.Append($"using {primaryKeyAttributeClass.Item1};\n");
            }

            var hasAutoIncrementKeys = table.Columns
                .Any(c => c.IsAutoIncrement);

            if (autoIncrementAttributeClass != null
                && hasAutoIncrementKeys
                && !matchingInterfaces.Exists(e => string.Equals($"{e.Namespace}",
                    autoIncrementAttributeClass.Item1, StringComparison.CurrentCultureIgnoreCase))
                && !autoIncrementAttributeClass.Item1.Equals(primaryKeyAttributeClass.Item1))
            {
                contentBuilder.Append($"using {autoIncrementAttributeClass.Item1};\n");
            }
        }

        contentBuilder.Append($"\nnamespace {ns};\n\n");

        contentBuilder.Append($"public sealed class {className}");
        if (matchingInterfaces.Count > 0)
        {
            contentBuilder.Append(
                $" : {string.Join(", ", matchingInterfaces.Select(e => e.GetDeclaration(table.Columns)))}");
        }

        contentBuilder.Append("\n{\n\n");

        var args = string.Join(", ", table.Columns.Select(c => $"{c.TypeAsString()} {c.Name}"));

        contentBuilder.Append($"\tpublic {className}({args})\n\t{{\n");

        contentBuilder.Append(string.Join("\n",
            table.Columns.Select(c => $"\t\t{ToPascalCase(c.Name)} = {c.Name};")));
        contentBuilder.Append("\n\t}\n\n");

        foreach (var column in table.Columns)
        {
            if (primaryKeyAttributeClass != null && table.IsPrimaryKey(column.Name))
            {
                contentBuilder.Append(
                    $"\t[{primaryKeyAttributeClass.Item2}]\n");
            }

            if (autoIncrementAttributeClass != null && column.IsAutoIncrement)
            {
                contentBuilder.Append(
                    $"\t[{autoIncrementAttributeClass.Item2}]\n");
            }

            contentBuilder.Append(
                $"\tpublic {column.TypeAsString()} {ToPascalCase(column.Name)} {{ get; }}\n\n");
        }

        contentBuilder.Append("}");
        var content = contentBuilder.ToString();
        return content;
    }

    private static string GetClassName(Table table, string suffix)
    {
        var s = suffix != null ? $"_{suffix}" : "";
        var className = ToPascalCase(table.Name + s);
        return className;
    }

    private static string ToPascalCase(string s)
    {
        var words = s.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder(words.Sum(x => x.Length));
        foreach (var word in words)
        {
            sb.Append(word[0].ToString().ToUpper() + word.Substring(1));
        }

        return sb.ToString();
    }

    private static EntityInterface ParseEntityInterface(string identityInterface)
    {
        if (identityInterface == null)
        {
            return null;
        }

        const string pattern = @"(?<ns>.*)\.(?<classname>\w+)\s*(\((?<properties>[\w!,]*)\))?";

        var match = Regex.Match(identityInterface, pattern);
        if (match.Success)
        {
            var ns = match.Groups["ns"].Value;
            var className = match.Groups["classname"].Value;
            var matchGroup = match.Groups["properties"].Value;
            ImmutableList<(string, bool)> properties;
            if (matchGroup == "")
            {
                properties = ImmutableList.Create(("id", true));
            }
            else
            {
                properties = matchGroup
                    .Split(',')
                    .Select(s =>
                    {
                        var property = s.Trim();
                        return property.EndsWith("!")
                            ? (property.Substring(0, property.Length - 1), true)
                            : (property, false);
                    })
                    .ToImmutableList();
            }

            return new EntityInterface(ns, className, properties);
        }

        throw new ArgumentException(
                $"Parameter IdentityInterface has wrong format : {identityInterface} must be of the form 'Namespace.ClassName'")
            ;
    }

    private static Tuple<string, string> ParseClassName(string identityInterface)
    {
        if (identityInterface == null)
        {
            return null;
        }

        const string pattern = @"(?<ns>.*)\.(?<classname>[^.]+)";

        var match = Regex.Match(identityInterface, pattern);
        if (match.Success)
        {
            return new Tuple<string, string>(match.Groups["ns"].Value, match.Groups["classname"].Value);
        }

        throw new ArgumentException(
                $"Parameter IdentityInterface has wrong format : {identityInterface} must be of the form 'Namespace.ClassName'")
            ;
    }
}