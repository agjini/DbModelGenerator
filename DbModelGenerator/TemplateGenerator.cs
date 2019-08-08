using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public IEnumerable<ITaskItem> Generate(IEnumerable<Schema> schemas, string projectPath,
            string identityInterfaceParam)
        {
            return schemas.SelectMany(s => Generate(s, projectPath, identityInterfaceParam))
                .ToImmutableList();
        }

        public static IEnumerable<ITaskItem> Generate(Schema schema, string projectPath, string identityInterface)
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
                var content = GenerateClass(ns, table, identityInterface);
                File.WriteAllText(outputFile, content, Encoding.UTF8);

                taskItems.Add(new TaskItem(outputFile));
                Console.WriteLine($"Table '{table}' -> {outputFile}");
            }

            return taskItems;
        }

        public static string GenerateClass(string ns, Table table, string identityInterfaceParam)
        {
            var className = ToPascalCase(table.Name);

            var identityInterface = ParseIdentityInterface(identityInterfaceParam);

            var contentBuilder = new StringBuilder();

            if (ColumnParser.RequiresSystemUsing(table.Columns))
            {
                contentBuilder.Append("using System;\n");
            }

            var idColumn = table.Columns
                .FirstOrDefault(c => c.IsPrimaryKey && c.Name.ToLower().Equals("id"));

            if (identityInterface != null && idColumn != null)
            {
                contentBuilder.Append($"using {identityInterface.Item1};\n");
            }

            contentBuilder.Append($"\nnamespace {ns}\n{{\n\n");
            
            contentBuilder.Append($"\tpublic sealed class {className}");
            if (identityInterface != null && idColumn != null)
            {
                contentBuilder.Append($" : {identityInterface.Item2}<{idColumn.TypeAsString()}>");
            }

            contentBuilder.Append("\n\t{\n\n");

            var args = string.Join(", ", table.Columns.Select(c => $"{c.TypeAsString()} {c.Name}"));

            contentBuilder.Append($"\t\tpublic {className}({args})\n\t\t{{\n");

            contentBuilder.Append(string.Join("\n",
                table.Columns.Select(c => $"\t\t\t{ToPascalCase(c.Name)} = {c.Name};")));
            contentBuilder.Append("\n\t\t}\n\n");

            foreach (var column in table.Columns)
            {
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

        private static Tuple<string, string> ParseIdentityInterface(string identityInterface)
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