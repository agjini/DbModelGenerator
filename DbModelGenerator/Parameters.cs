using System;
using Odin.Api.Util;

namespace DbModelGenerator
{
    public sealed class Parameters
    {
        public Parameters(string entityInterface, string primaryKeyAttribute, string autoIncrementAttribute,
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