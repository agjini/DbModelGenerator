using System;
using System.Collections.Immutable;
using System.Linq;
using DbModelGenerator.Parser.Ast.Alter;
using DbModelGenerator.Parser.Ast.Constraint;
using DbModelGenerator.Parser.Ast.Create;
using DeepEqual.Syntax;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Sprache;

namespace DbModelGenerator.Test
{
    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void ShouldParseColumnDefinition()
        {
            var columnDefinition = Parser.Parser.ColumnDefinition.Parse("   id  INT ");
            ClassicAssert.AreEqual("id", columnDefinition.Identifier);
            ClassicAssert.AreEqual("INT", columnDefinition.Type);
            ClassicAssert.AreEqual("", columnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseColumnDefinitionWithAttributes()
        {
            var columnDefinition = Parser.Parser.ColumnDefinition.Parse("   id  INT  noT NULL  DEFAULT 10.4   \n");
            ClassicAssert.AreEqual("id", columnDefinition.Identifier);
            ClassicAssert.AreEqual("INT", columnDefinition.Type);
            ClassicAssert.AreEqual("noT NULL DEFAULT 10.4", columnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseAddColumn()
        {
            var addColumn = Parser.Parser.AddColumn.Parse("ADD COlumn  id  INT noT NULL  DEFAULT '0'");
            ClassicAssert.AreEqual("id", addColumn.ColumnDefinition.Identifier);
            ClassicAssert.AreEqual("INT", addColumn.ColumnDefinition.Type);
            ClassicAssert.AreEqual("noT NULL DEFAULT '0'", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseComplexColumnDefinition()
        {
            var actual =
                Parser.Parser.ColumnDefinition.Parse(
                    "user_group_id                 INT          NOT NULL REFERENCES \"user_group\" (id)");
            ClassicAssert.AreEqual("user_group_id", actual.Identifier);
            ClassicAssert.AreEqual("INT", actual.Type);
            ClassicAssert.AreEqual("NOT NULL REFERENCES \"user_group\" (id)", actual.Attributes);
        }

        [Test]
        public void ShouldParseAddColumns()
        {
            var addColumn =
                Parser.Parser.AddColumn.Parse("ADD COlumn  id  INT \n\t NULL  DEFAULT (233 ) CHECK (id > 0)");
            ClassicAssert.AreEqual("id", addColumn.ColumnDefinition.Identifier);
            ClassicAssert.AreEqual("INT", addColumn.ColumnDefinition.Type);
            ClassicAssert.AreEqual("NULL DEFAULT (233) CHECK (id > 0)", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseRenameColumn()
        {
            var tested = Parser.Parser.RenameColumn.Parse("rename COlumn  id  TO  vignemale");
            ClassicAssert.AreEqual("id", tested.Column);
            ClassicAssert.AreEqual("vignemale", tested.NewName);
        }

        [Test]
        public void ShouldParseDropColumn()
        {
            var tested = Parser.Parser.DropColumn.Parse("dRoP  \n COlumn id");
            ClassicAssert.AreEqual("id", tested.Column);
        }

        [Test]
        public void ShouldParseCreateTable()
        {
            var tested = Parser.Parser.CreateTable.Parse(@"CREATE TABLE brand
            (
                id          SERIAL       NOT NULL,
                name        VARCHAR(50)  NOT NULL,
                logo        VARCHAR(200),
                archived    BOOLEAN DEFAULT '0',
                color       VARCHAR(100) NOT NULL,
                external_id VARCHAR(500),
                PRIMARY KEY (id),
                CONSTRAINT   bibi_uk   UNIQUE (name)
            )
            ");
            ClassicAssert.AreEqual("brand", tested.Table);
            ClassicAssert.AreEqual(6, tested.ColumnDefinitions.Count);

            CollectionAssert.AreEqual(
                ImmutableList.Create(
                    new ColumnDefinition("id", "SERIAL", "NOT NULL"),
                    new ColumnDefinition("name", "VARCHAR", "NOT NULL"),
                    new ColumnDefinition("logo", "VARCHAR", ""),
                    new ColumnDefinition("archived", "BOOLEAN", "DEFAULT '0'"),
                    new ColumnDefinition("color", "VARCHAR", "NOT NULL"),
                    new ColumnDefinition("external_id", "VARCHAR", "")
                ), tested.ColumnDefinitions);

            tested.ConstraintDefinitions.ShouldDeepEqual(ImmutableList.Create(
                new ConstraintDefinition(Option<string>.None(),
                    new PrimaryKeyConstraint(ImmutableSortedSet.Create(StringComparer.CurrentCultureIgnoreCase, "ID"))),
                new ConstraintDefinition("bibi_uk".ToOption(),
                    new UniqueConstraint(ImmutableSortedSet.Create(StringComparer.CurrentCultureIgnoreCase, "name")))
            ));
        }

        [Test]
        public void ShouldParsePrimaryKey()
        {
            var tested = Parser.Parser.PrimaryKeyConstraint.Parse(@"PRIMARY KEY (id)");
            CollectionAssert.AreEquivalent(ImmutableList.Create("ID"), tested.Columns);
        }

        [Test]
        public void ShouldParseSeveralColumnDefinition()
        {
            var tested = Parser.Parser.ColumnDefinition
                .DelimitedBy(Parse.Char(','))
                .Parse(@"id          SERIAL       NOT NULL,
    name        VARCHAR(50)  NOT NULL");
            ClassicAssert.AreEqual(2, tested.Count());
        }

        [Test]
        public void ShouldParseAlterTableWithAddColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand ADD   
        ColumN    name        VARCHAR(50) 
        NOT NULL
            ");
            ClassicAssert.AreEqual("brand", tested.Table);
            ClassicAssert.AreEqual(1, tested.DdlAlterTableStatements.Count);
            ClassicAssert.IsInstanceOf<AddColumn>(tested.DdlAlterTableStatements[0]);
            var addColumn = tested.DdlAlterTableStatements[0] as AddColumn;

            ClassicAssert.AreEqual("name", addColumn!.Column);
            ClassicAssert.AreEqual("name", addColumn.ColumnDefinition.Identifier);
            ClassicAssert.AreEqual("VARCHAR", addColumn.ColumnDefinition.Type);
            ClassicAssert.AreEqual("NOT NULL", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseAlterTableWithDropColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand DROP COLUMN name");
            ClassicAssert.AreEqual("brand", tested.Table);
            ClassicAssert.AreEqual(1, tested.DdlAlterTableStatements.Count);
            ClassicAssert.IsInstanceOf<DropColumn>(tested.DdlAlterTableStatements[0]);

            ClassicAssert.AreEqual("name", ((DropColumn) tested.DdlAlterTableStatements[0]).Column);
        }

        [Test]
        public void ShouldParseAlterTableWithAlterColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand alter
        
        COLUMN name set noT
        NUll");
            ClassicAssert.AreEqual("brand", tested.Table);
            ClassicAssert.AreEqual(1, tested.DdlAlterTableStatements.Count);
            ClassicAssert.IsInstanceOf<AlterColumn>(tested.DdlAlterTableStatements[0]);

            ClassicAssert.AreEqual("name", ((AlterColumn) tested.DdlAlterTableStatements[0]).Column);
            ClassicAssert.IsInstanceOf<SetNotNull>(((AlterColumn) tested.DdlAlterTableStatements[0]).AlterColumnAction);
        }

        [Test]
        public void ShouldParseAlterTableWithRenameColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand RENAME   
        ColumN    name      tO bo_bo
            ");
            ClassicAssert.AreEqual("brand", tested.Table);
            ClassicAssert.AreEqual(1, tested.DdlAlterTableStatements.Count);
            ClassicAssert.IsInstanceOf<RenameColumn>(tested.DdlAlterTableStatements[0]);

            var renameColumn = tested.DdlAlterTableStatements[0] as RenameColumn;

            ClassicAssert.AreEqual("name", renameColumn!.Column);
            ClassicAssert.AreEqual("bo_bo", renameColumn.NewName);
        }
    }
}