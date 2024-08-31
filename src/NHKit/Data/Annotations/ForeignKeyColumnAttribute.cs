using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NHKit.Data.Annotations;

/// <summary>
/// Specifies the foreign key column name for a collection.
/// </summary>
/// <remarks>
/// Could use the <see cref="ForeignKeyAttribute"/> in
/// System.ComponentModel.DataAnnotations.Schema, but its
/// semantics are a bit different: the name references a property
/// name where here it references a column name.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyColumnAttribute : Attribute
{
    public readonly string Name;

    public ForeignKeyColumnAttribute([NotNull] string name)
    {
        Name = name;
    }
}
