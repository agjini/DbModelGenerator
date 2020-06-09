using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DbModelGenerator
{
    public sealed class DbModelGenerator
    {
        public static ITaskItem[] Generate(Parameters parameters, TaskLoggingHelper log)
        {
            if (!Directory.Exists(parameters.ProjectPath))
            {
                throw new ArgumentException($"Project '{parameters.ProjectPath}' does not exist !");
            }

            if (!Directory.Exists(parameters.ScriptsPath))
            {
                throw new ArgumentException($"Project scripts path '{parameters.ScriptsPath}' does not exist !");
            }

            return Directory.GetDirectories(parameters.ScriptsPath)
                .Select(d => ReadSchema(d, log))
                .SelectMany(d => TemplateGenerator.Generate(d, parameters, log))
                .ToArray();
        }

        public static Schema ReadSchema(string scriptDirectory, TaskLoggingHelper log)
        {
            using (var dbSchemaReader = new DbSchemaReader())
            {
                return dbSchemaReader.Read(scriptDirectory, log);
            }
        }
    }
}