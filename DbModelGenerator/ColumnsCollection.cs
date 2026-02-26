using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast.Alter;
using DbModelGenerator.Parser.Ast.Constraint;
using DbModelGenerator.Parser.Ast.Create;

namespace DbModelGenerator;

public sealed class ColumnsCollection
{
    public ColumnsCollection(ImmutableList<ColumnDefinition> columnDefinitions,
        ImmutableList<ConstraintDefinition> constraintDefinitions)
    {
        var constraints = constraintDefinitions.ToList();

        foreach (var column in columnDefinitions)
        {
            if (new Regex("PRIMARY KEY", RegexOptions.IgnoreCase).IsMatch(column.Attributes))
            {
                constraints.Add(new ConstraintDefinition(Option<string>.None(),
                    new PrimaryKeyConstraint(
                        new[] {column.Identifier}.ToImmutableSortedSet(StringComparer
                            .InvariantCultureIgnoreCase))));
            }
        }

        Columns = columnDefinitions
            .Select(c => c.ToColumn())
            .ToList();
        Constraints = constraints;
    }

    public List<Column> Columns { get; }

    private List<ConstraintDefinition> Constraints { get; }

    public bool Remove(string column)
    {
        var index = GetIndex(column);
        if (index == -1)
        {
            return false;
        }

        Columns.RemoveAt(index);
        return true;
    }

    public bool Rename(string column, string newName)
    {
        foreach (var constraintDefinition in Constraints)
        {
            constraintDefinition.ColumnConstraint.RenameColumn(column, newName);
        }

        var index = GetIndex(column);
        if (index == -1)
        {
            return false;
        }

        var existingColumn = Columns[index];
        Columns[index] = new Column(newName, existingColumn.Type, existingColumn.IsNullable,
            existingColumn.IsAutoIncrementByDefinition,
            existingColumn.IsAutoIncrementByType);
        return true;
    }

    public void Add(ColumnDefinition columnDefinition)
    {
        Columns.Add(columnDefinition.ToColumn());
    }

    public ImmutableSortedSet<string> GetPrimaryKeys()
    {
        return Columns
            .Select(c => c.Name)
            .Where(IsPrimaryKey)
            .ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase);
    }

    private bool IsPrimaryKey(string columnName)
    {
        var index = GetIndex(columnName);
        if (index == -1)
        {
            return false;
        }

        var column = Columns[index];
        return Constraints.Exists(c =>
        {
            if (c.ColumnConstraint is PrimaryKeyConstraint constraint)
            {
                return constraint.Columns.Contains(column.Name);
            }

            return false;
        });
    }

    public bool Alter(string column, AlterColumnAction alterColumnAction)
    {
        var index = GetIndex(column);
        if (index == -1)
        {
            return false;
        }

        var previous = Columns[index];
        var type = previous.Type;
        var isNullable = previous.IsNullable;
        var isAutoIncrementByType = previous.IsAutoIncrementByType;
        switch (alterColumnAction)
        {
            case DropNotNull _:
                isNullable = true;
                break;
            case SetNotNull _:
                isNullable = false;
                break;
            case AlterType a:
                isAutoIncrementByType = a.Type.ToUpper().Equals("SERIAL") || a.Type.ToUpper().Equals("BIGSERIAL");

                type = ColumnParser.ParseType(a.Type);
                break;
        }

        Columns[index] = new Column(previous.Name, type, isNullable,
            previous.IsAutoIncrementByDefinition, isAutoIncrementByType);
        return true;
    }

    private int GetIndex(string column)
    {
        return Columns.FindIndex(c => c.Name.ToUpper().Equals(column.ToUpper()));
    }

    public void DropConstraint(string table, string identifier)
    {
        var index = Constraints.FindIndex(c =>
        {
            var name = GetNameOrInfer(table, c);
            return name.Equals(identifier, StringComparison.InvariantCultureIgnoreCase);
        });
        if (index == -1)
        {
            return;
        }

        Constraints.RemoveAt(index);
    }

    private static string GetNameOrInfer(string table, ConstraintDefinition constraint)
    {
        if (!constraint.Identifier.IsEmpty)
        {
            return constraint.Identifier.Get();
        }

        switch (constraint.ColumnConstraint)
        {
            case PrimaryKeyConstraint p:
                return $"{table}_{string.Join("_", p.Columns)}_pkey";
        }

        return "undefined";
    }

    public void AddConstraint(ConstraintDefinition constraintDefinition)
    {
        Constraints.Add(constraintDefinition);
    }
}