using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class GenerateDbModel : Task
    {
        [Output] public ITaskItem[] GeneratedFiles { get; private set; }

        public override bool Execute()
        {
            var projectPath = Path.GetDirectoryName(BuildEngine3.ProjectFileOfTaskNode);
            if (projectPath == null)
            {
                throw new ArgumentException("ProjectPath is not defined");
            }

            using (var modelGenerator = new DbModelGenerator())
            {
                var scriptsPath = Path.Combine(projectPath, "Scripts");

                GeneratedFiles = modelGenerator.Generate(projectPath, scriptsPath);
            }

            return true;
        }
    }
}