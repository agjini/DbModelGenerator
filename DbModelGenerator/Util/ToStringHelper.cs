using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbModelGenerator.Util
{
    public static class ToStringHelper
    {
        public static string ToString<T>(T t)
        {
            var builder = new ToStringBuilder<T>(t);
            foreach (var property in typeof(T).GetProperties())
            {
                builder.Append(property);
            }

            return builder.ToString();
        }
    }

    public sealed class ToStringBuilder<T>
    {
        private readonly T obj;
        private readonly Type objType;
        private readonly List<string> properties;

        public ToStringBuilder(T obj)
        {
            this.obj = obj;
            objType = obj.GetType();
            properties = new List<string>();
        }

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
}