using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast;
using DbModelGenerator.Parser.Ast.Constraint;
using Sprache;

namespace DbModelGenerator.Parser
{
    public static class Parser
    {
        public static readonly Parser<string> IdentifierName =
            from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
            where i.ToUpper() != "PRIMARY"
                  && i.ToUpper() != "PRIMARY"
                  && i.ToUpper() != "CONSTRAINT"
                  && i.ToUpper() != "TABLE"
                  && i.ToUpper() != "COLUMN"
            select i;

        public static readonly Parser<string> SimpleQuoteDelimitedIdentifierName =
            from o in Parse.Char('\'')
            from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
            from c in Parse.Char('\'')
            select i;

        public static readonly Parser<string> DoubleQuoteDelimitedIdentifierName =
            from o in Parse.Char('"')
            from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
            from c in Parse.Char('"')
            select i;

        public static readonly Parser<string> Identifier =
            from leading in Parse.WhiteSpace.Many()
            from i in IdentifierName.Or(SimpleQuoteDelimitedIdentifierName).Or(DoubleQuoteDelimitedIdentifierName)
            from trailing in Parse.WhiteSpace.Many()
            select i;

        public static readonly Parser<string> Attributes =
            from leading in Parse.WhiteSpace.Many()
            from attributes in Parse.Regex(new Regex(@"[^,);]+", RegexOptions.IgnoreCase))
            from trailing in Parse.WhiteSpace.Many()
            select attributes;

        public static readonly Parser<string> TypeInfo =
            from leading in Parse.WhiteSpace.Many()
            from openParenthesis in Parse.Char('(')
            from innerLeading in Parse.WhiteSpace.Many()
            from info in Parse.Regex(new Regex(@"[^)]+", RegexOptions.IgnoreCase))
            from innerTraziling in Parse.WhiteSpace.Many()
            from _ in Parse.WhiteSpace.Many()
            from closeParenthesis in Parse.Char(')')
            from trailing in Parse.WhiteSpace.Many()
            select info;

        public static readonly Parser<string> Type =
            from type in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
            from info in TypeInfo.Optional()
            select type;

        public static readonly Parser<ColumnDefinition> ColumnDefinition =
            from identifier in Identifier
            from type in Type
            from a in Attributes.Optional()
            select new ColumnDefinition(identifier, type, a.GetOrDefault());

        public static readonly Parser<string> ConstraintIdentifier =
            from add in Parse.IgnoreCase("CONSTRAINT")
            from sperator in Parse.WhiteSpace.Many()
            from identifier in Identifier
            select identifier;

        public static readonly Parser<PrimaryKeyConstraint> PrimaryKeyConstraint =
            from type in Parse.Regex(new Regex(@"PRIMARY\s+KEY", RegexOptions.IgnoreCase))
            from sperator in Parse.WhiteSpace.Many()
            from open in Parse.Char('(')
            from sperator2 in Parse.WhiteSpace.Many()
            from columns in Identifier.DelimitedBy(Parse.Char(','))
            from sperator3 in Parse.WhiteSpace.Many()
            from close in Parse.Char(')')
            select new PrimaryKeyConstraint(columns.Select(c => c.ToUpper()).ToImmutableList());

        public static readonly Parser<UniqueConstraint> UniqueConstraint =
            from type in Parse.IgnoreCase("UNIQUE")
            from sperator in Parse.WhiteSpace.Many()
            from open in Parse.Char('(')
            from sperator2 in Parse.WhiteSpace.Many()
            from columns in Identifier.DelimitedBy(Parse.Char(','))
            from sperator3 in Parse.WhiteSpace.Many()
            from close in Parse.Char(')')
            select new UniqueConstraint(columns.ToImmutableList());

        public static readonly Parser<ForeignKeyConstraint> ForeignKeyConstraint =
            from type in Parse.Regex(new Regex(@"FOREIGN\s+KEY", RegexOptions.IgnoreCase))
            from sperator in Parse.WhiteSpace.Many()
            from open in Parse.Char('(')
            from sperator2 in Parse.WhiteSpace.Many()
            from columns in Identifier.DelimitedBy(Parse.Char(','))
            from sperator3 in Parse.WhiteSpace.Many()
            from close in Parse.Char(')')
            from sperator4 in Parse.WhiteSpace.Many()
            from attributes in Attributes.Optional()
            select new ForeignKeyConstraint(columns.ToImmutableList(), attributes.GetOrDefault() ?? "");

        public static readonly Parser<ColumnConstraint> ColumnConstraint =
            from c in PrimaryKeyConstraint
                .Or<ColumnConstraint>(UniqueConstraint)
                .Or(ForeignKeyConstraint)
            select c;

