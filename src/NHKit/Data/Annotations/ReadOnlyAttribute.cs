using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that the property or class is read-only. NHibernate will not save changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadOnlyAttribute : Attribute;
}
