using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace NHKit.Data.Types;

/// <summary>
/// Polymorphic models a relationship where the object type is dynamic.
/// Similar to the polymorphic relationship in Active Record (RoR).
/// </summary>
/// <seealso cref="PolymorphicConvention"/>
[Serializable]
public abstract class Polymorphic
{
    public readonly int Id;
    public readonly string Type;
        
    // only used in stateless sessions
    protected Polymorphic(int id, [NotNull] string type, IReadOnlyDictionary<string, Type> allowedTypes)
    {
        if (!allowedTypes.ContainsKey(type)) {
            throw new ArgumentException($"Unknown type: {type}");
        }
        Id = id;
        Type = type;
    }

    public override bool Equals(object obj)
    {
        if (obj is Polymorphic other)
        {
            return other.Id == Id &&
                   other.Type == Type;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Type.GetHashCode() * 13 + Id;
        }
    }
}
