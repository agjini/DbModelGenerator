using System;
using Microsoft.CodeAnalysis;

namespace DbModelGenerator;

public sealed class DbModelGeneratorException(Location location, string message) : ArgumentException(message)
{
    public Location Location => location;
}

public static class ErrorUtils
{
    public static Diagnostic MapException(Exception baseException)
        => baseException switch
        {
            DbModelGeneratorException e => Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DbMG001",
                    "Invalid usage",
                    e.Message,
                    "DbModelGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                e.Location
            ),
            _ => Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DbMG999",
                    "Generation problem",
                    baseException.Message,
                    "DbModelGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                null
            )
        };
}