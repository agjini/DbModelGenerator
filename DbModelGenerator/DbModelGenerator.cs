using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace DbModelGenerator
{
    public sealed class DbModelGenerator
    {
        public static ITaskItem[] Generate(string projectPath, string scriptsPath, string identityInterface)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new ArgumentException($"Project '{projectPath}' does not exist !");
            }

            if (!Directory.Exists(scriptsPath))
            {
                throw new ArgumentException($"Project scripts path '{scriptsPath}' does not exist !");
            }

            var templateGenerator = new TemplateGenerator();

            return Directory.GetDirectories(scriptsPath)
                .Select(d => ReadSchema(d, projectPath))
                .SelectMany(d => templateGenerator.Generate(d, projectPath, identityInterface))
                .ToArray();
        }

        private static Schema ReadSchema(string scriptDirectory, string projectPath)
        {
            using (var dbSchemaReader = new DbSchemaReader())
            {
                return dbSchemaReader.Read(projectPath, scriptDirectory);
            }
        }
    }
}