        public static readonly Parser<ConstraintDefinition> ConstraintDefinition =
            from identifier in ConstraintIdentifier.Optional()
            from sperator in Parse.WhiteSpace.Many()
            from c in ColumnConstraint
            select new ConstraintDefinition(identifier.GetOrDefault(), c);

        public static readonly Parser<ImmutableList<ConstraintDefinition>> ConstraintDefinitions =
            from close in Parse.Char(',')
            from sperator in Parse.WhiteSpace.Many()
            from constraints in ConstraintDefinition.DelimitedBy(Parse.Char(','))
            select constraints.ToImmutableList();

        public static readonly Parser<AddColumn> AddColumn =
            from add in Parse.IgnoreCase("ADD")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("COLUMN")
            from sperator2 in Parse.WhiteSpace.Many()
            from columnDefinition in ColumnDefinition
            select new AddColumn(columnDefinition);

        public static readonly Parser<RenameColumn> RenameColumn =
            from action in Parse.IgnoreCase("RENAME")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("COLUMN")
            from sperator1 in Parse.WhiteSpace.Many()
            from identifier in Identifier
            from sperator2 in Parse.WhiteSpace.Many()
            from to in Parse.IgnoreCase("TO")
            from sperator3 in Parse.WhiteSpace.Many()
            from newName in Identifier
            select new RenameColumn(identifier, newName);

        public static readonly Parser<DropColumn> DropColumn =
            from action in Parse.IgnoreCase("DROP")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("COLUMN")
            from sperator1 in Parse.WhiteSpace.Many()
            from identifier in Identifier.Text()
            select new DropColumn(identifier);

        public static readonly Parser<DdlColumnStatement> DdlColumnStatement =
            from c in DropColumn
                .Or<DdlColumnStatement>(RenameColumn)
                .Or(AddColumn)
            select c;

        public static readonly Parser<CreateTableStatement> CreateTableStatement =
            from c in ConstraintDefinition
                .Or<CreateTableStatement>(ColumnDefinition)
            select c;

        public static readonly Parser<CreateTable> CreateTable =
            from action in Parse.IgnoreCase("CREATE")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("TABLE")
            from sperator1 in Parse.WhiteSpace.Many()
            from table in Identifier
            from sperator2 in Parse.WhiteSpace.Many()
            from open in Parse.Char('(')
            from sperator3 in Parse.WhiteSpace.Many()
            from statements in CreateTableStatement.DelimitedBy(Parse.Char(','))
            from sperator5 in Parse.WhiteSpace.Many()
            from close in Parse.Char(')')
            from sperator6 in Parse.WhiteSpace.Many()
            let columnDefinitions = statements.Where(s => s is ColumnDefinition).Cast<ColumnDefinition>()
            let constraints = statements.Where(s => s is ConstraintDefinition).Cast<ConstraintDefinition>()
            select new CreateTable(columnDefinitions.ToImmutableList(), constraints.ToImmutableList(), table);

        public static readonly Parser<AlterTable> AlterTable =
            from action in Parse.IgnoreCase("ALTER")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("TABLE")
            from sperator1 in Parse.WhiteSpace.Many()
            from table in Identifier
            from sperator2 in Parse.WhiteSpace.Many()
            from ddlColumnStatement in DdlColumnStatement
            select new AlterTable(table, ddlColumnStatement);

        public static readonly Parser<DropTable> DropTable =
            from action in Parse.IgnoreCase("DROP")
            from sperator in Parse.WhiteSpace.Many()
            from column in Parse.IgnoreCase("TABLE")
            from sperator1 in Parse.WhiteSpace.Many()
            from table in Identifier
            select new DropTable(table);

        public static readonly Parser<DdlTableStatement> DdlTableStatement =
            from c in CreateTable
                .Or<DdlTableStatement>(AlterTable)
                .Or(DropTable)
            from sperator2 in Parse.WhiteSpace.Many()
            select c;

        public static readonly Parser<string> NonDdlTableStatement = Parse.Regex(new Regex("[^;]+")).Text();

        public static readonly Parser<ImmutableList<DdlTableStatement>> DdlTableStatements =
            from sperator in Parse.WhiteSpace.Many()
            from statements in DdlTableStatement
                .Or<object>(NonDdlTableStatement)
                .DelimitedBy(Parse.Regex(new Regex(@"\s*;\s*")))
            select statements.Where(s => s is DdlTableStatement)
                .Cast<DdlTableStatement>()
                .ToImmutableList();
    }
}