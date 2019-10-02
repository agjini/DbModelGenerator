using Odin.Api.Util;

namespace DbModelGenerator
{
    public sealed class Parameters
    {
        public Parameters(string entityInterface, string primaryKeyAttribute, string autoIncrementAttribute,
            string suffix, string projectPath, string scriptsPath)
        {
            EntityInterface = entityInterface;
            PrimaryKeyAttribute = primaryKeyAttribute;
            AutoIncrementAttribute = autoIncrementAttribute;
            Suffix = suffix;
            ProjectPath = projectPath;
            ScriptsPath = scriptsPath;
        }

        public string EntityInterface { get; }

        public string PrimaryKeyAttribute { get; }

        public string AutoIncrementAttribute { get; }

        public string Suffix { get; }

        public string ScriptsPath { get; }

        public string ProjectPath { get; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}