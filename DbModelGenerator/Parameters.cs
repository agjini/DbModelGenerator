using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using DbModelGenerator.Util;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DbModelGenerator
{
    public sealed class Parameters
    {
        private static readonly ImmutableList<string> RoslynOptions =
        [
            "AutoIncrementAttribute", "EntityInterface", "Ignore", "PrimaryKeyAttribute",
            "ProjectDir", "ScriptsDir", "Suffix"
        ];

        public static Parameters LoadParameters(AnalyzerConfigOptionsProvider configOptionsProvider,
            CancellationToken cancellationToken)
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

            var scriptsPath = Path.Combine(projectDir, scriptsDirectory);

            return new Parameters(parameters["EntityInterface"], parameters["PrimaryKeyAttribute"],
                parameters["AutoIncrementAttribute"], parameters["Suffix"], parameters["Ignore"],
                new DirectoryInfo(projectDir).Name, scriptsPath
            );
        }

        private static (string Name, string Value) GetParameter(AnalyzerConfigOptions options, string parameterName)
        {
            return (Name: parameterName,
                Value: !options.TryGetValue($"build_property.{parameterName.ToLower()}", out var value)
                    ? null
                    : value.Replace(",", ";"));
        }

        private Parameters(string entityInterface, string primaryKeyAttribute, string autoIncrementAttribute,
            string suffix, string ignore, string projectPath, string scriptsPath)
        {
            EntityInterface = entityInterface;
            PrimaryKeyAttribute = primaryKeyAttribute;
            AutoIncrementAttribute = autoIncrementAttribute;
            Suffix = suffix;
            ProjectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
            ScriptsPath = scriptsPath ?? throw new ArgumentNullException(nameof(scriptsPath));
            Ignore = ignore;
        }

        public string EntityInterface { get; }

        public string PrimaryKeyAttribute { get; }

        public string AutoIncrementAttribute { get; }

        public string Suffix { get; }

        public string ScriptsPath { get; }

        public string ProjectPath { get; }

        public string Ignore { get; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}