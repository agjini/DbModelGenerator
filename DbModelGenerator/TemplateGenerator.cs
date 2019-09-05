using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class TemplateGenerator
    {
        public static IEnumerable<ITaskItem> Generate(Schema schema, string projectPath, string entityInterface,
            string primaryKeyAttribute, TaskLoggingHelper log)
        {
            if (!schema.Tables.Any())
            {
                return Array.Empty<ITaskItem>();
            }

            var scriptNamespace = Path.GetFileName(schema.ScriptDirectory);

            if (scriptNamespace == null)
            {
                throw new ArgumentException($"Project script namespace not found for '{schema.ScriptDirectory}' !");
            }

            var generatedPath = Path.Combine(projectPath, "Generated", "Db", scriptNamespace);
            try
            {
                Directory.Delete(generatedPath, true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            Directory.CreateDirectory(generatedPath);

            var taskItems = new List<ITaskItem>();
            foreach (var table in schema.Tables)
            {
                var className = ToPascalCase(table.Name);

                var outputFile = Path.Combine(generatedPath, $"{className}.cs");

                var ns = $"{Path.GetFileName(projectPath)}.Generated.Db.{scriptNamespace}";

                var content = GenerateClass(ns, table, entityInterface, primaryKeyAttribute);
                File.WriteAllText(outputFile, content, Encoding.UTF8);

                taskItems.Add(new TaskItem(outputFile));
                log.LogMessage(MessageImportance.Normal, $"Table '{table}' -> {outputFile}");
            }

            return taskItems;
        }

        public static string GenerateClass(string ns, Table table, string entityInterface, string primaryKeyAttribute)
        {
            var className = ToPascalCase(table.Name);

            var entityInterfaceClass = ParseClassName(entityInterface);
            var primaryKeyAttributeClass = ParseClassName(primaryKeyAttribute);
            var contentBuilder = new StringBuilder();

            if (ColumnParser.RequiresSystemUsing(table.Columns))
            {
                contentBuilder.Append("using System;\n");
            }

            var idColumn = table.Columns
                .FirstOrDefault(c => c.IsPrimaryKey && c.Name.ToLower().Equals("id"));

            var hasPrimaryKeys = table.Columns
                .Any(c => c.IsPrimaryKey);

            if (entityInterfaceClass != null && idColumn != null)
            {
                contentBuilder.Append($"using {entityInterfaceClass.Item1};\n");
            }

            if (primaryKeyAttributeClass != null && hasPrimaryKeys && (entityInterfaceClass == null ||
                                                                       !primaryKeyAttributeClass.Item1.Equals(
                                                                           entityInterfaceClass.Item1)))
            {
                contentBuilder.Append($"using {primaryKeyAttributeClass.Item1};\n");
            }

            contentBuilder.Append($"\nnamespace {ns}\n{{\n\n");

            contentBuilder.Append($"\tpublic sealed class {className}");
            if (entityInterfaceClass != null && idColumn != null)
            {
                contentBuilder.Append($" : {entityInterfaceClass.Item2}<{idColumn.TypeAsString()}>");
            }

            contentBuilder.Append("\n\t{\n\n");

            var args = string.Join(", ", table.Columns.Select(c => $"{c.TypeAsString()} {c.Name}"));

            contentBuilder.Append($"\t\tpublic {className}({args})\n\t\t{{\n");

            contentBuilder.Append(string.Join("\n",
                table.Columns.Select(c => $"\t\t\t{ToPascalCase(c.Name)} = {c.Name};")));
            contentBuilder.Append("\n\t\t}\n\n");

            foreach (var column in table.Columns)
            {
                if (primaryKeyAttributeClass != null && column.IsPrimaryKey)
                {
                    contentBuilder.Append(
                        $"\t\t[{primaryKeyAttributeClass.Item2}]\n");
                }

                Console.WriteLine("Column {0}, {1} => {2}", column, column.IsAutoIncrement, column.TypeAsString());
                contentBuilder.Append(
                    $"\t\tpublic {column.TypeAsString()} {ToPascalCase(column.Name)} {{ get; }}\n\n");
            }

            contentBuilder.Append("\t}\n\n}");
            var content = contentBuilder.ToString();
            return content;
        }

        private static string ToPascalCase(string s)
        {
            var words = s.Split(new[] {'-', '_'}, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder(words.Sum(x => x.Length));
            foreach (var word in words)
            {
                sb.Append(word[0].ToString().ToUpper() + word.Substring(1));
            }

            return sb.ToString();
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
}