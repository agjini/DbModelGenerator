using System;
using System.Collections.Generic;
using Sprache;

namespace DbModelGenerator;

public static class OptionExtensions
{
    public static Option<T> ToOption<T>(this T o)
    {
        if (o == null)
        {
            return None<T>.Instance;
        }

        return new Some<T>(o);
    }

    public static Option<T> ToOption<T>(this IOption<T> o)
    {
        if (o.IsDefined)
        {
            return new Some<T>(o.Get());
        }

        return None<T>.Instance;
    }
}

public abstract class Option<T>
{
    public static Option<T> None()
    {
        return None<T>.Instance;
    }

    public abstract bool IsEmpty { get; }

    public abstract T Get();

    protected bool Equals(Option<T> other)
    {
        return IsEmpty == other.IsEmpty;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Option<T>) obj);
    }

    public override int GetHashCode()
    {
        return IsEmpty.GetHashCode();
    }
}

public sealed class Some<T> : Option<T>
{
    private readonly T value;

    public Some(T value)
    {
        this.value = value;
    }

    public override bool IsEmpty => false;

    public override T Get()
    {
        return value;
    }

    private bool Equals(Some<T> other)
    {
        return EqualityComparer<T>.Default.Equals(value, other.value);
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is Some<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(value);
    }
}

public sealed class None<T> : Option<T>
{
    public override bool IsEmpty => true;

    internal static readonly None<T> Instance = new None<T>();

    private None()
    {
    }

    public override T Get()
    {
        throw new InvalidOperationException("Cannot get value from None.");
    }
}