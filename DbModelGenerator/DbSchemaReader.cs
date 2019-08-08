using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class DbSchemaReader : IDisposable
    {
        private readonly SqliteMemoryDatabase database;
        private readonly DirectoryInfo directoryInfo;

        public DbSchemaReader()
        {
            directoryInfo = Directory.CreateDirectory(Path.GetTempPath() + Guid.NewGuid());
            database = new SqliteMemoryDatabase(directoryInfo.FullName);
        }

        public Schema Read(string projectPath, string scriptDirectory, TaskLoggingHelper log)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new ArgumentException($"Project '{projectPath}' does not exist !");
            }

            var ignore = new[] {"SchemaVersions", "sqlite_sequence"};

            var scriptNamespace = Path.GetFileName(scriptDirectory);

            if (scriptNamespace == null)
            {
                throw new ArgumentException($"Project script namespace not found for '{scriptDirectory}' !");
            }

            database.UpdgradeSchema(scriptDirectory, scriptNamespace, log);

            using (var connection = database.NewConnection(scriptNamespace))
            {
                connection.Open();

                var tables = connection
                    .Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
                    .Where(t => !ignore.Contains(t))
                    .ToImmutableList();

                if (!tables.Any())
                {
                    return new Schema(scriptDirectory, Array.Empty<Table>());
                }

                var list = new List<Table>();
                foreach (var table in tables)
                {
                    var columns = connection.Query<dynamic>($"pragma table_info('{table}');")
                        .Select(ColumnParser.Parse)
                        .ToImmutableList();
                    list.Add(new Table(table, columns));
                }

                return new Schema(scriptDirectory, list);
            }
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