namespace DbModelGenerator;

public sealed class InputFile(string path, string content)
{
    public string Path => path;

    public string Content => content;
}