using System.Text;
using System.Text.RegularExpressions;
using DbUp.Engine;

namespace DbModelGenerator.Preprocessor
{
    public sealed class DdlPreprocessor : IScriptPreprocessor
    {
        public string Process(string contents)
        {
            var regex = new Regex(@"((ALTER|DROP|CREATE)\s+(TABLE|INDEX|CONSTRAINT)\s+[^;]*;)", RegexOptions.IgnoreCase);
            var filteredScript = new StringBuilder();
            foreach (Match match in regex.Matches(contents))
            {
                filteredScript.Append(match.Groups[1].Value);
                filteredScript.Append("\n");
            }

            return filteredScript.ToString();
        }
    }
}