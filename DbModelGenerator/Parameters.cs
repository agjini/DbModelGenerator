using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using DbModelGenerator.Util;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DbModelGenerator;

public sealed class Parameters(
    ImmutableList<string>? interfaces,
    string? primaryKeyAttribute,
    string? autoIncrementAttribute,
    string? suffix,
    ImmutableList<string>? ignores
)
{
    public static Parameters Default()
    {
        return new Parameters(null, null, null, null, null);
    }


    private static readonly ImmutableList<string> RoslynOptions =
    [
        "ProjectDir", "ScriptsDir"
    ];

    public static ProjectInfo GetProjectInfo(AnalyzerConfigOptionsProvider configOptionsProvider, CancellationToken _)
    {
        var parameters = RoslynOptions
            .Select(parameterName => GetParameter(configOptionsProvider.GlobalOptions, parameterName))
            .ToImmutableDictionary(kv => kv.Name, kv => kv.Value);

        var projectDir = parameters["ProjectDir"];
        if (string.IsNullOrEmpty(projectDir))
        {
            throw new ArgumentException("ProjectDir is not defined");
        }

        var scriptsDirectory = parameters["ScriptsDir"] ?? "Scripts";

        return new ProjectInfo(Path.Combine(projectDir, scriptsDirectory), new DirectoryInfo(projectDir).Name);
    }

    private static (string Name, string? Value) GetParameter(AnalyzerConfigOptions options, string parameterName)
    {
        return (Name: parameterName,
            Value: !options.TryGetValue($"build_property.{parameterName.ToLower()}", out var value)
                ? null
                : value.Replace(",", ";"));
    }

    public ImmutableList<string> Interfaces { get; } = interfaces ?? [];

    public string? PrimaryKeyAttribute { get; } = primaryKeyAttribute;

    public string? AutoIncrementAttribute { get; } = autoIncrementAttribute;

    public string Suffix { get; } = suffix ?? "";

    public ImmutableList<string> Ignores { get; } = ignores ?? [];

    public override string ToString()
    {
        return ToStringHelper.ToString(this);
    }
}

public sealed class ProjectInfo(string scriptPath, string projectName)
{
    public string ScriptPath => scriptPath;
    public string ProjectName => projectName;
}