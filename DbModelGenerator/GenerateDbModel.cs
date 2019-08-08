using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class GenerateDbModel : Task
    {
        public string IdentityInterface { get; set; }

        public string ScriptsDir { get; set; }

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

            GeneratedFiles = DbModelGenerator.Generate(projectPath, scriptsPath, IdentityInterface);

            return true;
        }
    }
}