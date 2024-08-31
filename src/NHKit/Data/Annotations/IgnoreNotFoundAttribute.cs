using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Apply to entities that might not be found.
    /// </summary>
    /// <remarks>
    /// WARNING: This has a severe impact on performance.
    /// Consider using <see cref="Dangling{T}"/> or even better,
    /// find a way to clean up the data on a permanent basis.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property)]
    public class IgnoreNotFoundAttribute : Attribute;
}