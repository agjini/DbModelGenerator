using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
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

    private const string CreateTable = @"
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

    private const string InsertTable = @"
        INSERT INTO country (id, name, code, default_currency_id)
        VALUES (1, 'Belgium', 'BE', '1'),
               (2, 'France', '$TEST_VAR$', '1'),
               (4, 'Luxembourg', 'LU', '1'),
               (5, 'Netherlands', 'NL', '1');
    ";

    private const string DropTableIfExists = "DROP TABLE IF EXISTS tenant_saml;";

    private GeneratorDriver _driver;

    public SourceGeneratorTest()
    {
        var generator = new GenerateDbModel();
        var options = new Dictionary<string, string>
        {
            ["build_property.projectdir"] = "/home/test/DbModelGenerator.Test/"
        };
        _driver = CSharpGeneratorDriver.Create(generator).WithUpdatedAnalyzerConfigOptions(CompilerAnalyzerConfigOptionsProvider.WithOptions(options));
    }

    [Fact]
    public void GenerateDbModel()
    {
        _driver = _driver.AddAdditionalTexts(
            [
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/db.json", Config),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_1.sql", CreateTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_2.sql", InsertTable),
                new TestAdditionalFile("/home/test/DbModelGenerator.Test/Scripts/script_3.sql", DropTableIfExists)
            ]
        );

        var compilation = CSharpCompilation.Create(nameof(GenerateDbModel));

        _driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        var generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        Assert.Equivalent(new[]
        {
            "BrandDb.cs"
        }, generatedFiles);
    }
}