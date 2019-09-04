using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class GenerateDbModel : Task
    {
        public string PrimaryKeyAttribute { get; set; }

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

            Log.LogMessage("ProjectPath : " + projectPath);
            Log.LogMessage("PrimaryKeyAttribute : " + PrimaryKeyAttribute);
            Log.LogMessage("ScriptsPath : " + scriptsPath);
            
            GeneratedFiles = DbModelGenerator.Generate(projectPath, scriptsPath, PrimaryKeyAttribute, Log);

            return true;
        }
    }
}