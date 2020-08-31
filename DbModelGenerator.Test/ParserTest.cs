using System.Collections.Immutable;
using System.Linq;
using DbModelGenerator.Parser.Ast;
using DbModelGenerator.Parser.Ast.Constraint;
using NUnit.Framework;
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
            Assert.AreEqual("id", columnDefinition.Identifier);
            Assert.AreEqual("INT", columnDefinition.Type);
            Assert.AreEqual("", columnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseColumnDefinitionWithAttributes()
        {
            var columnDefinition = Parser.Parser.ColumnDefinition.Parse("   id  INT  noT NULL  DEFAULT 10.4   \n");
            Assert.AreEqual("id", columnDefinition.Identifier);
            Assert.AreEqual("INT", columnDefinition.Type);
            Assert.AreEqual("noT NULL DEFAULT 10.4", columnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseAddColumn()
        {
            var addColumn = Parser.Parser.AddColumn.Parse("ADD COlumn  id  INT noT NULL  DEFAULT '0'");
            Assert.AreEqual("id", addColumn.ColumnDefinition.Identifier);
            Assert.AreEqual("INT", addColumn.ColumnDefinition.Type);
            Assert.AreEqual("noT NULL DEFAULT '0'", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseComplexColumnDefinition()
        {
            var actual =
                Parser.Parser.ColumnDefinition.Parse(
                    "user_group_id                 INT          NOT NULL REFERENCES \"user_group\" (id)");
            Assert.AreEqual("user_group_id", actual.Identifier);
            Assert.AreEqual("INT", actual.Type);
            Assert.AreEqual("NOT NULL REFERENCES \"user_group\" (id)", actual.Attributes);
        }

        [Test]
        public void ShouldParseAddColumns()
        {
            var addColumn =
                Parser.Parser.AddColumn.Parse("ADD COlumn  id  INT \n\t NULL  DEFAULT (233 ) CHECK (id > 0)");
            Assert.AreEqual("id", addColumn.ColumnDefinition.Identifier);
            Assert.AreEqual("INT", addColumn.ColumnDefinition.Type);
            Assert.AreEqual("NULL DEFAULT (233) CHECK (id > 0)", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseRenameColumn()
        {
            var tested = Parser.Parser.RenameColumn.Parse("rename COlumn  id  TO  vignemale");
            Assert.AreEqual("id", tested.Column);
            Assert.AreEqual("vignemale", tested.NewName);
        }

        [Test]
        public void ShouldParseDropColumn()
        {
            var tested = Parser.Parser.DropColumn.Parse("dRoP  \n COlumn id");
            Assert.AreEqual("id", tested.Column);
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
            Assert.AreEqual("brand", tested.Table);
            Assert.AreEqual(6, tested.ColumnDefinitions.Count);

            CollectionAssert.AreEqual(
                ImmutableList.Create(
                    new ColumnDefinition("id", "SERIAL", "NOT NULL"),
                    new ColumnDefinition("name", "VARCHAR", "NOT NULL"),
                    new ColumnDefinition("logo", "VARCHAR", ""),
                    new ColumnDefinition("archived", "BOOLEAN", "DEFAULT '0'"),
                    new ColumnDefinition("color", "VARCHAR", "NOT NULL"),
                    new ColumnDefinition("external_id", "VARCHAR", "")
                ), tested.ColumnDefinitions);


            CollectionAssert.AreEqual(
                ImmutableList.Create(
                    new ConstraintDefinition(null, new PrimaryKeyConstraint(ImmutableList.Create("ID"))),
                    new ConstraintDefinition("bibi_uk", new UniqueConstraint(ImmutableList.Create("name")))
                ), tested.ConstraintDefinitions);
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
            Assert.AreEqual(2, tested.Count());
        }

        [Test]
        public void ShouldParseAlterTableWithAddColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand ADD   
        ColumN    name        VARCHAR(50) 
        NOT NULL
            ");
            Assert.AreEqual("brand", tested.Table);
            Assert.IsInstanceOf<AddColumn>(tested.DdlColumnStatement);
            var addColumn = tested.DdlColumnStatement as AddColumn;

            Assert.AreEqual("name", addColumn!.Column);
            Assert.AreEqual("name", addColumn.ColumnDefinition.Identifier);
            Assert.AreEqual("VARCHAR", addColumn.ColumnDefinition.Type);
            Assert.AreEqual("NOT NULL", addColumn.ColumnDefinition.Attributes);
        }

        [Test]
        public void ShouldParseAlterTableWithDropColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand DROP COLUMN name");
            Assert.AreEqual("brand", tested.Table);
            Assert.IsInstanceOf<DropColumn>(tested.DdlColumnStatement);

            Assert.AreEqual("name", tested.DdlColumnStatement.Column);
        }

        [Test]
        public void ShouldParseAlterTableWithRenameColumn()
        {
            var tested = Parser.Parser.AlterTable.Parse(@"alTer    
    tablE brand RENAME   
        ColumN    name      tO bo_bo
            ");
            Assert.AreEqual("brand", tested.Table);
            Assert.IsInstanceOf<RenameColumn>(tested.DdlColumnStatement);

            var renameColumn = tested.DdlColumnStatement as RenameColumn;

            Assert.AreEqual("name", renameColumn!.Column);
            Assert.AreEqual("bo_bo", renameColumn.NewName);
        }
    }
}