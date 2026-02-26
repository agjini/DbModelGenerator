using System.Collections.Generic;

namespace DbModelGenerator;

public sealed class Schema(string scriptDirectory, IEnumerable<Table> tables)
{
    public string ScriptDirectory { get; } = scriptDirectory;

    public IEnumerable<Table> Tables { get; } = tables;
}