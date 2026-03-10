using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework.Legacy;
using Xunit;
using Assert = Xunit.Assert;

namespace DbModelGenerator.Test;

public class TestAdditionalFile(string path, string text) : AdditionalText
{
    private readonly SourceText _text = SourceText.From(text);

    public override SourceText GetText(CancellationToken cancellationToken = new()) => _text;

    public override string Path { get; } = path;
}

public sealed class SourceGeneratorTest
{
    private const string WrongConfig = @"
        ""primaryKeyAttribute"": ""Example.Service.PrimaryKey"",
          ""autoIncrementAttribute"": ""Example.Service.AutoIncrement"",
          ""suffix"": ""Db"",
          ""ignores"": [
            ""role""
          ]
        }
    ";

    private const string Config = @"
        {
          ""primaryKeyAttribute"": ""Example.Service.PrimaryKey"",
          ""autoIncrementAttribute"": ""Example.Service.AutoIncrement"",
          ""suffix"": ""Db"",
          ""ignores"": [
            ""role""
          ]
        }
    ";

    private const string CreateTableBrand = @"
        CREATE TABLE brand
        (
            id          SERIAL       NOT NULL,
            name        VARCHAR(50)  NOT NULL,
            logo        VARCHAR(200),
            archived    BOOLEAN DEFAULT '0',
            color       VARCHAR(100) NOT NULL,
            external_id VARCHAR(500),
            PRIMARY KEY (id),
            UNIQUE (name)
        );
    ";

    private const string CreateTableUser = @"
        CREATE TABLE user
        (
            id          SERIAL       NOT NULL,
            name        VARCHAR(50)  NOT NULL,
            PRIMARY KEY (id),
            UNIQUE (name)
        );
    ";

    private const string AlterTableBrand = @"
        ALTER TABLE brand ADD COLUMN company_id INTEGER REFERENCES referential (id);
    ";


    private const string InsertTable = @"
        INSERT INTO country (id, name, code, default_currency_id)
        VALUES (1, 'Belgium', 'BE', '1'),
               (2, 'France', '$TEST_VAR$', '1'),
               (4, 'Luxembourg', 'LU', '1'),
               (5, 'Netherlands', 'NL', '1');
    ";

    private const string DropTableIfExists = "DROP TABLE IF EXISTS tenant_saml;";

    private const string WrongCreateTable = @"
        CREATE TABLE brand
        (
            id          SERIAL       NOT NULL,
            name        VARCHAR(50)  NOT NULL,
            logo        VARCHAR(200),
            archived    BOOLEAN DEFAULT '0',
            color       VARCHAR(100) NOT NULL,
            external_id VARCHAR(500),
            PRIMARY KEY (id),
            UNIQUE (name)
       ";

    private const string WrongInsertTable = @"
        INSERT INTO brand VALUES (
           
       ";

    private const string WrongAlterTable = @"
        ALTER TABLE brand
        (
           
       ";

    private const string WrongDropTableIfExists = "DROP TABLE IF tenant_saml;";


    private GeneratorDriver _driver;

    public SourceGeneratorTest()
    {
        var generator = new GenerateDbModel();
        var options = new Dictionary<string, string>
        {
            ["build_property.projectdir"] = "/home/test/DbModelGenerator.Test/"
        };
        _driver = CSharpGeneratorDriver.Create(generator)
            .WithUpdatedAnalyzerConfigOptions(CompilerAnalyzerConfigOptionsProvider.WithOptions(options));
    }

    [Fact]
    public void WrongFormattedJson()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", WrongConfig),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", CreateTableBrand),
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        CollectionAssert.IsNotEmpty(diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
        Assert.Equal("DbMG001", diagnostics[0].Id);

        CollectionAssert.IsEmpty(generatedFiles);
    }

    [Fact]
    public void GenerateDbModel()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", Config),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_1.sql", CreateTableBrand),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", InsertTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_3.sql", DropTableIfExists)
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        CollectionAssert.AreEquivalent(new[]
        {
            "BrandDb.cs"
        }, generatedFiles);
    }

    [Fact]
    public void OneParsingErrorFormatting()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", Config),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_1.sql", WrongCreateTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", InsertTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_3.sql", DropTableIfExists)
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        CollectionAssert.IsNotEmpty(diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
        Assert.Equal("DbMG002", diagnostics[0].Id);
        Assert.Equal("ExternalFile(/home/test/DbModelGenerator.Test/Scripts/script_1.sql@12:9)",
            diagnostics[0].Location.ToString());

        CollectionAssert.IsEmpty(generatedFiles);
    }

    [Fact]
    public void MultipleParsingErrorsFormatting()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", Config),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_1.sql", WrongCreateTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", WrongAlterTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_3.sql", WrongInsertTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_4.sql", WrongDropTableIfExists)
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        CollectionAssert.IsNotEmpty(diagnostics);

        Assert.Equal(3, diagnostics.Length);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
        Assert.Equal("DbMG002", diagnostics[0].Id);
        Assert.Equal("ExternalFile(/home/test/DbModelGenerator.Test/Scripts/script_1.sql@12:9)",
            diagnostics[0].Location.ToString());

        Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
        Assert.Equal("DbMG002", diagnostics[1].Id);
        Assert.Equal("ExternalFile(/home/test/DbModelGenerator.Test/Scripts/script_2.sql@3:10)",
            diagnostics[1].Location.ToString());

        Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
        Assert.Equal("DbMG002", diagnostics[2].Id);
        Assert.Equal("ExternalFile(/home/test/DbModelGenerator.Test/Scripts/script_4.sql@1:13)",
            diagnostics[2].Location.ToString());


        CollectionAssert.IsEmpty(generatedFiles);
    }

    [Fact]
    public void ErrorsAndGenerateDbModel()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", Config),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_1.sql", WrongCreateTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", CreateTableBrand),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_3.sql", CreateTableUser),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_4.sql", AlterTableBrand)
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        CollectionAssert.IsNotEmpty(diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
        Assert.Equal("DbMG002", diagnostics[0].Id);
        Assert.Equal("ExternalFile(/home/test/DbModelGenerator.Test/Scripts/script_1.sql@12:9)",
            diagnostics[0].Location.ToString());

        CollectionAssert.AreEquivalent(new[]
        {
            "UserDb.cs",
            "BrandDb.cs"
        }, generatedFiles);
    }
}