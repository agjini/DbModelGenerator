using System.Collections.Generic;
using DbModelGenerator.Parser.Ast;
using DbModelGenerator.Parser.Ast.Constraint;

namespace DbModelGenerator
{
    public sealed class ColumnsCollection
    {
        public ColumnsCollection(List<Column> columns, List<ColumnConstraint> constraints)
        {
            Columns = columns;
            Constraints = constraints;
        }

        public List<Column> Columns { get; }

        public List<ColumnConstraint> Constraints { get; }

        public bool Remove(string column)
        {
            var index = Columns.FindIndex(c => c.Name.ToUpper().Equals(column.ToUpper()));
            if (index == -1)
            {
                return false;
            }

            Columns.RemoveAt(index);
            return true;
        }

        public bool Rename(string column, string newName)
        {
            var index = Columns.FindIndex(c => c.Name.ToUpper().Equals(column.ToUpper()));
            if (index == -1)
            {
                return false;
            }

            var existingColumn = Columns[index];
            Columns[index] = new Column(newName, existingColumn.Type, existingColumn.IsNullable,
                existingColumn.IsPrimaryKey, existingColumn.IsAutoIncrement);
            return true;
        }

        public void Add(ColumnDefinition columnDefinition)
        {
            Columns.Add(columnDefinition.ToColumn(Constraints));
        }
    }
}