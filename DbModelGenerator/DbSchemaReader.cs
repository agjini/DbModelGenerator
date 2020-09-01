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
                var content = IgnoreComments(File.ReadAllText(file, Encoding.UTF8));

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

                            var (newTableName, newColumns) = AlterColumns(tables[a.Table], a);
                            tables.Remove(a.Table);
                            tables[newTableName] = newColumns;
                            break;
                        case DropTable a:
                            if (!tables.Remove(a.Table))
                            {
                                throw new ArgumentException($"Table {a.Table} not found");
                            }

                            break;
                    }
                }
            }

            return new Schema(scriptDirectory, tables
                .Select(e => new Table(e.Key, e.Value.Columns.ToImmutableList()))
                .ToImmutableList());
        }

        private static string IgnoreComments(string content)
        {
            return new Regex(@"--.*\n").Replace(content, "");
        }

        private static (string, ColumnsCollection) AlterColumns(ColumnsCollection columns, AlterTable alterTable)
        {
            var table = alterTable.Table;

            foreach (var alterTableStatement in alterTable.DdlAlterTableStatements)
            {
                switch (alterTableStatement)
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
                    case AlterColumn a:
                        if (!columns.Alter(a.Column, a.NotNullAction))
                        {
                            throw new ArgumentException($"Column '{alterTable.Table}.{a.Column}' does not exist");
                        }

                        break;
                    case RenameColumn r:
                        if (!columns.Rename(r.Column, r.NewName))
                        {
                            throw new ArgumentException($"Column '{alterTable.Table}.{r.Column}' does not exist");
                        }

                        break;
                    case RenameTable r:
                        table = r.NewName;

                        break;
                }
            }

            return (table, columns);
        }
    }
}