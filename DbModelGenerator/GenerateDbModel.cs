using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DbModelGenerator;

// Ajouter logs
// var diagnostic = Diagnostic.Create(
//     new DiagnosticDescriptor(
//         "SG001",
//         "Invalid usage",
//         "Class {0} must have a parameterless constructor",
//         "SourceGenerator",
//         DiagnosticSeverity.Error,
//         isEnabledByDefault: true),
//     classDecl.GetLocation(),
//     classSymbol.Name);
// 
// context.ReportDiagnostic(diagnostic);

// AJOUTER CELA AU PROJET POUR LES FICHIERS À INCLURE (EN PLUS D'EMBEDDED)
// <ItemGroup>
//     <AdditionalFiles Include="*.sql" />
// </ItemGroup>

[Generator]
public class GenerateDbModel : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
    {
        var parameters = initializationContext.AnalyzerConfigOptionsProvider
            .Select(Parameters.LoadParameters);

        var contents = initializationContext.AdditionalTextsProvider.Combine(parameters)
            .Where(providers =>
            {
                var (file, projectParameters) = providers;
                return IsSqlFile(file, projectParameters.ScriptsPath);
            })
            .Select((providers, cancellationToken) => new InputSqlFile(
                $"{providers.Left.Path.Replace($"{providers.Right.ScriptsPath}/", "")}",
                providers.Left.GetText(cancellationToken)!.ToString()))
            .Collect();

        initializationContext.RegisterSourceOutput(parameters.Combine(contents),
            (sourceProductionContext, providers) =>
            {
                var (projectParameters, fileList) = providers;

                var generatedOutput = fileList.GroupBy(f => f.Path.Split('/').First())
                    .Select(kvp => DbSchemaReader.Read(kvp.Key, kvp))
                    .Select(s =>
                        TemplateGenerator.Generate(IgnoreTables(s, projectParameters.Ignore), projectParameters));

                foreach (var folderContent in generatedOutput)
                {
                    foreach (var (fileName, content) in folderContent.Select(f => (f.Key, f.Value)))
                    {
                        sourceProductionContext.AddSource(fileName, content);
                    }
                }
            });
    }

    private static bool IsSqlFile(AdditionalText file, string scriptsPath) => file.Path.EndsWith(".sql")
                                                                              && file.Path.Contains(scriptsPath);

    private static Schema IgnoreTables(Schema schema, string ignore)
    {
        var toIgnore = ignore.Split(',').Select(i => i.Trim().ToUpper()).ToImmutableHashSet();
        return new Schema(schema.ScriptDirectory, schema.Tables.Where(t => !toIgnore.Contains(t.Name.ToUpper())));
    }
}