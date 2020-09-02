using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class GenerateDbModel : Task
    {
        public string EntityInterface { get; set; }

        public string PrimaryKeyAttribute { get; set; }

        public string AutoIncrementAttribute { get; set; }

        public string Suffix { get; set; }

        public string ScriptsDir { get; set; }

        public string Ignore { get; set; }

        [Output] public ITaskItem[] GeneratedFiles { get; private set; }

        public override bool Execute()
        {
            var projectPath = Path.GetDirectoryName(BuildEngine3.ProjectFileOfTaskNode);
            if (projectPath == null)
            {
                throw new ArgumentException("ProjectPath is not defined");
            }

            var scriptsDirectory = ScriptsDir ?? "Scripts";

            var scriptsPath = Path.Combine(projectPath, scriptsDirectory);

            var parameters = new Parameters(EntityInterface, PrimaryKeyAttribute, AutoIncrementAttribute, Suffix,
                Ignore ?? "", projectPath, scriptsPath);
            Log.LogMessage("GeneraDbModel parameters:\n", parameters);

            GeneratedFiles = DbModelGenerator.Generate(parameters, Log);

            return true;
        }
    }
}