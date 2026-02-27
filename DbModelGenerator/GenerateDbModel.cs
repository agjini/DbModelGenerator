using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace DbModelGenerator;

[Generator]
public class GenerateDbModel : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var project = initContext.AnalyzerConfigOptionsProvider
            .Select(Parameters.GetProjectInfo);

        var additionalFiles = initContext.AdditionalTextsProvider.Combine(project)
            .Where(providers =>
            {
                var (file, projectInfo) = providers;
                return file.Path.StartsWith(projectInfo.ScriptPath);
            })
            .Select((providers, cancellationToken) =>
            {
                var (file, projectInfo) = providers;
                return new InputFile(
                    $"{file.Path.Replace($"{projectInfo.ScriptPath}/", "")}",
                    file.GetText(cancellationToken)!.ToString());
            })
            .Collect();

        initContext.RegisterSourceOutput(additionalFiles.Combine(project),
            (context, providers) =>
            {
                var (files, projectInfo) = providers;

                try
                {
                    ProcessFiles(projectInfo, context, files);
                }
                catch (Exception e)
                {
                    var diagnostic = ErrorUtils.MapException(e);
                    context.ReportDiagnostic(diagnostic);
                }
            });
    }

    private static void ProcessFiles(ProjectInfo projectInfo, SourceProductionContext context,
        ImmutableArray<InputFile> files)
    {
        var configFile = files
            .Where(f => f.Path.Equals("db.json", StringComparison.InvariantCultureIgnoreCase))
            .Select(f =>
            {
                try
                {
                    return JsonSerializer.Deserialize<Parameters>(f.Content,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch (Exception e)
                {
                    throw new DbModelGeneratorException(
                        Location.Create($"{projectInfo.ScriptPath}/{f.Path}", default, default), e.Message);
                }
            })
            .FirstOrDefault() ?? Parameters.Default();

        var generatedOutput = files
            .Where(f => f.Path.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
            .GroupBy(f => GetParentFolderIfExist(f, projectInfo.ScriptPath))
            .Select(kvp => DbSchemaReader.Read(kvp.Key, kvp))
            .Select(result =>
            {
                var (schema, errors) = result;
                errors.ForEach(error => context.ReportDiagnostic(ErrorUtils.MapException(error)));
                return schema;
            })
            .SelectMany(s => TemplateGenerator.Generate(projectInfo, IgnoreTables(s, configFile.Ignores), configFile))
            .Select(f => (f.Key, f.Value));

        foreach (var (fileName, content) in generatedOutput)
        {
            context.AddSource(fileName, content);
        }
    }

    private static string GetParentFolderIfExist(InputFile f, string projectInfoScriptPath)
    {
        var split = f.Path.Split('/');
        return split.Length > 1 ? split.First() : projectInfoScriptPath;
    }

    private static Schema IgnoreTables(Schema schema, ImmutableList<string> ignores)
    {
        var toIgnore = ignores.Select(i => i.Trim().ToUpper()).ToImmutableHashSet();
        return new Schema(schema.ScriptDirectory, schema.Tables.Where(t => !toIgnore.Contains(t.Name.ToUpper())));
    }
}