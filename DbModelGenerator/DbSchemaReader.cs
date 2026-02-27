using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast;
using DbModelGenerator.Parser.Ast.Alter;
using DbModelGenerator.Parser.Ast.Constraint;
using DbModelGenerator.Parser.Ast.Create;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Sprache;

namespace DbModelGenerator;

public sealed class DbSchemaReader
{
    public static (Schema, ImmutableList<SqlParserException>) Read(string scriptDirectory,
        IEnumerable<InputFile> allSqlFilesContent)
    {
        var tables =
            new SortedDictionary<string, ColumnsCollection>(StringComparer.InvariantCultureIgnoreCase);

        var parserExceptions = ImmutableList.CreateBuilder<SqlParserException>();

        foreach (var inputSqlFile in allSqlFilesContent.OrderBy(f => f.Path))
        {
            var content = IgnoreComments(inputSqlFile.Content);

            var statements =
                GetDdlTableStatements(GetFilePath(scriptDirectory, inputSqlFile), content, parserExceptions);

            foreach (var ddlTableStatement in statements)
            {
                switch (ddlTableStatement)
                {
                    case CreateTable a:
                        tables.Add(a.Table, new ColumnsCollection(a.ColumnDefinitions, a.ConstraintDefinitions));
                        break;
                    case AlterTable a:
                        if (!tables.TryGetValue(a.Table, out var table))
                        {
                            throw new ArgumentException($"Table {a.Table} not found");
                        }

                        var (newTableName, newColumns) = AlterColumns(table, a);
                        tables.Remove(a.Table);
                        tables[newTableName] = newColumns;
                        break;
                    case DropTable a:
                        tables.Remove(a.Table);

                        break;
                }
            }
        }

        return (new Schema(scriptDirectory, tables
            .Select(e => new Table(e.Key, e.Value.Columns.ToImmutableList(), e.Value.GetPrimaryKeys()))
            .ToImmutableList()), parserExceptions.ToImmutable());
    }

    private static string GetFilePath(string scriptDirectory, InputFile inputSqlFile) =>
        scriptDirectory != inputSqlFile.Path ? $"{scriptDirectory}/{inputSqlFile.Path}" : inputSqlFile.Path;

    private static ImmutableList<DdlTableStatement> GetDdlTableStatements(string filePath, string content,
        ImmutableList<SqlParserException>.Builder parsingExceptions)
    {
        try
        {
            return Parser.Parser.DdlTableStatements.Parse(content);
        }
        catch (ParseException pe)
        {
            parsingExceptions.Add(new SqlParserException(
                Location.Create(filePath, default,
                    new LinePositionSpan(new LinePosition(pe.Position.Line - 1, pe.Position.Column),
                        new LinePosition(pe.Position.Line - 1, pe.Position.Column))), pe.Message));
            return ImmutableList<DdlTableStatement>.Empty;
        }
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
                    if (!columns.Alter(a.Column, a.AlterColumnAction))
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
                case AddConstraint a:
                    if (a.ConstraintDefinition.ColumnConstraint is PrimaryKeyConstraint _)
                    {
                        columns.AddConstraint(a.ConstraintDefinition);
                    }

                    break;
                case DropConstraint d:
                    columns.DropConstraint(table, d.Identifier);

                    break;
            }
        }

        return (table, columns);
    }
}