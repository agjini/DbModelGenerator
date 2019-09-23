using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class DbModelGenerator
    {
        public static ITaskItem[] Generate(string projectPath, string scriptsPath, string entityInterface,
            string primaryKeyAttribute, string autoIncrementAttribute, TaskLoggingHelper log)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new ArgumentException($"Project '{projectPath}' does not exist !");
            }

            if (!Directory.Exists(scriptsPath))
            {
                throw new ArgumentException($"Project scripts path '{scriptsPath}' does not exist !");
            }

            return Directory.GetDirectories(scriptsPath)
                .Select(d => ReadSchema(d, projectPath, log))
                .SelectMany(d => TemplateGenerator.Generate(d, projectPath, entityInterface, primaryKeyAttribute, autoIncrementAttribute,log))
                .ToArray();
        }

        private static Schema ReadSchema(string scriptDirectory, string projectPath, TaskLoggingHelper log)
        {
            using (var dbSchemaReader = new DbSchemaReader())
            {
                return dbSchemaReader.Read(projectPath, scriptDirectory, log);
            }
        }
    }
}