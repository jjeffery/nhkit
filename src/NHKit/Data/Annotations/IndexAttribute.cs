using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that a non-unique index should be created for the column associated
    /// with this property. A multi-column index can be created by specifying
    /// the same index name for multiple properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public readonly string Name;

        public IndexAttribute(string name = null)
        {
            Name = name;
        }
    }
}