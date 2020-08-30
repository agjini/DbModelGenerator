using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast;
using Microsoft.Build.Utilities;
using Sprache;

namespace DbModelGenerator
{
    public sealed class DbSchemaReader
    {
        public Schema Read(string scriptDirectory, TaskLoggingHelper log)
        {
            var scriptNamespace = Path.GetFileName(scriptDirectory);

            if (scriptNamespace == null)
            {
                throw new ArgumentException($"Project script namespace not found for '{scriptDirectory}' !");
            }

            var tables =
                new SortedDictionary<string, ColumnsCollection>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var file in Directory.GetFiles(scriptDirectory).OrderBy(f => f))
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                var statements = Parser.Parser.DdlTableStatements.Parse(content);

                foreach (var ddlTableStatement in statements)
                {
                    switch (ddlTableStatement)
                    {
                        case CreateTable a:
                            var columnConstraints = a.ConstraintDefinitions.Select(cd => cd.ColumnConstraint)
                                .ToList();
                            var columns = a.ColumnDefinitions
                                .Select(c => c.ToColumn(columnConstraints))
                                .ToList();
                            tables.Add(a.Table, new ColumnsCollection(columns, columnConstraints));
                            break;
                        case AlterTable a:
                            if (!tables.ContainsKey(a.Table))
                            {
                                throw new ArgumentException($"Table {a.Table} not found");
                            }

                            tables[a.Table] = AlterColumns(tables[a.Table], a);
                            break;
                        case DropTable a:
                            break;
                    }
                }
            }

            return new Schema(scriptDirectory, tables
                .Select(e => new Table(e.Key, e.Value.Columns.ToImmutableList()))
                .ToImmutableList());
        }

        private ColumnsCollection AlterColumns(ColumnsCollection columns, AlterTable alterTable)
        {
            switch (alterTable.DdlColumnStatement)
            {
                case AddColumn a:
                    columns.Add(a.ColumnDefinition);

                    break;
                case DropColumn d:
                    if (!columns.Remove(d.Column))
                    {
                        throw new ArgumentException($"Column '{alterTable.Table}.{d.Column}' does not exist");
                    }

                    break;
                case RenameColumn r:
                    if (!columns.Rename(r.Column, r.NewName))
                    {
                        throw new ArgumentException($"Column '{alterTable.Table}.{r.Column}' does not exist");
                    }

                    break;
            }

            return columns;
        }

        private IDictionary<string, Column> ReadColumns(string definition)
        {
            var primaryKeys = ReadPrimaryKeys(definition);
            var columnDefinitionRegex =
                new Regex(@"\s*(?<column>\w+)\s+((?<type>\w+)\s*(?<parameters>\(\s*\d+\s*\))?)(?<attributes>[^,)]*)?");
            var columns = new Dictionary<string, Column>(StringComparer.InvariantCultureIgnoreCase);
            foreach (Match match in columnDefinitionRegex.Matches(definition))
            {
                var column = match.Groups["column"].Value;
                var type = match.Groups["type"].Value;
                var parameters = match.Groups["parameters"].Value;
                var attributes = match.Groups["attributes"]?.Value ?? "";

                if (!(column.ToUpper().Equals("PRIMARY") && type.ToUpper().Equals("KEY")))
                {
                    var isNotNull = new Regex(@"NOT\s+NULL", RegexOptions.IgnoreCase).IsMatch(attributes);
                    var isSerial = type.ToUpper() == "SERIAL";
                    columns.Add(column.ToUpper(), new Column(column, ColumnParser.ParseType(type), !isNotNull,
                        primaryKeys.Contains(column.ToLower()), isSerial));
                }
            }

            return columns;
        }

        private ImmutableHashSet<string> ReadPrimaryKeys(string definition)
        {
            var primaryKeyDefinitionRegex =
                new Regex(@"PRIMARY KEY \((?<columns>[^)]+)\)", RegexOptions.IgnoreCase);
            var primaryKeyMatch = primaryKeyDefinitionRegex.Match(definition);
            if (primaryKeyMatch.Success)
            {
                var columns = primaryKeyMatch.Groups["columns"].Value;
                return columns.Split(',')
                    .Select(n => n.Trim().ToLower())
                    .ToImmutableHashSet();
            }

            return ImmutableHashSet<string>.Empty;
        }
    }
}