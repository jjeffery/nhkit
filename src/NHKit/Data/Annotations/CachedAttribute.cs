using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Specifies that  instances of this class should be stored in the second-level
    /// cache as read-only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CachedAttribute : Attribute;
}