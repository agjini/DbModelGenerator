using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbModelGenerator.Util;

public static class ToStringHelper
{
    public static string ToString<T>(T t)
        where T : notnull
    {
        var builder = new ToStringBuilder<T>(t);
        foreach (var property in typeof(T).GetProperties())
        {
            builder.Append(property);
        }

        return builder.ToString();
    }
}

public sealed class ToStringBuilder<T>(T obj)
    where T : notnull
{
    private readonly Type objType = obj.GetType();
    private readonly List<string> properties = new();

    public void Append(PropertyInfo propertyInfo)
    {
        var propertyName = propertyInfo.Name;

        var value = propertyInfo.GetValue(obj);

        properties.Add($"{propertyName}: {value}");
    }

    public override string ToString()
    {
        return objType.Name + "{" + string.Join(", ", properties) + "}";
    }
}