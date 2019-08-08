using System.Collections.Generic;

namespace DbModelGenerator
{
    public sealed class Schema
    {
        public Schema(string scriptDirectory, IEnumerable<Table> tables)
        {
            ScriptDirectory = scriptDirectory;
            Tables = tables;
        }

        public string ScriptDirectory { get; }

        public IEnumerable<Table> Tables { get; }
    }
}