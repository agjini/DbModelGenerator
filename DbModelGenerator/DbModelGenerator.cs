using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class DbModelGenerator : IDisposable
    {
        private readonly SqliteMemoryDatabase database;
        private readonly DirectoryInfo directoryInfo;

        public DbModelGenerator()
        {
            directoryInfo = Directory.CreateDirectory(Path.GetTempPath() + Guid.NewGuid());
            database = new SqliteMemoryDatabase(directoryInfo.FullName);
        }

        public ITaskItem[] Generate(string projectPath, string scriptsPath)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new ArgumentException($"Project '{projectPath}' does not exist !");
            }

            if (!Directory.Exists(scriptsPath))
            {
                throw new ArgumentException($"Project scripts path '{scriptsPath}' does not exist !");
            }

            return Directory.GetDirectories(scriptsPath)
                .SelectMany(d => GenerateDirectory(d, projectPath))
                .ToArray();
        }

        private IEnumerable<ITaskItem> GenerateDirectory(string scriptDirectory, string projectPath)
        {
            var ignore = new[] {"SchemaVersions", "sqlite_sequence"};

            var scriptNamespace = Path.GetFileName(scriptDirectory);

            if (scriptNamespace == null)
            {
                throw new ArgumentException($"Project script namespace not found for '{scriptDirectory}' !");
            }

            database.UpdgradeSchema(scriptDirectory, scriptNamespace);

            using (var connection = database.NewConnection(scriptNamespace))
            {
                connection.Open();

                var tables = connection
                    .Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
                    .ToImmutableList();

                if (!tables.Any())
                {
                    return Array.Empty<ITaskItem>();
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
                foreach (var table in tables.Where(t => !ignore.Contains(t)))
                {
                    var columns = connection.Query<dynamic>($"pragma table_info('{table}');")
                        .Select(ColumnParser.Parse)
                        .ToImmutableList();

                    var className = ToPascalCase(table);

                    var outputFile = Path.Combine(generatedPath, $"{className}.cs");

                    var contentBuilder = new StringBuilder();

                    var ns = $"{Path.GetFileName(projectPath)}.Generated.Db.{scriptNamespace}";

                    if (ColumnParser.RequiresSystemUsing(columns))
                    {
                        contentBuilder.Append("using System;\n\n");
                    }

                    contentBuilder.Append($"namespace {ns}\n{{\n\n");
                    contentBuilder.Append($"\tpublic sealed class {className}\n{{\n\n");

                    var args = string.Join(", ", columns.Select(c => $"{c.TypeAsString()} {c.Name}"));

                    contentBuilder.Append($"\t\tpublic {className}({args})\n{{\n");
                    contentBuilder.Append(string.Join("\n",
                        columns.Select(c => $"\t\t\t{ToPascalCase(c.Name)} = {c.Name};")));
                    contentBuilder.Append("\n\t\t}\n\n");

                    foreach (var column in columns)
                    {
                        contentBuilder.Append(
                            $"\t\tpublic {column.TypeAsString()} {ToPascalCase(column.Name)} {{ get; }}\n\n");
                    }

                    contentBuilder.Append("\t}\n\n}");
                    File.WriteAllText(outputFile, contentBuilder.ToString(), Encoding.UTF8);

                    taskItems.Add(new TaskItem(outputFile));
                    Console.WriteLine($"Table '{table}' -> {outputFile}");
                }

                return taskItems;
            }
        }

        public static string ToPascalCase(string s)
        {
            var words = s.Split(new[] {'-', '_'}, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder(words.Sum(x => x.Length));

            foreach (var word in words)
            {
                sb.Append(word[0].ToString().ToUpper() + word.Substring(1));
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            try
            {
                directoryInfo.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }
}