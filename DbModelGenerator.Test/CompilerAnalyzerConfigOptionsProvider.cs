using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DbModelGenerator.Test;

public static class CompilerAnalyzerConfigOptionsProvider
{
    public static AnalyzerConfigOptionsProvider Empty { get; } = new EmptyAnalyzerConfigOptionsProvider();

    public static AnalyzerConfigOptionsProvider WithOptions(Dictionary<string, string> options)
    {
        return new TestAnalyzerConfigOptionsProvider(options);
    }

    private class EmptyAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GlobalOptions => new EmptyAnalyzerConfigOptions();

        private class EmptyAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value) => throw new NotSupportedException();
        }
    }

    private class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GlobalOptions => new TestAnalyzerConfigOptions(options);

        private class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value)
            {
                return options.TryGetValue(key, out value);
            }
        }
    }
}
