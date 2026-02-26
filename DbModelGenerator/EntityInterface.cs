using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DbModelGenerator;

public class EntityInterface
{
    public EntityInterface(string ns, string name, ImmutableList<(string, bool)> properties)
    {
        Namespace = ns;
        Name = name;
        Properties = properties;
    }

    public string Namespace { get; }
    public string Name { get; }
    public ImmutableList<(string, bool)> Properties { get; }

    public string GetDeclaration(ImmutableList<Column> columns)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.Append($"{Name}");
        var parameters = GetParameters(columns);
        if (parameters != "")
        {
            contentBuilder.Append($"<{parameters}>");
        }

        return contentBuilder.ToString();
    }

    private string GetParameters(ImmutableList<Column> columns)
    {
        var dictionary = columns.ToImmutableDictionary(c => c.Name.ToUpper(), c => c.TypeAsString());
        return string.Join(", ", Properties.Where(p => p.Item2)
            .Select(p => dictionary.GetValueOrDefault(p.Item1.ToUpper())));
    }
}