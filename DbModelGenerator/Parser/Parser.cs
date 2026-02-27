using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using DbModelGenerator.Parser.Ast;
using DbModelGenerator.Parser.Ast.Alter;
using DbModelGenerator.Parser.Ast.Constraint;
using DbModelGenerator.Parser.Ast.Create;
using Sprache;

namespace DbModelGenerator.Parser;

public static class Parser
{
    private static readonly Parser<char> SeparatorChar =
        Parse.Chars("()@,;:\\/[]?{} \t\n");

    private static readonly Parser<char> ControlChar =
        Parse.Char(char.IsControl, "Control character");

    private static readonly Parser<char> TokenChar =
        Parse.AnyChar
            .Except(SeparatorChar)
            .Except(ControlChar);

    private static readonly Parser<string> Token =
        TokenChar.AtLeastOnce().Text().Token();

    private static readonly Parser<string> IdentifierName =
        from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase)).Token()
        where i.ToUpper() != "PRIMARY"
              && i.ToUpper() != "UNIQUE"
              && i.ToUpper() != "CONSTRAINT"
              && i.ToUpper() != "TABLE"
              && i.ToUpper() != "COLUMN"
              && i.ToUpper() != "IF"
              && i.ToUpper() != "EXISTS"
        select i;

    private static readonly Parser<string> SimpleQuoteDelimitedIdentifierName =
        from o in Parse.Char('\'')
        from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
        from c in Parse.Char('\'')
        select i;

    private static readonly Parser<string> DoubleQuoteDelimitedIdentifierName =
        from o in Parse.Char('"')
        from i in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
        from c in Parse.Char('"')
        select i;

    private static readonly Parser<string> Identifier =
        from i in IdentifierName
            .Or(SimpleQuoteDelimitedIdentifierName)
            .Or(DoubleQuoteDelimitedIdentifierName)
            .Token()
        select i;

    private static readonly Parser<string> Expression =
        from open in Parse.Char('(')
        from e in ExpressionOrLiteral
        from close in Parse.Char(')')
        select open + e + close;

    private static readonly Parser<string> ExpressionOrLiteral =
        from attributes in Expression
            .Or(Token)
            .Many()
        select string.Join(" ", attributes);

    private static readonly Parser<string> Attributes =
        from leading in Parse.WhiteSpace.Many()
        from constraintIdentifier in ConstraintIdentifier.Optional()
        from attributes in ExpressionOrLiteral
        from trailing in Parse.WhiteSpace.Many()
        select string.Join(" ", attributes);

    private static readonly Parser<string> TypeInfo =
        from leading in Parse.WhiteSpace.Many()
        from openParenthesis in Parse.Char('(')
        from innerLeading in Parse.WhiteSpace.Many()
        from info in Parse.Regex(new Regex(@"[^)]+", RegexOptions.IgnoreCase))
        from innerTraziling in Parse.WhiteSpace.Many()
        from _ in Parse.WhiteSpace.Many()
        from closeParenthesis in Parse.Char(')')
        from trailing in Parse.WhiteSpace.Many()
        select info;

    private static readonly Parser<string> Type =
        from type in Parse.Regex(new Regex(@"\w+", RegexOptions.IgnoreCase))
        from info in TypeInfo.Optional()
        select type;

    public static readonly Parser<ColumnDefinition> ColumnDefinition =
        from identifier in Identifier
        from type in Type
        from a in Attributes.Optional()
        select new ColumnDefinition(identifier, type, a.GetOrDefault());

    private static readonly Parser<string> ConstraintIdentifier =
        from add in Parse.IgnoreCase("CONSTRAINT").Token()
        from ifExists in IfExists.Optional()
        from identifier in Identifier
        select identifier;

    public static readonly Parser<PrimaryKeyConstraint> PrimaryKeyConstraint =
        from type in Parse.Regex(new Regex(@"PRIMARY\s+KEY", RegexOptions.IgnoreCase)).Token()
        from open in Parse.Char('(').Token()
        from columns in Identifier.DelimitedBy(Parse.Char(',').Token())
        from close in Parse.Char(')').Token()
        select new PrimaryKeyConstraint(columns.Select(c => c.ToUpper())
            .ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase));

    private static readonly Parser<UniqueConstraint> UniqueConstraint =
        from type in Parse.IgnoreCase("UNIQUE").Token()
        from open in Parse.Char('(').Token()
        from columns in Identifier.DelimitedBy(Parse.Char(',').Token())
        from close in Parse.Char(')').Token()
        select new UniqueConstraint(columns.ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase));

    private static readonly Parser<ForeignKeyConstraint> ForeignKeyConstraint =
        from type in Parse.Regex(new Regex(@"FOREIGN\s+KEY", RegexOptions.IgnoreCase))
        from separator in Parse.WhiteSpace.Many()
        from open in Parse.Char('(')
        from separator2 in Parse.WhiteSpace.Many()
        from columns in Identifier.DelimitedBy(Parse.Char(','))
        from separator3 in Parse.WhiteSpace.Many()
        from close in Parse.Char(')')
        from separator4 in Parse.WhiteSpace.Many()
        from attributes in Attributes.Optional()
        select new ForeignKeyConstraint(columns.ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase),
            attributes.GetOrDefault() ?? "");

    private static readonly Parser<ColumnConstraint> ColumnConstraint =
        from c in PrimaryKeyConstraint
            .Or<ColumnConstraint>(UniqueConstraint)
            .Or(ForeignKeyConstraint)
        select c;

    private static readonly Parser<ConstraintDefinition> ConstraintDefinition =
        from identifier in ConstraintIdentifier.Optional()
        from separator in Parse.WhiteSpace.Many()
        from c in ColumnConstraint
        select new ConstraintDefinition(identifier.ToOption(), c);

    public static readonly Parser<AddColumn> AddColumn =
        from add in Parse.IgnoreCase("ADD").Token()
        from column in Parse.IgnoreCase("COLUMN").Token().Optional()
        from ifNotExists in IfNotExists.Optional()
        from columnDefinition in ColumnDefinition
        select new AddColumn(columnDefinition);

    public static readonly Parser<RenameColumn> RenameColumn =
        from action in Parse.IgnoreCase("RENAME").Token()
        from column in Parse.IgnoreCase("COLUMN").Token()
        from identifier in Identifier.Token()
        from to in Parse.IgnoreCase("TO").Token()
        from newName in Identifier
        select new RenameColumn(identifier, newName);

    private static readonly Parser<RenameTable> RenameTable =
        from action in Parse.IgnoreCase("RENAME").Token()
        from to in Parse.IgnoreCase("TO").Token()
        from newName in Identifier
        select new RenameTable(newName);

    private static readonly Parser<bool> IfExists =
        from i in Parse.IgnoreCase("IF").Token()
        from e in Parse.IgnoreCase("EXISTS").Token()
        select true;

    private static readonly Parser<bool> IfNotExists =
        from i in Parse.IgnoreCase("IF").Token()
        from n in Parse.IgnoreCase("NOT").Token()
        from e in Parse.IgnoreCase("EXISTS").Token()
        select true;

    public static readonly Parser<DropColumn> DropColumn =
        from action in Parse.IgnoreCase("DROP").Token()
        from column in Parse.IgnoreCase("COLUMN").Token().Optional()
        from ifExists in IfExists.Optional()
        from identifier in Identifier.Text()
        select new DropColumn(identifier);

    private static readonly Parser<AlterColumnAction> AlterColumnTypeAction =
        from t in Parse.IgnoreCase("TYPE").Token()
        from type in Type
        select AlterColumnAction.AlterType(type);

    private static readonly Parser<AlterColumnAction> NullAlterColumnAction =
        from action in Parse.IgnoreCase("SET")
            .Or(Parse.IgnoreCase("DROP"))
            .Token()
            .Text()
        from not in Parse.IgnoreCase("NOT").Token()
        from n in Parse.IgnoreCase("NULL").Token()
        select action.ToUpper().Equals("SET")
            ? AlterColumnAction.SetNotNull()
            : AlterColumnAction.DropNotNull();

    private static readonly Parser<AlterColumn> AlterColumn =
        from alter in Parse.IgnoreCase("ALTER").Token()
        from column in Parse.IgnoreCase("COLUMN").Token().Optional()
        from identifier in Identifier.Token()
        from action in NullAlterColumnAction.Or(AlterColumnTypeAction)
        select new AlterColumn(identifier, action);

    private static readonly Parser<AddConstraint> AddConstraint =
        from a in Parse.IgnoreCase("ADD").Token()
        from c in ConstraintDefinition
        select new AddConstraint(c);

    private static readonly Parser<DropConstraint> DropConstraint =
        from a in Parse.IgnoreCase("DROP").Token()
        from identifier in ConstraintIdentifier
        select new DropConstraint(identifier);

    private static readonly Parser<DdlAlterTableStatement> DdlAlterTableStatement =
        from c in DropColumn
            .Or<DdlAlterTableStatement>(RenameColumn)
            .Or(AddColumn)
            .Or(RenameTable)
            .Or(AlterColumn)
            .Or(AddConstraint)
            .Or(DropConstraint)
        select c;

    private static readonly Parser<CreateTableStatement> CreateTableStatement =
        from c in ConstraintDefinition
            .Or<CreateTableStatement>(ColumnDefinition)
        select c;

    public static readonly Parser<CreateTable> CreateTable =
        from action in Parse.IgnoreCase("CREATE")
        from separator in Parse.WhiteSpace.Many()
        from column in Parse.IgnoreCase("TABLE")
        from ifNotExists in IfNotExists.Optional()
        from separator1 in Parse.WhiteSpace.Many()
        from table in Identifier
        from separator2 in Parse.WhiteSpace.Many()
        from open in Parse.Char('(')
        from separator3 in Parse.WhiteSpace.Many()
        from statements in CreateTableStatement.DelimitedBy(Parse.Char(',').Token())
        from separator5 in Parse.WhiteSpace.Many()
        from close in Parse.Char(')')
        from separator6 in Parse.WhiteSpace.Many()
        let columnDefinitions = statements.Where(s => s is ColumnDefinition).Cast<ColumnDefinition>()
        let constraints = statements.Where(s => s is ConstraintDefinition).Cast<ConstraintDefinition>()
        select new CreateTable(columnDefinitions.ToImmutableList(), constraints.ToImmutableList(), table);

    public static readonly Parser<AlterTable> AlterTable =
        from action in Parse.IgnoreCase("ALTER").Token()
        from column in Parse.IgnoreCase("TABLE").Token()
        from table in Identifier.Token()
        from ddlAlterTableStatements in DdlAlterTableStatement.DelimitedBy(Parse.Char(','))
        select new AlterTable(table, ddlAlterTableStatements.ToImmutableList());

    private static readonly Parser<DropTable> DropTable =
        from action in Parse.IgnoreCase("DROP").Token()
        from column in Parse.IgnoreCase("TABLE").Token()
        from ifExists in IfExists.Optional()
        from table in Identifier.Token()
        select new DropTable(table);

    private static readonly Parser<DdlTableStatement> DdlTableStatement =
        from c in CreateTable
            .Or<DdlTableStatement>(AlterTable)
            .Or(DropTable)
        from separator2 in Parse.WhiteSpace.Many()
        select c;

    private static readonly Parser<string> NonDdlTableStatement =
        from action in Parse.IgnoreCase("CREATE")
            .Or(Parse.IgnoreCase("DROP"))
            .Or(Parse.IgnoreCase("ALTER")).Not()
        from _ in Parse.AnyChar.Except(Parse.Char(';')).Many()
        select "";

    public static readonly Parser<ImmutableList<DdlTableStatement>> DdlTableStatements =
        from separator in Parse.WhiteSpace.Many()
        from statements in DdlTableStatement
            .Or<object>(NonDdlTableStatement)
            .DelimitedBy(Parse.Regex(new Regex(@"\s*;\s*")))
        select statements.Where(s => s is DdlTableStatement)
            .Cast<DdlTableStatement>()
            .ToImmutableList();
}