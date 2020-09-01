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
            var index = GetIndex(column);
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

        public bool Alter(string column, NotNullAction notNullAction)
        {
            var index = GetIndex(column);
            if (index == -1)
            {
                return false;
            }

            var previous = Columns[index];
            Columns[index] = new Column(previous.Name, previous.Type, notNullAction == NotNullAction.DropNotNull,
                previous.IsPrimaryKey, previous.IsAutoIncrement);

            return true;
        }

        private int GetIndex(string column)
        {
            return Columns.FindIndex(c => c.Name.ToUpper().Equals(column.ToUpper()));
        }
    }
}