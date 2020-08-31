using System.Collections.Generic;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast.Constraint;

namespace DbModelGenerator.Parser.Ast
{
    public sealed class ColumnDefinition : CreateTableStatement
    {
        public ColumnDefinition(string identifier, string type, string attributes)
        {
            Identifier = identifier;
            Type = type;
            Attributes = attributes ?? "";
        }

        public string Identifier { get; }
        public string Type { get; }
        public string Attributes { get; }

        public Column ToColumn(List<ColumnConstraint> constraints)
        {
            var regex = new Regex(@"NOT\s+NULL", RegexOptions.IgnoreCase);
            var isNotNull = regex.IsMatch(Attributes);
            var isPrimaryKey = constraints.Exists(c =>
            {
                if (c is PrimaryKeyConstraint constraint)
                {
                    return constraint.Columns.Contains(Identifier.ToUpper());
                }

                return false;
            }) || new Regex(@"PRIMARY KEY", RegexOptions.IgnoreCase).IsMatch(Attributes);
            return new Column(Identifier, ColumnParser.ParseType(Type), !isNotNull, isPrimaryKey,
                Type.ToUpper().Equals("SERIAL") || Attributes.ToUpper().Contains("AUTO_INCREMENT"));
        }

        private bool Equals(ColumnDefinition other)
        {
            return Identifier == other.Identifier && Type == other.Type && Attributes == other.Attributes;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ColumnDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Identifier != null ? Identifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}