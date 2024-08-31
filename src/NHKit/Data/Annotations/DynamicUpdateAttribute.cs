using System;

namespace NHKit.Data.Annotations;

/// <summary>
/// Specify whether to use dynamic SQL for update statements.
/// </summary>
/// <remarks>
/// Dynamic update is on by default. Use this attribute to turn it off.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class DynamicUpdateAttribute : Attribute
{
    public bool Enabled { get; }

    public DynamicUpdateAttribute(bool enabled)
    {
        Enabled = enabled;
    }
}