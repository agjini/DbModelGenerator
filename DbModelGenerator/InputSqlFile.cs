namespace DbModelGenerator;

public sealed class InputSqlFile(string path, string content)
{
    public string Path => path;

    public string Content => content;
}